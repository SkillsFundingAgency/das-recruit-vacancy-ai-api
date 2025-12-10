using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public class SpellingChecks
{
    public List<AICheckOutput> Checks { get; set; } = [];
    public AICheckOutput EvaluateAllSpellingChecks()
    {
        var retobj = new AICheckOutput(false,"-", "SpellingCheck_AllFields");
        if(Checks.Any(spchk => spchk.Value))
        {
            retobj.Value = true;
        }
        return retobj;
    }
}