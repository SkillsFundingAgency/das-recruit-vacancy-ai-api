using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;
using System.ClientModel;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public interface IRecruitAiService
{
    Task<AiReviewResultV1> ReviewVacancyAsync(InputObject data, CancellationToken cancellationToken);
}

public class RecruitAiService(
    VacancyAiConfiguration configuration,
    IAzureAiClient azureAiClient, IAzureAIClientSpellcheckVerifier spellchecker): IRecruitAiService
{
    public record Spchk_Result(string input,object Value);
    public async Task<AiReviewResultV1> ReviewVacancyAsync(InputObject data, CancellationToken cancellationToken)
    {
        var spellcheckFields = new Dictionary<string, string>
        {
            ["AdditionalTrainingDescription"] = data.AdditionalTrainingDescription,
            ["Description"] = data.Description,
            ["EmployerDescription"] = data.EmployerDescription,
            ["Qualifications"] = data.Qualifications,
            ["ShortDescription"] = data.ShortDescription,
            ["Skills"] = data.Skills,
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


        // queue up N spelling and grammar checks for us to spit out / process as well
        List<Task<AzureAiResponse<Dictionary<string, string>>>> List_spellchecks = [];        
        foreach (string k in spellcheckFields.Keys) {
            //field_to_check=
            string spellfield_name = k;
            string spellcheck_data = spellcheckFields[k];
   
            Task<AzureAiResponse<Dictionary<string, string>>> spag_task =spellchecker.PerformCustomSpellcheck<Dictionary<string,string>>(
                spellcheckPrompt,
                spellcheck_data, 
                spellfield_name,
                cancellationToken
                );

            List_spellchecks.Add(spag_task);
        }
        await Task.WhenAll(List_spellchecks);

        List < AzureAiResponse < Dictionary<string, string>>> List_SpellcheckResults = [];
        foreach (Task<AzureAiResponse<Dictionary<string, string>>> x in List_spellchecks) {
            List_SpellcheckResults.Add(x.Result);
        }


        return new AiReviewResultV1
        {
            SpellcheckResult = spellcheckTask.Result,
            DiscriminationResult = discriminationTask.Result,
            ContentEvaluationResult = contentEvaluationTask.Result,
            RetrySpellChecks=List_SpellcheckResults
            
        };
    }
}