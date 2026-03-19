namespace SFA.DAS.RAA.Vacancy.AI.Api.Models;

public class PostPerformReviewDto
{
    public required Guid? VacancyId { get; init; }
    public required string? Title { get; init; }
    public required string? ShortDescription { get; init; }
    public required string? Description { get; init; }
    public required string? EmployerDescription { get; init; }
    public string? ThingsToConsider { get; init; }                          // optional
    public string? TrainingDescription { get; init; }                       // optional
    public string? AdditionalTrainingDescription { get; init; }             // optional
    public required string? TrainingProgrammeTitle { get; init; }
    public required string? TrainingProgrammeLevel { get; init; }
    public required string? OutcomeDescription { get; init; }
    public string? ApplicationInstructions { get; init; }                   // optional
    public string? AdditionalQuestion1 { get; init; }                       // optional
    public string? AdditionalQuestion2 { get; init; }                       // optional
    public string? WageAdditionalInformation { get; init; }                 // optional
    public string? WageCompanyBenefitsInformation { get; init; }            // optional
    public required string? WageWorkingWeekDescription { get; init; }
}