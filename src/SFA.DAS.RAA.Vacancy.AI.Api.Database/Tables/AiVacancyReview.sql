CREATE TABLE dbo.AiVacancyReview (
    [Id]                int NOT NULL IDENTITY,
    [VacancyReviewId]   uniqueidentifier NOT NULL,
    [Status]            tinyint NOT NULL,
    [Output]            nvarchar(max) NULL,
    CONSTRAINT [PK_AiVacancyReview] PRIMARY KEY (Id),
    INDEX [IX_AiVacancyReview_VacancyReviewId] NONCLUSTERED(VacancyReviewId)
)