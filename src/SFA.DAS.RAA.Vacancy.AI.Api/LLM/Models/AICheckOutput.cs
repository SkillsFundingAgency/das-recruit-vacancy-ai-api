namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class AICheckOutput
{
    public AICheckOutput(bool checkval = false,string llmdebug="", string checkname = "")
    {
        Name = checkname;
        Value = checkval;
        LLMOutput = llmdebug;
    }
    public bool Value { get; set; }
    public string Name { get; set; } = "";
    public string LLMOutput { get; set; } = "";
}