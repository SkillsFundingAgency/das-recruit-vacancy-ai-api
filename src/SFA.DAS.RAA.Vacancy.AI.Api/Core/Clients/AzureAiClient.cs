using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ComponentModel;

using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using System.Dynamic;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

public class AzureAISchema {
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

        // JSON mode schema in OpenAI documentation doesn't ensure that the output returned has the same columns as that in the input. 
        // This can occur because the model hallucinates new columns into the output as valid JSON.
        // We now leverage the Azure OpenAI structured output mode to fix this.
        
       
        
        try
        {
            var azAIschema = new AzureAISchema();
            var chatOptions = new ChatCompletionOptions 
            {
                //ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                ResponseFormat=ChatResponseFormat.CreateJsonSchemaFormat(
                    "vacancy_review",
                    BinaryData.FromObjectAsJson(azAIschema),
                    "The structure of the vacancy you are tasked with reviewing",
                    true // strictness parameter
                    ),
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