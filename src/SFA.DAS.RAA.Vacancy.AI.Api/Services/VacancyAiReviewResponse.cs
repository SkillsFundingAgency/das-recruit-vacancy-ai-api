using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public class VacancyAiReviewResponse
{
    public int Version { get; init; }
    public required bool ManualReviewRequired { get; set; }
    public required AiReviewStatus Status { get; set; }
    
    public AzureAiResponse<Dictionary<string, string?>>? SpellcheckResult { get; init; }
    public AzureAiResponse<Dictionary<string, string?>>? DiscriminationResult { get; init; }
    public AzureAiResponse<Dictionary<string, string?>>? ContentEvaluationResult { get; init; }

    // temp fields to implement
    public AzureAiResponse<Dictionary<string, string?>>? ContentEvalTaskCopilotOptResult { get; init; }
    public AzureAiResponse<Dictionary<string, string?>>? ContentEvalTaskManualOptResult { get; init; }

    public AzureAiResponse<Dictionary<string,string?>>? SpellingTaskManualOpt{ get; init; }

    public AzureAiResponse<Dictionary<string,string?>>? DiscrimTaskManualOpt { get; set; }
    
    public List<ReviewError>? Errors { get; init; }
}