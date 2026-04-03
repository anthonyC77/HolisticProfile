namespace HolisticProfile.Infrastructure.LlmClients;

public class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model   { get; set; } = "llama3.1:8b-instruct-q4_0";
}
