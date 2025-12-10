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
    Task<LLMReturnResult> CallLLM(string systemHeader, string mainDirective, string additionalDirective, string vacancyTextToReview, string checkname);
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
    public async Task<LLMReturnResult> CallLLM(string systemHeader, string mainDirective, string additionalDirective, string vacancyTextToReview, string checkname)
    {
        try
        {
            var azureclient = new AzureOpenAIClient(
                new Uri(configuration.Value.LlmEndpointShort),
                new AzureKeyCredential(configuration.Value.LlmKey)
            );

            var chatclient = azureclient.GetChatClient("gpt-4o");
            
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
                ]
            );
            
            return new LLMReturnResult { LLMResponse = resp.Content[0].Text, LLMErrorFlag = false, Error= new ErrorReturnObject { Check = "", Errormsg = "" } };
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "LLM returned error for check {checkname}",checkname);
            return new LLMReturnResult { LLMResponse = "LANGUAGE MODEL API FAILED", LLMErrorFlag = true,Error=new ErrorReturnObject { Check = checkname, Errormsg = ex.Message } };
        }
    }
}