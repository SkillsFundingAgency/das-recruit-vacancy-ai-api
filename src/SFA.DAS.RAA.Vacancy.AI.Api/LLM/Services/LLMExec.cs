using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework.Internal;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public interface ILLMExec
{
    Task<AICheckReturnResultObject> ExecLLM(InputObject vacancyInput);
}
public class LLMExec(ILogger<LLMExec> logger,IVacancyQA qa, IOptions<VacancyAiConfiguration> configuration) : ILLMExec
{
    public async Task<AICheckReturnResultObject> ExecLLM(InputObject vacancyInput) // simple LLM output returns a battery of tests
    {
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
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
        GetCheckLlmResult(vacancyInput.VacancyFull, llmerrors, aichecks_shortlist, "DiscriminationCheck",config.DiscriminationPrompt, config.Temperature_Discrimination),
        GetCheckLlmResult(vacancyInput.VacancyFull, llmerrors, aichecks_shortlist, "TextInconsistencyCheck",config.MissingContentPrompt, config.Temperature_MissingContent));


        var spellingAndGrammarChecks = new ConcurrentBag<AICheckOutput>();
        var tasks = spellingAndGrammarInputCheck
            .Select(key => GetCheckLlmResult(key.Value, llmerrors, spellingAndGrammarChecks, $"Spelling Check {key.Key}", config.SpellingCheckPrompt, config.Temperature_SpellCheck))
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
        
        sw.Stop();

        float process_runtime = sw.ElapsedMilliseconds / 1000.0F; // convert time to seconds
        logger.LogDebug("Full LLM checks(s) processed in " + process_runtime.ToString() + " seconds");
        TrafficLight trafdbg = prioritisationSystem.TrafficLightAssignment(aichecks_shortlist.ToList());
   
        bool reqreview = reviewAllocator.Allocator(prioritisationSystem.TrafficLightAssignment(aichecks_shortlist.ToList()));

        List<AICheckOutput> checklist_dbg=aichecks_shortlist.Concat(spellingChecks.Checks).ToList();
        foreach (AICheckOutput dbg_chk in checklist_dbg) {            
            logger.LogDebug("LLM DEBUG {name}, Value: {value}",dbg_chk.Name, dbg_chk.Value);
            logger.LogDebug("LLM DEBUG {name}, LLM Output: {value}",dbg_chk.Name, dbg_chk.LLMOutput);
        }
        logger.LogDebug("Traffic light assignment: " + trafdbg.TrafficLightRatingSystemDescription.ToString());
        logger.LogDebug("Recommend review?: " + reqreview.ToString());
        
        return new AICheckReturnResultObject
        {
            DebugAICheckOutput = aichecks_shortlist.Concat(spellingChecks.Checks).ToList(),
            AICheckOutput = aichecks_shortlist.ToList(),
            VacancyID = vacancyInput.VacancyId ?? "-",
            TrafficLightScore = prioritisationSystem.TrafficLightAssignment(aichecks_shortlist.ToList()),
            RecommendReview = reviewAllocator.Allocator(prioritisationSystem.TrafficLightAssignment(aichecks_shortlist.ToList())),
            Errors = llmerrors.ToList(),
            Job_Runtime = process_runtime
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
        aichecksShortlist.Add(new AICheckOutput(qa.FlagifyLLMResponse(llmOutput.LLMResponse, false, false), llmOutput.LLMResponse, checkName,llmOutput.CheckRuntime));
    }
}