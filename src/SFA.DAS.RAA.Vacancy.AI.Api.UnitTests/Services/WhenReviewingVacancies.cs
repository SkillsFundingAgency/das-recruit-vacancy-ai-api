using System.Net;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.Services;
using SFA.DAS.RAA.Vacancy.AI.Api.Testing;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests.Services;

public class WhenReviewingVacancies
{
    [Test, MoqAutoData]
    public async Task Then_The_Prompts_Are_Passed_Correctly(
        PostPerformReviewDto data,
        [Frozen] VacancyAiConfiguration config,
        [Frozen] Mock<IAzureAiClient> azureAiClient,
        [Greedy] RecruitAiService sut)
    {
        // arrange
        var spellcheckPrompt = new AzureAiClientPrompt(config.SpellingCheckPrompt.SystemPrompt, config.SpellingCheckPrompt.UserHeader, config.SpellingCheckPrompt.UserInstruction);
        var discriminationPrompt = new AzureAiClientPrompt(config.DiscriminationPrompt.SystemPrompt, config.DiscriminationPrompt.UserHeader, config.DiscriminationPrompt.UserInstruction);
        var contentEvaluationPrompt = new AzureAiClientPrompt(config.MissingContentPrompt.SystemPrompt, config.MissingContentPrompt.UserHeader, config.MissingContentPrompt.UserInstruction);
        
        azureAiClient
            .Setup(x => x.PerformCheckAsync<Dictionary<string, string>>(It.IsAny<AzureAiClientPrompt>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AzureAiResponse<Dictionary<string, string>>
            {
                Result = [],
                RawResult = "{}",
                StatusCode = HttpStatusCode.OK
            });

        // act
        await sut.ReviewVacancyAsync(data, CancellationToken.None);

        // assert
        azureAiClient.Verify(x => x.PerformCheckAsync<Dictionary<string, string>>(ItIs.EquivalentTo(spellcheckPrompt), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once());
        azureAiClient.Verify(x => x.PerformCheckAsync<Dictionary<string, string>>(ItIs.EquivalentTo(discriminationPrompt), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once());
        azureAiClient.Verify(x => x.PerformCheckAsync<Dictionary<string, string>>(ItIs.EquivalentTo(contentEvaluationPrompt), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once());
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Data_Is_Passed_Correctly(
        PostPerformReviewDto data,
        [Frozen] Mock<IAzureAiClient> azureAiClient,
        [Greedy] RecruitAiService sut)
    {
        // arrange
        var fields = new Dictionary<string, string?>
            {
                ["Title"] = data.Title,
                ["ShortDescription"] = data.ShortDescription,
                ["Description"] = data.Description,
                ["EmployerDescription"] = data.EmployerDescription,
                ["ThingsToConsider"] = data.ThingsToConsider,
                ["TrainingDescription"] = data.TrainingDescription,
                ["AdditionalTrainingDescription"] = data.AdditionalTrainingDescription,
                ["TrainingProgrammeTitle"] = data.TrainingProgrammeTitle,
                ["TrainingProgrammeLevel"] = data.TrainingProgrammeLevel,
                ["OutcomeDescription"] = data.OutcomeDescription,
                ["ApplicationInstructions"] = data.ApplicationInstructions,
                ["AdditionalQuestion1"] = data.AdditionalQuestion1,
                ["AdditionalQuestion2"] = data.AdditionalQuestion2,
                ["WageAdditionalInformation"] = data.WageAdditionalInformation,
                ["WageCompanyBenefitsInformation"] = data.WageCompanyBenefitsInformation,
                ["WageWorkingWeekDescription"] = data.WageWorkingWeekDescription,
            }
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value!);
        
        azureAiClient
            .Setup(x => x.PerformCheckAsync<Dictionary<string, string>>(It.IsAny<AzureAiClientPrompt>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AzureAiResponse<Dictionary<string, string>>
            {
                Result = [],
                RawResult = "{}",
                StatusCode = HttpStatusCode.OK
            });

        // act
        await sut.ReviewVacancyAsync(data, CancellationToken.None);

        // assert
        azureAiClient.Verify(x => x.PerformCheckAsync<Dictionary<string, string>>(It.IsAny<AzureAiClientPrompt>(), ItIs.EquivalentTo(fields), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Review_Is_Returned(
        PostPerformReviewDto data,
        [Frozen] Mock<IAzureAiClient> azureAiClient,
        [Greedy] RecruitAiService sut)
    {
        // arrange
        var spellcheckResult = new Dictionary<string, string> { ["foo"] = "some text" };
        var discriminationResult = new Dictionary<string, string> { ["foo"] = "some text" };
        var scontentEvaluationResult = new Dictionary<string, string> { ["foo"] = "some text" };
        azureAiClient
            .SetupSequence(x => x.PerformCheckAsync<Dictionary<string, string>>(It.IsAny<AzureAiClientPrompt>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AzureAiResponse<Dictionary<string, string>>
            {
                Result = spellcheckResult,
                RawResult = "{}",
                StatusCode = HttpStatusCode.OK
            }).ReturnsAsync(new AzureAiResponse<Dictionary<string, string>>
            {
                Result = discriminationResult,
                RawResult = "{}",
                StatusCode = HttpStatusCode.OK
            }).ReturnsAsync(new AzureAiResponse<Dictionary<string, string>>
            {
                Result = scontentEvaluationResult,
                RawResult = "{}",
                StatusCode = HttpStatusCode.OK
            });

        // act
        var result = await sut.ReviewVacancyAsync(data, CancellationToken.None);

        // assert
        result.SpellcheckResult.Result.Should().BeEquivalentTo(spellcheckResult);
        result.DiscriminationResult.Result.Should().BeEquivalentTo(discriminationResult);
        result.ContentEvaluationResult.Result.Should().BeEquivalentTo(scontentEvaluationResult);
    }
}