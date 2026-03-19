using System.Net;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Core;

public class WhenAssessingAiResponseSamplingRate
{
    private static readonly Dictionary<string, string> InputFields = new()
    {
        ["field1"] = "field1 input text",
        ["field2"] = "field2 input text",
    };
    
    private static readonly AzureAiResponse<Dictionary<string, string?>> OkResult = new()
    {
        Result = new Dictionary<string, string?>
        {
            ["field1"] = null,
            ["field2"] = null,
        },
        StatusCode = HttpStatusCode.OK
    };
    
    private static readonly AzureAiResponse<Dictionary<string, string?>> FailingResult = new()
    {
        Result = new Dictionary<string, string?>
        {
            ["field1"] = "some issue here",
            ["field2"] = "some issue here",
        },
        StatusCode = HttpStatusCode.OK
    };

    [Test]
    [MoqInlineAutoData(0.98, false)]
    [MoqInlineAutoData(0.99, true)]
    public void Then_A_Passing_Review_Has_A_1_Percent_Chance_To_Be_Flagged_For_Review(
        double chance,
        bool isFlagged,
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(chance);

        // act
        var actual = sut.AssessResponse(InputFields, OkResult, OkResult, OkResult);

        // assert
        actual.manualReviewRequired.Should().Be(isFlagged);
        actual.status.Should().Be(AiReviewStatus.Passed);
    }

    [Test]
    [MoqInlineAutoData(0)]
    [MoqInlineAutoData(0.5)]
    [MoqInlineAutoData(0.95)]
    public void Then_A_Discrimination_Failure_Will_Always_Get_Flagged(
        double chance,
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(chance);

        // act
        var actual = sut.AssessResponse(InputFields, OkResult, FailingResult, OkResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
    }
    
    [Test]
    [MoqInlineAutoData(0)]
    [MoqInlineAutoData(0.5)]
    [MoqInlineAutoData(0.95)]
    public void Then_A_Content_Evaluation_Failure_Will_Always_Get_Flagged(
        double chance,
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(chance);

        // act
        var actual = sut.AssessResponse(InputFields, OkResult, OkResult, FailingResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
    }
    
    [Test]
    [MoqInlineAutoData(0.48, false)]
    [MoqInlineAutoData(0.49, false)]
    [MoqInlineAutoData(0.50, true)]
    [MoqInlineAutoData(0.51, true)]
    public void Then_A_Spelling_Failure_Has_A_50_Percent_Chance_To_Be_Flagged_For_Review(
        double chance,
        bool isFlagged,
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(chance);

        // act
        var actual = sut.AssessResponse(InputFields, FailingResult, OkResult, OkResult);

        // assert
        actual.manualReviewRequired.Should().Be(isFlagged);
        actual.status.Should().Be(AiReviewStatus.Passed);
    }
}