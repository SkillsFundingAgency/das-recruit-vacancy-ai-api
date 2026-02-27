CREATE TABLE dbo.AiVacancyReview (
    [VacancyReviewId]   uniqueidentifier NOT NULL,
    [VacancyId]         uniqueidentifier NOT NULL,
    [Status]            nvarchar(12) NOT NULL,
    [Output]            nvarchar(max) NULL,
    [CreatedDate]       datetime2(7) NOT NULL DEFAULT GETDATE(),
    [UpdatedDate]       datetime2(7) NULL,
    CONSTRAINT [PK_AiVacancyReview] PRIMARY KEY (VacancyReviewId),
    INDEX [IX_AiVacancyReview_VacancyId] NONCLUSTERED(VacancyId)
)