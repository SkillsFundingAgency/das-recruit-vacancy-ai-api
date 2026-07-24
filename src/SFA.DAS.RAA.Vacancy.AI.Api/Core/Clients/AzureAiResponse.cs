using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using OpenAI.Chat;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

public class AzureAiResponse<T> where T: class
{
    public HttpStatusCode StatusCode { get; set; }
    public string? Refusal { get; set; }
    public T? Result { get; set; }
    public string? RawResult { get; set; }

    public static AzureAiResponse<T> From(ClientResult<ChatCompletion?> clientResult)
    {
        var rawResponse = clientResult.GetRawResponse();
        var chatCompletion = clientResult.Value;
        if (chatCompletion is null)
        {
            return new AzureAiResponse<T>
            {
                StatusCode = (HttpStatusCode)rawResponse.Status,
            };
        }

        var chatMessageContentPart = chatCompletion.Content.FirstOrDefault();
        var result = new AzureAiResponse<T>
        {
            Refusal = chatCompletion.Refusal,
            StatusCode = (HttpStatusCode)rawResponse.Status,
        };

        if (chatMessageContentPart is not null)
        {
            result.RawResult = chatMessageContentPart.Text;
            if (TryDeserialize(chatMessageContentPart.Text, out var jsonObject))
            {
                result.Result = jsonObject;
            }
        }
        
        return result;
    }

    private static bool TryDeserialize(string json, [NotNullWhen(true)]out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
            return result is not null;
        }
        catch (Exception)
        {
            result = null;
            return false;
        }
    }

    public static AzureAiResponse<T> From(Exception exception)
    {
        while (true)
        {
            switch (exception)
            {
                case ClientResultException clientResultException:
                    return new AzureAiResponse<T> { Refusal = clientResultException.Message, StatusCode = (HttpStatusCode)clientResultException.Status, };
                case HttpRequestException httpRequestException:
                    return new AzureAiResponse<T> { Refusal = httpRequestException.Message, StatusCode = httpRequestException.StatusCode ?? HttpStatusCode.InternalServerError, };
                case AggregateException aggregateException:
                {
                    if (aggregateException.InnerExceptions is not { Count: > 0 })
                    {
                        return new AzureAiResponse<T> { Refusal = aggregateException.Message, StatusCode = HttpStatusCode.InternalServerError, };
                    }
                    exception = aggregateException.InnerExceptions[0];
                    continue;
                }
                default:
                    return new AzureAiResponse<T> { Refusal = exception.Message, StatusCode = HttpStatusCode.InternalServerError, };
            }
        }
    }
}