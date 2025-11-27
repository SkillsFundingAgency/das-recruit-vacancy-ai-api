namespace SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;

public class InputObject
{
    public string? VacancyId { get; set; } = "";
    public string? Title { get; set; } = "";
    public string? ShortDescription { get; set; } = "";
    public string? Description { get; set; } = "";
    public string? EmployerDescription { get; set; } = "";
    public string? Skills { get; set; } = "";
    public string? Qualifications { get; set; } = "";
    public string? ThingsToConsider { get; set; } = "";
    public string? TrainingDescription { get;set; } = "";
    public string? AdditionalTrainingDescription { get; set; } = "";

    public string? TrainingProgrammeTitle { get; set; } = "";
    public string? TrainingProgrammeLevel { get; set; } = "";
    public string? VacancyFull => Create_VacancyText();
    private string Create_VacancyText()
    {
        return $"""
                       Title: {Title}

                       Short description: {ShortDescription}

                       Full description: {Description}

                       Employer description: {EmployerDescription}

                       Training programme title: {TrainingProgrammeTitle}

                       Training programme level (as set by user) {TrainingProgrammeLevel}

                       Training description: {TrainingDescription}

                       Additional training description: {AdditionalTrainingDescription}

                       Skills: {Skills}

                       Qualifications: {Qualifications}

                       Things to consider: {ThingsToConsider}
                       """;
    }
        

}