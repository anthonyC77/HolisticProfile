using FluentAssertions;
using HolisticProfile.Infrastructure.LlmClients;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HolisticProfile.Infrastructure.Tests.LlmClients;

/// <summary>
/// Fake handler testable : override SendAsync (protected) sans NSubstitute.
/// Capture la requête pour assertions, retourne une réponse configurable.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseBody;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content is not null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
        };
    }
}

public class OllamaClientTests
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static string OkJson(string responseText) =>
        JsonSerializer.Serialize(new { model = "mistral", response = responseText, done = true }, JsonOpts);

    private static (OllamaClient client, FakeHttpMessageHandler handler) BuildClient(
        HttpStatusCode statusCode, string body)
    {
        var handler = new FakeHttpMessageHandler(statusCode, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") };
        return (new OllamaClient(httpClient, "mistral"), handler);
    }

    [Fact]
    public async Task GenerateAsync_ValidResponse_ReturnsResponseText()
    {
        var expected = "Voici ta synthèse personnalisée...";
        var (client, _) = BuildClient(HttpStatusCode.OK, OkJson(expected));

        var result = await client.GenerateAsync("un prompt");

        result.Should().Be(expected);
    }

    [Fact]
    public async Task GenerateAsync_ServerError_ThrowsHttpRequestException()
    {
        var (client, _) = BuildClient(HttpStatusCode.InternalServerError, "erreur serveur");

        var act = () => client.GenerateAsync("un prompt");

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GenerateAsync_SendsRequestToCorrectEndpoint()
    {
        var (client, handler) = BuildClient(HttpStatusCode.OK, OkJson("ok"));

        await client.GenerateAsync("prompt");

        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/api/generate");
    }

    [Fact]
    public async Task GenerateAsync_PayloadContainsModelAndPrompt()
    {
        var (client, handler) = BuildClient(HttpStatusCode.OK, OkJson("ok"));

        await client.GenerateAsync("mon prompt de test");

        handler.LastRequestBody.Should().Contain("mistral");
        handler.LastRequestBody.Should().Contain("mon prompt de test");
    }

    [Fact]
    public async Task GenerateAsync_PayloadHasStreamFalse()
    {
        var (client, handler) = BuildClient(HttpStatusCode.OK, OkJson("ok"));

        await client.GenerateAsync("prompt");

        handler.LastRequestBody.Should().Contain("\"stream\":false");
    }
}
