using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public class PrioritisationSystem {
        
    public TrafficLight TrafficLightAssignment(List<AICheckOutput> checklist) 
    { 
        foreach (var check in checklist)
        { 
            if(check.Name.Contains("Discrimination") && check.Value)
            {                    
                TrafficLight traf = new(3);
                return traf;                    
            }
            if (check.Name.Contains("TextInconsistencyCheck") && check.Value) 
            {                    
                TrafficLight traf1 = new(3);
                return traf1;
            }
            if (check.Name.Contains("Spelling") && check.Value) 
            {                    
                TrafficLight traf2 = new(2);
                return traf2;                    
            }
        }
        // if it does not flag at any point, then we're OK to mark as green
        TrafficLight traf3 = new(1);
        return traf3;

    }
}