using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using OpenAI.Chat;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

public record AzureAiClientPrompt(string SystemPrompt, string[] UserPrompts, float? Temperature = null);

public interface IAzureAiClient
{
    Task<AzureAiResponse<TResult>> PerformCheckAsync<TResult>(AzureAiClientPrompt prompt, Dictionary<string, string> items, CancellationToken cancellationToken) where TResult : class;
}

[ExcludeFromCodeCoverage(Justification = "Has a dependency on AzureOpenAiClient")]
public class AzureAiClient(IChatGptClient chatGptClient) : IAzureAiClient
{
    public async Task<AzureAiResponse<TResult>> PerformCheckAsync<TResult>(AzureAiClientPrompt prompt, Dictionary<string, string> items, CancellationToken cancellationToken) where TResult : class
    {
        ArgumentNullException.ThrowIfNull(prompt);
        ArgumentNullException.ThrowIfNull(items);
        
        List<ChatMessage> messages = [new SystemChatMessage(prompt.SystemPrompt)];
        messages.AddRange(prompt.UserPrompts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new UserChatMessage(x)).ToList());
        messages.Add(new UserChatMessage(JsonSerializer.Serialize(items)));

        try
        {
            var chatOptions = new ChatCompletionOptions 
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                Temperature = prompt.Temperature,
            };
            var response = await chatGptClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            return AzureAiResponse<TResult>.From(response);
        }
        catch (Exception e)
        {
            return AzureAiResponse<TResult>.From(e);
        }
    }
}