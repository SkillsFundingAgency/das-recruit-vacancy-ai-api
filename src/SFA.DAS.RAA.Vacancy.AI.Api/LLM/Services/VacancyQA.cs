using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

public interface IVacancyQA
{
    bool FlagifyLLMResponse(string llMtext, bool invertLogic, bool spellingCheck);

    // Excluded from code coverage BECAUSE this relies on Azure OpenAI and is thus nondeterministic output.
    Task<LLMReturnResult> CallLLM(string systemHeader, string mainDirective, string additionalDirective, string vacancyTextToReview, string checkname, float temperature);
}
public class VacancyQA(ILogger<VacancyQA> logger, IOptions<VacancyAiConfiguration> configuration) : IVacancyQA
{
    public bool FlagifyLLMResponse(string llMtext, bool invertLogic, bool spellingCheck)
    {
        if (spellingCheck)
        {
            // spelling check is simpler - check for existence of "None" keyword as this a specific prompt directive
            return !llMtext.Contains("none", StringComparison.CurrentCultureIgnoreCase);
        }
            
        var containsyes = llMtext.Contains("yes", StringComparison.CurrentCultureIgnoreCase);
        var containsno = llMtext.Contains("no", StringComparison.CurrentCultureIgnoreCase);
        if (invertLogic)
        {
            return !containsyes && // test passes in this instance
                   containsno;
        }

        return containsyes;
    }

    [ExcludeFromCodeCoverage] // Excluded from code coverage BECAUSE this relies on Azure OpenAI and is thus nondeterministic output.
    public async Task<LLMReturnResult> CallLLM(string systemHeader, string mainDirective, string additionalDirective, string vacancyTextToReview, string checkname,float temperature=1.0F)
    {
        Stopwatch sw_internal = new Stopwatch();
        sw_internal.Start();
        try
        {
            var azureclient = new AzureOpenAIClient(
                new Uri(configuration.Value.LlmEndpointShort),
                new AzureKeyCredential(configuration.Value.LlmKey)
            );

            var chatclient = azureclient.GetChatClient("gpt-4o");
            var ChatOptions = new ChatCompletionOptions()
            {
                Temperature = temperature
            };

            ChatCompletion resp = await chatclient.CompleteChatAsync(
                [
                    new SystemChatMessage(systemHeader),
                    new UserChatMessage(
                        $"""
                         {mainDirective}

                         {additionalDirective}

                         {vacancyTextToReview}
                         """
                    )
                ],ChatOptions
                
            );
            sw_internal.Stop();
            float jobexectime = sw_internal.ElapsedMilliseconds / 1000.0F; // convert to seconds
            logger.LogDebug(checkname+": LLM response returned OK in " + jobexectime.ToString() + " seconds");
            return new LLMReturnResult { LLMResponse = resp.Content[0].Text, LLMErrorFlag = false, Error= new ErrorReturnObject { Check = "", Errormsg = "" },CheckRuntime=jobexectime };
        }
        catch(Exception ex)
        {
            sw_internal.Stop();
            float jobexectime = sw_internal.ElapsedMilliseconds / 1000.0F; // convert to seconds
            logger.LogError(ex, "LLM returned error for check {checkname}",checkname);
            return new LLMReturnResult { LLMResponse = "LANGUAGE MODEL API FAILED", LLMErrorFlag = true,Error=new ErrorReturnObject { Check = checkname, Errormsg = ex.Message },CheckRuntime=jobexectime };
        }
    }
}