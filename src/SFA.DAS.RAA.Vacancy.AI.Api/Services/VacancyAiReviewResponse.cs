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
    public AzureAiResponse<Dictionary<string, string?>>? ContentEvalTaskCorrResult { get; init; }

    public AzureAiResponse<Dictionary<string,string?>>? SpellingTaskManualOpt{ get; init; }
    public AzureAiResponse<Dictionary<string, string?>>? SpellingTaskCorrResult { get; init; }


    public AzureAiResponse<Dictionary<string,string?>>? DiscrimTaskManualOpt { get; set; }
    
    public List<ReviewError>? Errors { get; init; }


    // Dummy dump object for new prompt system.
    public bool ManualReviewRequired_newconfig { get; set; } = false;
    public AiReviewStatus? Status_newconfig { get; set; }
    public List<ReviewError>? Errors_newconfig { get; init; }


    public bool ManualReviewRequired_mixedconfig { get; set; } = false;
    public AiReviewStatus? Status_mixedconfig { get; set; }
    public List<ReviewError>? Errors_mixedconfig { get; init; }

    public bool ManualReviewRequired_corrconfig { get; set; } = false;
    public AiReviewStatus? Status_corrconfig { get; set; }
    public List<ReviewError>? Errors_corrconfig { get; init; }
}