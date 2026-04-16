using Azure;
using Azure.AI.OpenAI;
//using Newtonsoft.Json.Schema;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

using NJsonSchema;
using NJsonSchema.Generation;


namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

/*public class AzureAISchema {
    public  string Title { get; init; } = string.Empty;
    public  string ShortDescription { get; init; } = string.Empty;
    public  string Description { get; init; } = string.Empty;
    public  string EmployerDescription { get; init; } = string.Empty;
    public string ThingsToConsider { get; init; } = string.Empty;                        // optional
    public string TrainingDescription { get; init; } = string.Empty;          // optional
    public string AdditionalTrainingDescription { get; init; } = string.Empty;       // optional
    public  string TrainingProgrammeTitle { get; init; } = string.Empty;
    public  string TrainingProgrammeLevel { get; init; } = string.Empty;
    public  string OutcomeDescription { get; init; } = string.Empty;
    public string ApplicationInstructions { get; init; } = string.Empty;// optional
    public string AdditionalQuestion1 { get; init; } = string.Empty;    // optional
    public string AdditionalQuestion2 { get; init; } = string.Empty;       // optional
    public string WageAdditionalInformation { get; init; } = string.Empty;       // optional
    public string WageCompanyBenefitsInformation { get; init; } = string.Empty;   // optional
    public  string WageWorkingWeekDescription { get; init; } = string.Empty;

}
*/
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
        var gptClient = openAiClient.GetChatClient("gpt-4o");

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