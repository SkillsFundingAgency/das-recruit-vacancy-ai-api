using System.Text.Json.Serialization;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class JsonPrompt
{
    [JsonPropertyName("SYSTEM_PROMPT")]
    public string? SystemPrompt { get; set; }
    [JsonPropertyName("USER_HEADER")]
    public string? UserHeader { get; set; }
    [JsonPropertyName("USER_INSTRUCTIONS")]
    public string? UserInstructions { get; set; }
}