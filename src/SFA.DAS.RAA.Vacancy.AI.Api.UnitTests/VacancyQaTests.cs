using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.RAA.Vacancy.AI.Api.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;

namespace SFA.DAS.RAA.Vacancy.AI.Api.UnitTests;

public class VacancyQaTests
{
    private VacancyQA _vacancyQa;

    [SetUp]
    public void Arrange()
    {
        _vacancyQa = new VacancyQA(Mock.Of<ILogger<VacancyQA>>(), Mock.Of<IOptions<VacancyAiConfiguration>>());
    }
    [Test]
    public void CheckLLMFlagging_YES()
    {
        var output = _vacancyQa.FlagifyLLMResponse("YES", false, false);
            
        Assert.That(output,Is.True); // expect true
    }
    [Test]
    public void CheckLLMFlagging_NO()
    {
        var output = _vacancyQa.FlagifyLLMResponse("No", false, false);

        Assert.That(output, Is.False);
    }

    [Test]
    public void CheckLLMFlagging_YES_INVERTED()
    {
        var output = _vacancyQa.FlagifyLLMResponse("YES", true, false);
            
        Assert.That(output, Is.False);
    }
    [Test]
    public void CheckLLMFlagging_NO_INVERTED()
    {
        var output = _vacancyQa.FlagifyLLMResponse("NO", true, false);
            
        Assert.That(output, Is.True);
    }

    [Test]
    public void CheckSpellingNo()
    {
        var output = _vacancyQa.FlagifyLLMResponse("None", false, true);
            
        Assert.That(output, Is.False);
    }

    [Test]
    public void CheckSpellingYes()
    {
        var output = _vacancyQa.FlagifyLLMResponse("I've identified several spelling mistakes in this document", false, true);
            
        Assert.That(output, Is.True);
    }
}