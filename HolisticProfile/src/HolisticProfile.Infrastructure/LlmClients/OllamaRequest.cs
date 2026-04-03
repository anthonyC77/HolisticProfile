using System.Text.Json.Serialization;

namespace HolisticProfile.Infrastructure.LlmClients;

internal record OllamaRequest(
    [property: JsonPropertyName("model")]  string Model,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("stream")] bool Stream = false
);
