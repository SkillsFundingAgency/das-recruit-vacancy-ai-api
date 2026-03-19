namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;

public class VacancyAiConfiguration
{
    public string LlmKey { get; set; }
    public string LlmEndpointShort { get; set; }
    public Prompt DiscriminationPrompt { get; set; }
    public Prompt MissingContentPrompt { get; set; }
    public Prompt SpellingCheckPrompt { get; set; }

    // configurable temperature - set to specific values in config based on performance measures.
    
    
    public float Temperature_Discrimination { get; set; } = 1.0F; // default value is 1 but may vary to 0.7 or 1.3 depending on perf calcs
    public float Temperature_MissingContent { get; set; } = 0.7F; // default value is 1 but may vary to 0.7 or 1.3 depending on perf calcs
    public float Temperature_SpellCheck { get; set; } = 0.7F; // default value is 1 but may vary to 0.7 or 1.3 depending on perf calcs
}

public class Prompt
{
    public string SystemPrompt { get; set; }
    public string UserHeader { get; set; }
    public string UserInstruction { get; set; }
}