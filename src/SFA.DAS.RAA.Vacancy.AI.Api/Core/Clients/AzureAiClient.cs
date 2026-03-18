using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

public record AzureAiClientPrompt(string SystemPrompt, params string[] UserPrompts);

public interface IAzureAiClient
{
    Task<AzureAiResponse<TResult>> PerformCheckAsync<TResult>(AzureAiClientPrompt prompt, Dictionary<string, string> items, CancellationToken cancellationToken) where TResult : class;
}

[ExcludeFromCodeCoverage(Justification = "Has a dependency on AzureOpenAiClient")]
public class AzureAiClient(VacancyAiConfiguration configuration) : IAzureAiClient
{
    private const int MaxRetryAttempts = 4;
    
    public async Task<AzureAiResponse<TResult>> PerformCheckAsync<TResult>(AzureAiClientPrompt prompt, Dictionary<string, string> items, CancellationToken cancellationToken) where TResult : class
    {
        ArgumentNullException.ThrowIfNull(prompt);
        ArgumentNullException.ThrowIfNull(items);
        
        List<ChatMessage> messages = [new SystemChatMessage(prompt.SystemPrompt)];
        messages.AddRange(prompt.UserPrompts?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new UserChatMessage(x)).ToList() ?? []);
        messages.Add(new UserChatMessage(JsonSerializer.Serialize(items)));

        var uri = new Uri(configuration.LlmEndpointShort);
        var credential = new AzureKeyCredential(configuration.LlmKey);
        var clientOptions = new AzureOpenAIClientOptions
        {
            //Transport = new HttpClientPipelineTransport(httpClient), // we _could_ customise the httpclient retry policy
            RetryPolicy = new ClientRetryPolicy(MaxRetryAttempts)
        };
        var openAiClient = new AzureOpenAIClient(uri, credential, clientOptions);
        var gptClient = openAiClient.GetChatClient("gpt-4o");

        try
        {
            var response = await gptClient.CompleteChatAsync(messages, new ChatCompletionOptions { 
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                // Log Probabilities
                IncludeLogProbabilities=true,
                TopLogProbabilityCount=2,
            }, cancellationToken);

            var LogProbs = response.Value.ContentTokenLogProbabilities;
            Console.WriteLine("TEST");
            for (int i = 0; i < LogProbs.ToList().Count();i++){
                var list_top_tokens = LogProbs[i].TopLogProbabilities;
                for (int j = 0; j < list_top_tokens.Count();j++) {
                    var tok = list_top_tokens[j].Token;
                    var prob = list_top_tokens[j].LogProbability;
                    Console.WriteLine("Token " + i.ToString() +" ( ) "+j.ToString()+" : " + tok.ToString() + " Prob: " + prob.ToString());
                };
            };
            
            
            //Console.WriteLine(LogProbs.ToString());

            return AzureAiResponse<TResult>.From(response);
        }
        catch (ClientResultException e)
        {
            return AzureAiResponse<TResult>.From(e);
        }
    }
}


public interface IAzureAIClientSpellcheckVerifier{
    //
    public Task<AzureAiResponse<TResult>> PerformCustomSpellcheck<TResult>(AzureAiClientPrompt prompt, string field, string checkname, CancellationToken cancellationToken) where TResult : class;
}
[ExcludeFromCodeCoverage(Justification = "Has a dependency on AzureOpenAiClient")]
public class AzureAIClientSpellcheckVerifier(VacancyAiConfiguration configuration) : IAzureAIClientSpellcheckVerifier 
{
    private const int MaxRetryAttempts = 4;
    public async Task<AzureAiResponse<TResult>> PerformCustomSpellcheck<TResult>(AzureAiClientPrompt prompt, string field, string checkname, CancellationToken cancellationToken) where TResult : class
    {

        ArgumentNullException.ThrowIfNull(prompt);
        ArgumentNullException.ThrowIfNull(field);
        List<ChatMessage> messages = [new SystemChatMessage(prompt.SystemPrompt)];
        messages.AddRange(prompt.UserPrompts?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new UserChatMessage(x)).ToList() ?? []);
        messages.Add(new UserChatMessage(field));

        var uri = new Uri(configuration.LlmEndpointShort);
        var credential = new AzureKeyCredential(configuration.LlmKey);
        var clientOptions = new AzureOpenAIClientOptions
        {
            //Transport = new HttpClientPipelineTransport(httpClient), // we _could_ customise the httpclient retry policy
            RetryPolicy = new ClientRetryPolicy(MaxRetryAttempts)
        };
        var openAiClient = new AzureOpenAIClient(uri, credential, clientOptions);
        var gptClient = openAiClient.GetChatClient("gpt-4o");
        try
        {
            var response = await gptClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                //ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                // Log Probabilities
                IncludeLogProbabilities = true,
                TopLogProbabilityCount = 5,
                Temperature=0.7F,
            }, cancellationToken);

            var LogProbs = response.Value.ContentTokenLogProbabilities;
            //Console.WriteLine("TEST");
            for (int i = 0; i < LogProbs.ToList().Count(); i++)
            {
                var list_top_tokens = LogProbs[i].TopLogProbabilities;
                for (int j = 0; j < list_top_tokens.Count(); j++)
                {
                    var tok = list_top_tokens[j].Token;
                    var prob = list_top_tokens[j].LogProbability;
                    //Console.WriteLine("Token " + i.ToString() + " ( ) " + j.ToString() + " : " + tok.ToString() + " Prob: " + prob.ToString());
                }
                
            }
            ;
            //Console.WriteLine(LogProbs.ToString());

            return AzureAiResponse<TResult>.From(response,checkname);
        }
        catch (ClientResultException e)
        {
            return AzureAiResponse<TResult>.From(e);
        }
    }

}
