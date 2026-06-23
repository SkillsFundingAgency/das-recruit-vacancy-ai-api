using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

public record AzureAiClientPrompt(string SystemPrompt, string[] UserPrompts, float? Temperature = null);

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
        messages.AddRange(prompt.UserPrompts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new UserChatMessage(x)).ToList());
        messages.Add(new UserChatMessage(JsonSerializer.Serialize(items)));

        var uri = new Uri(configuration.LlmEndpointShort);
        var credential = new AzureKeyCredential(configuration.LlmKey);
        var clientOptions = new AzureOpenAIClientOptions
        {
            RetryPolicy = new ClientRetryPolicy(MaxRetryAttempts)
        };
        var openAiClient = new AzureOpenAIClient(uri, credential, clientOptions);
        var gptClient = openAiClient.GetChatClient("gpt-5");

        try
        {
            var chatOptions = new ChatCompletionOptions 
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                Temperature = prompt.Temperature,
            };
            var response = await gptClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            return AzureAiResponse<TResult>.From(response);
        }
        catch (ClientResultException e)
        {
            return AzureAiResponse<TResult>.From(e);
        }
    }
}