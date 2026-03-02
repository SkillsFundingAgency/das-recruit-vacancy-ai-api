namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class AICheckOutput
{
    public AICheckOutput(bool checkval = false,string llmdebug="", string checkname = "",float runtime=-1.0F)
    {
        Name = checkname;
        Value = checkval;
        LLMOutput = llmdebug;
        ExecutionTime = runtime;
    }
    public bool Value { get; set; }
    public string Name { get; set; } = "";
    public string LLMOutput { get; set; } = "";
    public float ExecutionTime { get; set; } = -1.0F;
}