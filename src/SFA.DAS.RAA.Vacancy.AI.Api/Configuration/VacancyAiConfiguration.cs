namespace SFA.DAS.RAA.Vacancy.AI.Api.Configuration;

public class VacancyAiConfiguration
{
    public string QueueKey { get; set; }
    public string QueueName { get; set; }

    public string LlmKey { get; set; }
    public string LlmEndpointShort { get; set; }
    public Prompt DiscriminationPrompt { get; set; }
    public Prompt MissingContentPrompt { get; set; }
    public Prompt SpellingCheckPrompt { get; set; }

    public float Temperature_spellcheck { get; set; } = 1.0F;
    public float Temperature_missingcontent { get; set; } = 1.0F;
    public float Temperature_discrimination { get; set; } = 1.0F;
}

public class Prompt
{
    public string SystemPrompt { get; set; }
    public string UserHeader { get; set; }
    public string UserInstruction { get; set; }
}