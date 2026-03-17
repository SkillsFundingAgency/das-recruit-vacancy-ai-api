using System.Diagnostics.CodeAnalysis;
using System.Net;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Http;
using SFA.DAS.RAA.Vacancy.AI.Api.Domain;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core;

public interface IAiReviewResultChecker
{
    (AiReviewStatus status, bool manualReviewRequired, List<ReviewError>? errors) AssessResponse(
        Dictionary<string, string> fields,
        AzureAiResponse<Dictionary<string, string?>> spellCheckResult,
        AzureAiResponse<Dictionary<string, string?>> discriminationResult,
        AzureAiResponse<Dictionary<string, string?>> contentEvaluationResult);
}

public class AiReviewResultChecker(IRandomNumberGenerator generator): IAiReviewResultChecker
{
    public (AiReviewStatus status, bool manualReviewRequired, List<ReviewError>? errors) AssessResponse(
        Dictionary<string, string> fields,
        AzureAiResponse<Dictionary<string, string?>> spellCheckResult,
        AzureAiResponse<Dictionary<string, string?>> discriminationResult,
        AzureAiResponse<Dictionary<string, string?>> contentEvaluationResult)
    {
        List<ReviewError> errors = [];

        CheckSpellingResult(fields, errors, spellCheckResult);
        CheckDiscriminationResult(fields, errors, discriminationResult);
        CheckContentEvaluationResult(fields, errors, contentEvaluationResult);
        
        var totalScore = errors.Sum(x => x.Score);
        return totalScore switch
        {
            0 => (AiReviewStatus.Passed, 0.01 + generator.NextDouble() >= 1, null),
            < 1 => (AiReviewStatus.Passed, 0.5 + generator.NextDouble() >= 1, errors), // currently only spelling issues can cause this state
            _ => (AiReviewStatus.Failed, true, errors)
        };
    }

    private static void CheckSpellingResult(Dictionary<string, string> fields, List<ReviewError> errors, AzureAiResponse<Dictionary<string, string?>> spellCheckResult)
    {
        if (HasStatusError(ReviewCategory.Spellcheck, spellCheckResult.StatusCode, out var scError))
        {
            errors.Add(scError);
        }
        else if (HasIntegrityError(ReviewCategory.Spellcheck, fields, spellCheckResult.Result, out var intError))
        {
            errors.Add(intError);
        }
        else
        {
            var fieldsWithFailuresCount = spellCheckResult.Result?.Count(x => !string.IsNullOrWhiteSpace(x.Value));
            if (fieldsWithFailuresCount is >0)
            {
                errors.Add(new ReviewError(ReviewCategory.Spellcheck, $"There were {fieldsWithFailuresCount} field(s) with spelling errors", 0.5));
            }
        }
    }
    
    private static void CheckDiscriminationResult(Dictionary<string, string> fields, List<ReviewError> errors, AzureAiResponse<Dictionary<string, string?>> discriminationResult)
    {
        if (HasStatusError(ReviewCategory.Discrimination, discriminationResult.StatusCode, out var scError))
        {
            errors.Add(scError);
        }
        else if (HasIntegrityError(ReviewCategory.Discrimination, fields, discriminationResult.Result, out var intError))
        {
            errors.Add(intError);
        }
        else
        {
            var fieldsWithFailuresCount = discriminationResult.Result?.Count(x => !string.IsNullOrWhiteSpace(x.Value));
            if (fieldsWithFailuresCount is >0)
            {
                errors.Add(new ReviewError(ReviewCategory.Discrimination, $"There were {fieldsWithFailuresCount} field(s) with discrimination issues", 1));
            }
        }
    }
    
    private static void CheckContentEvaluationResult(Dictionary<string, string> fields, List<ReviewError> errors, AzureAiResponse<Dictionary<string, string?>> contentEvaluationResult)
    {
        if (HasStatusError(ReviewCategory.ContentEvaluation, contentEvaluationResult.StatusCode, out var scError))
        {
            errors.Add(scError);
        }
        else if (HasIntegrityError(ReviewCategory.ContentEvaluation, fields, contentEvaluationResult.Result, out var intError))
        {
            errors.Add(intError);
        }
        else
        {
            var fieldsWithFailuresCount = contentEvaluationResult.Result?.Count(x => !string.IsNullOrWhiteSpace(x.Value));
            if (fieldsWithFailuresCount is >0)
            {
                errors.Add(new ReviewError(ReviewCategory.ContentEvaluation, $"There were {fieldsWithFailuresCount} field(s) with content issues", 1));
            }
        }
    }

    private static bool HasStatusError(ReviewCategory category, HttpStatusCode? status, [NotNullWhen(true)] out ReviewError? error)
    {
        if (!status?.IsSuccessStatusCode() ?? false)
        {
            error = new ReviewError(category, $"HttpStatus code '{(int)status}' does not indicate success", 1);
            return true;
        }
        
        error = null;
        return false;        
    }
    
    private static bool HasIntegrityError(ReviewCategory category, Dictionary<string, string> fields, Dictionary<string, string?>? result, [NotNullWhen(true)] out ReviewError? error)
    {
        if (result is null)
        {
            error = new ReviewError(category, "The result was empty", 1);
            return true;
        }

        if (!VerifyFields(result, fields))
        {
            var missingFields = fields.Keys.Except(result.Keys).ToList();
            var additionalFields = result.Keys.Except(fields.Keys).ToList();
            error = new JsonFieldsMismatchReviewError(category, "There was a mismatch between the provided and returned fields", 1, additionalFields, missingFields);
            return true;    
        }
        
        error = null;
        return false;
    }

    private static bool VerifyFields<T>(Dictionary<string, T?> llmResult, Dictionary<string, string> fields)
    {
        return llmResult.Count == fields.Count && llmResult.Keys.All(fields.ContainsKey);
    }
}