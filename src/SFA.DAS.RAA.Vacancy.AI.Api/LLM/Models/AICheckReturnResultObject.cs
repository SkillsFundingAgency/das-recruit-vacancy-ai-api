using Microsoft.EntityFrameworkCore;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using System.Net;

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

public class AILLMResultObject {
    public string VacancyId { get; set; }
    public string VacancyReviewId { get; set; }
    public string LLMObject { get; set; } = "";
    public string ReviewStatus { get; set; } = "";
    public string Status { get; set; } = "";
    public string UpdatedDateTime { get; set; } = "";
    public string ManualReviewRequired {get;set;}="";

}
