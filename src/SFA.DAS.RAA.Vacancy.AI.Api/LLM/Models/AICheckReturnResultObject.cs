namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class AICheckReturnResultObject
{
    public string? VacancyID { get; set; } = "";
    public List<AICheckOutput>? AICheckOutput { get; set; } = [];
    public List<AICheckOutput>? DebugAICheckOutput { get; set; } = [];
    public TrafficLight? TrafficLightScore { get; set; } = new(-1);
        
    public bool? RecommendReview { get; set; } = false;
    public List<ErrorReturnObject> Errors { get; set; } = []; 
}