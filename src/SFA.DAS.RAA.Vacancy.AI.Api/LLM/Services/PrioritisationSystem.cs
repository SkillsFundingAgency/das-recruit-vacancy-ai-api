using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public class PrioritisationSystem 
{
    public TrafficLight TrafficLightAssignment(List<AICheckOutput> checklist) 
    { 
        foreach (var check in checklist.Where(c=>c.Value))
        { 
            if(check.Name.Contains("Discrimination", StringComparison.CurrentCultureIgnoreCase))
            {
                return new TrafficLight(3);                    
            }
            if (check.Name.Contains("TextInconsistencyCheck", StringComparison.CurrentCultureIgnoreCase)) 
            {
                return new TrafficLight(3);
            }
            if (check.Name.Contains("Spelling", StringComparison.CurrentCultureIgnoreCase)) 
            {
                return new TrafficLight(2);                    
            }
        }
        // if it does not flag at any point, then we're OK to mark as green
        return new TrafficLight(1);

    }
}