using System.Text.Json.Serialization;

namespace HolisticProfile.Infrastructure.LlmClients;

internal record OllamaResponse(
    [property: JsonPropertyName("model")]          string Model,
    [property: JsonPropertyName("response")]       string Response,
    [property: JsonPropertyName("done")]           bool Done,
    [property: JsonPropertyName("total_duration")] long TotalDuration = 0,
    [property: JsonPropertyName("eval_count")]     int EvalCount = 0
);
