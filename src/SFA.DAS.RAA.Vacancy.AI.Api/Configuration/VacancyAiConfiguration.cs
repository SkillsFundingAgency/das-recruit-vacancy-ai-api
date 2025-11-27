namespace SFA.DAS.RAA.Vacancy.AI.Api.Configuration;

public class VacancyAiConfiguration
{
    public string LlmKey { get; set; }
    public string LlmEndpointShort { get; set; }
    public Prompt DiscriminationPrompt { get; set; }
    public Prompt MissingContentPrompt { get; set; }
    public Prompt SpellingCheckPrompt { get; set; }
}

public class Prompt
{
    public string SystemPrompt { get; set; }
    public string UserHeader { get; set; }
    public string UserInstruction { get; set; }
}