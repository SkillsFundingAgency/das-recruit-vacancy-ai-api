namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class LLMReturnResult {
    public string LLMResponse { get; set; } = "";
    public bool LLMErrorFlag { get; set; } = false;
    public ErrorReturnObject Error { get; set; }
}