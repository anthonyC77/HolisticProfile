using HolisticProfile.Core.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace HolisticProfile.Infrastructure.LlmClients;

public class OllamaClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OllamaClient(HttpClient httpClient, string model)
    {
        _httpClient = httpClient;
        _model = model;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new OllamaRequest(_model, prompt, Stream: false);

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request, JsonOptions, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Ollama a retourné une réponse vide.");

        return result.Response;
    }
}
