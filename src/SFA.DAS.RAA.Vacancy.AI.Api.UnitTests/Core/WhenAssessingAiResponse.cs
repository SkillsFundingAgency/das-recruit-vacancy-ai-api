using System.Net;
using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Core;

public class WhenAssessingAiResponse
{
    private static readonly Dictionary<string, string> InputFields = new()
    {
        ["field1"] = "field1 input text",
        ["field2"] = "field2 input text",
    };

    [Test, MoqAutoData]
    public void Then_A_Passing_Review_Will_Not_Get_Flagged(
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(0);
        var okResult = new AzureAiResponse<Dictionary<string, string?>>
        {
            Result = new Dictionary<string, string?>
            {
                ["field1"] = null,
                ["field2"] = null,
            },
            StatusCode = HttpStatusCode.OK
        };

        // act
        var actual = sut.AssessResponse(InputFields, okResult, okResult, okResult);

        // assert
        actual.manualReviewRequired.Should().BeFalse();
        actual.status.Should().Be(AiReviewStatus.Passed);
        actual.errors.Should().BeNullOrEmpty();
    }
    
    [Test, MoqAutoData]
    public void Then_A_Failing_Review_Will_Get_Flagged(
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(0);
        var failingResult = new AzureAiResponse<Dictionary<string, string?>>
        {
            Result = new Dictionary<string, string?>
            {
                ["field1"] = "some issue here",
                ["field2"] = "some issue here",
            },
            StatusCode = HttpStatusCode.OK
        };

        // act
        var actual = sut.AssessResponse(InputFields, failingResult, failingResult, failingResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
        actual.errors.Should().HaveCount(3);
    }
    
    [Test, MoqAutoData]
    public void Then_A_Http_Failure_Code_Will_Get_Flagged(
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(0);
        var httpFailingResult = new AzureAiResponse<Dictionary<string, string?>>
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        // act
        var actual = sut.AssessResponse(InputFields, httpFailingResult, httpFailingResult, httpFailingResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
        actual.errors.Should().HaveCount(3);
    }
    
    [Test, MoqAutoData]
    public void Then_A_Missing_Field_Will_Get_Flagged(
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(0);
        var missingFieldResult = new AzureAiResponse<Dictionary<string, string?>>
        {
            Result = new Dictionary<string, string?>
            {
                ["field1"] = null,
            },
            StatusCode = HttpStatusCode.OK
        };

        // act
        var actual = sut.AssessResponse(InputFields, missingFieldResult, missingFieldResult, missingFieldResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
        actual.errors.Should().HaveCount(3);
        actual.errors.Should().AllSatisfy(x =>
        {
            var jsonError = x as JsonFieldsMismatchReviewError;
            jsonError.Should().NotBeNull();
            jsonError.MissingFields.Should().HaveCount(1);
            jsonError.MissingFields.Should().BeEquivalentTo("field2");
        });
    }
    
    [Test, MoqAutoData]
    public void Then_A_Hallucinated_Unknown_Field_Will_Get_Flagged(
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(0);
        var unknownFieldResult = new AzureAiResponse<Dictionary<string, string?>>
        {
            Result = new Dictionary<string, string?>
            {
                ["field1"] = null,
                ["field3"] = null,
            },
            StatusCode = HttpStatusCode.OK
        };

        // act
        var actual = sut.AssessResponse(InputFields, unknownFieldResult, unknownFieldResult, unknownFieldResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
        actual.errors.Should().HaveCount(3);
        actual.errors.Should().AllSatisfy(x =>
        {
            var jsonError = x as JsonFieldsMismatchReviewError;
            jsonError.Should().NotBeNull();
            jsonError.AdditionalFields.Should().HaveCount(1);
            jsonError.AdditionalFields.Should().BeEquivalentTo("field3");
        });
    }
    
    [Test, MoqAutoData]
    public void Then_A_Null_Result_Will_Get_Flagged(
        [Frozen] Mock<IRandomNumberGenerator> generator,
        [Greedy] AiReviewResultChecker sut)
    {
        // arrange
        generator.Setup(x => x.NextDouble()).Returns(0);
        var nullResult = new AzureAiResponse<Dictionary<string, string?>>
        {
            Result = null,
            StatusCode = HttpStatusCode.OK
        };

        // act
        var actual = sut.AssessResponse(InputFields, nullResult, nullResult, nullResult);

        // assert
        actual.manualReviewRequired.Should().BeTrue();
        actual.status.Should().Be(AiReviewStatus.Failed);
        actual.errors.Should().HaveCount(3);
    }
}