using System.ClientModel;
using System.ClientModel.Primitives;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

public interface IChatGptClient
{
    Task<ClientResult<ChatCompletion?>> CompleteChatAsync(IEnumerable<ChatMessage> messages, ChatCompletionOptions? options = null, CancellationToken cancellationToken = default);
}

public class ChatGptClient: IChatGptClient
{
    private const int MaxRetryAttempts = 4;
    private readonly ChatClient _chatClient;
    
    public ChatGptClient(VacancyAiConfiguration configuration)
    {
        var uri = new Uri(configuration.LlmEndpointShort);
        var credential = new AzureKeyCredential(configuration.LlmKey);
        var clientOptions = new AzureOpenAIClientOptions
        {
            RetryPolicy = new ClientRetryPolicy(MaxRetryAttempts)
        };
        var openAiClient = new AzureOpenAIClient(uri, credential, clientOptions);
        _chatClient = openAiClient.GetChatClient("gpt-5.1");
    }
    
    public async Task<ClientResult<ChatCompletion?>> CompleteChatAsync(IEnumerable<ChatMessage> messages, ChatCompletionOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
    }
}