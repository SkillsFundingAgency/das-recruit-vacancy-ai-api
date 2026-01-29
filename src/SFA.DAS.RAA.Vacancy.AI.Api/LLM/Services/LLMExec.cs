using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public interface ILLMExec
{
    Task<AICheckReturnResultObject> ExecLLM(InputObject vacancyInput);
}
public class LLMExec(IVacancyQA qa, IOptions<VacancyAiConfiguration> configuration) : ILLMExec
{
        
    public async Task<AICheckReturnResultObject> ExecLLM(InputObject vacancyInput) // simple LLM output returns a battery of tests
    {
        var spellingAndGrammarInputCheck=new Dictionary<string, string>
        {
            { "Description", vacancyInput.Description ?? "=" },
            { "ShortDescription", vacancyInput.ShortDescription ?? "" },
            { "Qualifications", vacancyInput.Qualifications??"-" },
            { "Skills", vacancyInput.Skills??"-" },
            { "Title", vacancyInput.Title??"-" },
            { "EmployerDescription", vacancyInput.EmployerDescription??"-" },
            { "TrainingDesiption", vacancyInput.TrainingDescription??"-" },
            { "AdditionalTrainingDescription", vacancyInput.AdditionalTrainingDescription??"-" }
        };
        var config = configuration.Value;

        var llmerrors = new ConcurrentBag<ErrorReturnObject>();
        var aichecks_shortlist = new ConcurrentBag<AICheckOutput>();

        await Task.WhenAll(
        GetCheckLlmResult(vacancyInput.VacancyFull, llmerrors, aichecks_shortlist, "DiscriminationCheck",config.DiscriminationPrompt, config.Temperature_discrimination),
        GetCheckLlmResult(vacancyInput.VacancyFull, llmerrors, aichecks_shortlist, "TextInconsistencyCheck",config.MissingContentPrompt, config.Temperature_missingcontent));


        var spellingAndGrammarChecks = new ConcurrentBag<AICheckOutput>();
        var tasks = spellingAndGrammarInputCheck
            .Select(key => GetCheckLlmResult(key.Value, llmerrors, spellingAndGrammarChecks, $"Spelling Check {key.Key}", config.SpellingCheckPrompt, config.Temperature_spellcheck))
            .ToList();
        
        await Task.WhenAll(tasks);

        var spellingChecks = new SpellingChecks
        {
            Checks = spellingAndGrammarChecks.ToList()
        };

            
        aichecks_shortlist.Add(spellingChecks.EvaluateAllSpellingChecks());

        // initialize the traffic light system & Allocation system
        var prioritisationSystem = new PrioritisationSystem();
        var reviewAllocator = new ReviewAllocator();

        return new AICheckReturnResultObject
        {
            DebugAICheckOutput = aichecks_shortlist.Concat(spellingChecks.Checks).ToList(),
            AICheckOutput = aichecks_shortlist.ToList(),
            VacancyID = vacancyInput.VacancyId ?? "-",
            TrafficLightScore = prioritisationSystem.TrafficLightAssignment(aichecks_shortlist.ToList()),
            RecommendReview = reviewAllocator.Allocator(prioritisationSystem.TrafficLightAssignment(aichecks_shortlist.ToList())),
            Errors=llmerrors.ToList()
        };
    }

    private async Task GetCheckLlmResult(string? input,
        ConcurrentBag<ErrorReturnObject> llmerrors, ConcurrentBag<AICheckOutput> aichecksShortlist, string checkName, Prompt prompt,float temperature)
    {
        var llmOutput = await qa.CallLLM(
            prompt.SystemPrompt,
            prompt.UserHeader,
            prompt.UserInstruction,
            input ?? " ",
            checkName,
            temperature
        );

        if (llmOutput.LLMErrorFlag) {
            llmerrors.Add(llmOutput.Error);
        }
        aichecksShortlist.Add(new AICheckOutput(qa.FlagifyLLMResponse(llmOutput.LLMResponse, false, false), llmOutput.LLMResponse, checkName));
    }
}