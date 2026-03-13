using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public interface IRecruitAiService
{
    Task<AiReviewResultV1> ReviewVacancyAsync(PostPerformReviewDto data, CancellationToken cancellationToken);
}

public class RecruitAiService(
    VacancyAiConfiguration configuration,
    IAzureAiClient azureAiClient): IRecruitAiService
{
    public async Task<AiReviewResultV1> ReviewVacancyAsync(PostPerformReviewDto data, CancellationToken cancellationToken)
    {
        var spellcheckFields = new Dictionary<string, string>
        {
            ["AdditionalTrainingDescription"] = data.AdditionalTrainingDescription,
            ["Description"] = data.Description,
            ["EmployerDescription"] = data.EmployerDescription,
            ["ShortDescription"] = data.ShortDescription,
            ["Title"] = data.Title,
            ["TrainingDescription"] = data.TrainingDescription,
        };

        var fieldsToCheck = new Dictionary<string, string>(spellcheckFields)
        {
            ["TrainingProgrammeTitle"] = data.TrainingProgrammeTitle,
            ["TrainingProgrammeLevel"] = data.TrainingProgrammeLevel,
            ["ThingsToConsider"] = data.ThingsToConsider,
        };

        var spellcheckPrompt = new AzureAiClientPrompt(configuration.SpellingCheckPrompt.SystemPrompt, configuration.SpellingCheckPrompt.UserHeader, configuration.SpellingCheckPrompt.UserInstruction);
        var discriminationPrompt = new AzureAiClientPrompt(configuration.DiscriminationPrompt.SystemPrompt, configuration.DiscriminationPrompt.UserHeader, configuration.DiscriminationPrompt.UserInstruction);
        var contentEvaluationPrompt = new AzureAiClientPrompt(configuration.MissingContentPrompt.SystemPrompt, configuration.MissingContentPrompt.UserHeader, configuration.MissingContentPrompt.UserInstruction);
        
        var spellcheckTask = azureAiClient.PerformCheckAsync<Dictionary<string, string>>(spellcheckPrompt, spellcheckFields, cancellationToken);
        var discriminationTask = azureAiClient.PerformCheckAsync<Dictionary<string, string>>(discriminationPrompt, fieldsToCheck, cancellationToken);
        var contentEvaluationTask = azureAiClient.PerformCheckAsync<Dictionary<string, string>>(contentEvaluationPrompt, fieldsToCheck, cancellationToken);
        
        await Task.WhenAll(spellcheckTask, discriminationTask, contentEvaluationTask);

        return new AiReviewResultV1
        {
            SpellcheckResult = spellcheckTask.Result,
            DiscriminationResult = discriminationTask.Result,
            ContentEvaluationResult = contentEvaluationTask.Result,
        };
    }
}