using System.Net;
using OpenAI.Chat;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Core.Clients;

public class WhenPerformingCheckAsync
{
    [Test, MoqAutoData]
    public async Task Then_Exceptions_Calling_Chat_Gpt_Are_Converted_Into_A_Result(
        AzureAiClientPrompt prompt,
        Dictionary<string, string> items,
        [Frozen] Mock<IChatGptClient> chatGptClient,
        [Greedy] AzureAiClient sut)
    {
        // arrange
        chatGptClient
            .Setup(x => x.CompleteChatAsync(It.IsAny<List<ChatMessage>>(), It.IsAny<ChatCompletionOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AggregateException(
                new HttpRequestException(HttpRequestError.SecureConnectionError, "The SSL connection could not be established.", statusCode: HttpStatusCode.InternalServerError),
                new HttpRequestException(HttpRequestError.SecureConnectionError, "The SSL connection could not be established.", statusCode: HttpStatusCode.InternalServerError),
                new HttpRequestException(HttpRequestError.SecureConnectionError, "The SSL connection could not be established.", statusCode: HttpStatusCode.InternalServerError)));
        
        // act
        var result = await sut.PerformCheckAsync<Dictionary<string, string?>>(prompt, items, CancellationToken.None);

        // assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Refusal.Should().Be("The SSL connection could not be established.");
    }
}