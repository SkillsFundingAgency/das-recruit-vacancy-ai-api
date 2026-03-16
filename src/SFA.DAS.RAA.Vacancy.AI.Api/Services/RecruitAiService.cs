using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public interface IRecruitAiService
{
    Task<VacancyAiReviewResponse> ReviewVacancyAsync(PostPerformReviewDto data, CancellationToken cancellationToken);
}

public class RecruitAiService(
    VacancyAiConfiguration configuration,
    IAiReviewResultChecker checker,
    IAzureAiClient azureAiClient): IRecruitAiService
{
    public async Task<VacancyAiReviewResponse> ReviewVacancyAsync(PostPerformReviewDto data, CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, string?>
            {
                ["Title"] = data.Title,
                ["ShortDescription"] = data.ShortDescription,
                ["Description"] = data.Description,
                ["EmployerDescription"] = data.EmployerDescription,
                ["ThingsToConsider"] = data.ThingsToConsider,
                ["TrainingDescription"] = data.TrainingDescription,
                ["AdditionalTrainingDescription"] = data.AdditionalTrainingDescription,
                ["TrainingProgrammeTitle"] = data.TrainingProgrammeTitle,
                ["TrainingProgrammeLevel"] = data.TrainingProgrammeLevel,
                ["OutcomeDescription"] = data.OutcomeDescription,
                ["ApplicationInstructions"] = data.ApplicationInstructions,
                ["AdditionalQuestion1"] = data.AdditionalQuestion1,
                ["AdditionalQuestion2"] = data.AdditionalQuestion2,
                ["WageAdditionalInformation"] = data.WageAdditionalInformation,
                ["WageCompanyBenefitsInformation"] = data.WageCompanyBenefitsInformation,
                ["WageWorkingWeekDescription"] = data.WageWorkingWeekDescription,
            }
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value!);

        var spellcheckPrompt = new AzureAiClientPrompt(configuration.SpellingCheckPrompt.SystemPrompt, configuration.SpellingCheckPrompt.UserHeader, configuration.SpellingCheckPrompt.UserInstruction);
        var discriminationPrompt = new AzureAiClientPrompt(configuration.DiscriminationPrompt.SystemPrompt, configuration.DiscriminationPrompt.UserHeader, configuration.DiscriminationPrompt.UserInstruction);
        var contentEvaluationPrompt = new AzureAiClientPrompt(configuration.MissingContentPrompt.SystemPrompt, configuration.MissingContentPrompt.UserHeader, configuration.MissingContentPrompt.UserInstruction);
        
        var spellcheckTask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(spellcheckPrompt, fields, cancellationToken);
        var discriminationTask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(discriminationPrompt, fields, cancellationToken);
        var contentEvaluationTask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(contentEvaluationPrompt, fields, cancellationToken);
        
        await Task.WhenAll(spellcheckTask, discriminationTask, contentEvaluationTask);
        return CreateResponse(fields, spellcheckTask.Result, discriminationTask.Result, contentEvaluationTask.Result);
    }

    private VacancyAiReviewResponse CreateResponse(
        Dictionary<string, string> fields,
        AzureAiResponse<Dictionary<string, string?>> spellcheckResult,
        AzureAiResponse<Dictionary<string, string?>> discriminationResult,
        AzureAiResponse<Dictionary<string, string?>> contentEvaluationResult)
    {
        var (status, manualReviewRequired, errors) = checker.AssessResponse(fields, spellcheckResult, discriminationResult, contentEvaluationResult);
        
        return new VacancyAiReviewResponse
        {
            Version = 1,
            ManualReviewRequired = manualReviewRequired,
            Status = status,
            SpellcheckResult = spellcheckResult,
            DiscriminationResult = discriminationResult,
            ContentEvaluationResult = contentEvaluationResult,
            Errors = errors,
        };
    }
}