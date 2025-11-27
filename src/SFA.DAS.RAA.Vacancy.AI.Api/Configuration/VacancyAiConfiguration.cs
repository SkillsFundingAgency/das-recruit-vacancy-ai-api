namespace SFA.DAS.RAA.Vacancy.AI.Api.Configuration;

public class VacancyAiConfiguration
{
    public string LlmKey { get; set; }
    public string LlmEndpointShort { get; set; }
    public Prompts DiscriminationPrompt { get; set; }
    public Prompts MissingContentPrompt { get; set; }
    public Prompts SpellingCheckPrompt { get; set; }
}

public class Prompts
{
    public string SystemPrompt { get; set; }
    public string UserHeader { get; set; }
    public string UserInstruction { get; set; }
}