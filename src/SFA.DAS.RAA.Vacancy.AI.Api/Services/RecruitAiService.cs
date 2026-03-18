using SFA.DAS.RAA.Vacancy.AI.Api.Core;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Clients;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Configuration;
using SFA.DAS.RAA.Vacancy.AI.Api.Models;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Services;

public interface IRecruitAiService
{
    Task<VacancyAiReviewResponse> ReviewVacancyAsync(PostPerformReviewDto data, CancellationToken cancellationToken);
}

public class RecruitAiService(
    VacancyAiConfiguration configuration,
    IAiReviewResultChecker checker,
    IAzureAiClient azureAiClient): IRecruitAiService
{
    public async Task<VacancyAiReviewResponse> ReviewVacancyAsync(PostPerformReviewDto data, CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, string?>
            {
                ["Title"] = data.Title,
                ["ShortDescription"] = data.ShortDescription,
                ["Description"] = data.Description,
                ["EmployerDescription"] = data.EmployerDescription,
                ["ThingsToConsider"] = data.ThingsToConsider,
                ["TrainingDescription"] = data.TrainingDescription,
                ["AdditionalTrainingDescription"] = data.AdditionalTrainingDescription,
                ["TrainingProgrammeTitle"] = data.TrainingProgrammeTitle,
                ["TrainingProgrammeLevel"] = data.TrainingProgrammeLevel,
                ["OutcomeDescription"] = data.OutcomeDescription,
                ["ApplicationInstructions"] = data.ApplicationInstructions,
                ["AdditionalQuestion1"] = data.AdditionalQuestion1,
                ["AdditionalQuestion2"] = data.AdditionalQuestion2,
                ["WageAdditionalInformation"] = data.WageAdditionalInformation,
                ["WageCompanyBenefitsInformation"] = data.WageCompanyBenefitsInformation,
                ["WageWorkingWeekDescription"] = data.WageWorkingWeekDescription,
            }
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value!);

        var spellcheckPrompt = new AzureAiClientPrompt(
            configuration.SpellingCheckPrompt.SystemPrompt,
            [configuration.SpellingCheckPrompt.UserHeader, configuration.SpellingCheckPrompt.UserInstruction],
            configuration.Temperature.SpellCheck);
        
        var discriminationPrompt = new AzureAiClientPrompt(
            configuration.DiscriminationPrompt.SystemPrompt,
            [configuration.DiscriminationPrompt.UserHeader, configuration.DiscriminationPrompt.UserInstruction],
            configuration.Temperature.Discrimination);
        
        var contentEvaluationPrompt = new AzureAiClientPrompt(
            configuration.MissingContentPrompt.SystemPrompt,
            [configuration.MissingContentPrompt.UserHeader, configuration.MissingContentPrompt.UserInstruction],
            configuration.Temperature.MissingContent);
        
        var spellcheckTask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(spellcheckPrompt, fields, cancellationToken);
        var discriminationTask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(discriminationPrompt, fields, cancellationToken);
        var contentEvaluationTask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(contentEvaluationPrompt, fields, cancellationToken);
        
        await Task.WhenAll(spellcheckTask, discriminationTask, contentEvaluationTask);


        // queue up N spelling and grammar checks for us to spit out / process as well
        var retrySpellCheckPrompt = new AzureAiClientPrompt(
            "You are a reviewer of apprenticeship vacancies within England, and you must be clear, concise, professional and polite in your responses, and do not use slang, inappropriate language or emojis in any responses",
            """
            Correct any significant spelling or grammar errors in the document provided.
            This includes:
            - Typos or misspellings
            - Improper use of grammar that significantly change the meaning or are not valid sentences.
            - Incorrect use of verb tenses / grammar structures.
            - Incorrect use of US English Spellings as opposed to UK English spellings.
            
            Please ignore the following:
            - Empty documents or empty lists - these fields may be considered optional.            
            - minor rephrasing or use of informal language
            - minor readability issues / idiom where the text is gramatically valid, but required some additional clarity.
            - Use of shortening punctuation such as "&" instead of "and" which have no            
            - incorrect use of spacing (e.g. two spaces instead of one) or tabs.
            - use of valid shortenings.            
            - use of html tags (associated to the input format of the data - these can be ignored), 
            - use of \n or \t tags or  '&nbsp' / '&amp' or similar markdown/html tags (associated to the input format of the data - these can be ignored)          
            - any minor change not strictly needed for correctness of the text.
            - any other issue not explicitly related to spelling / grammar such as discrimination or choice of language used.


            Where a significant issue is identified, provide explanation of any errors identified, otherwise if no issue is identified return null with no additional comment, correction or remarks.
            
            """,
            "Document to review (if empty - return null):  "
            );




        List<Task<AzureAiResponse<Dictionary<string, string>>>> List_spellchecks = [];        
        foreach (string k in spellcheckFields.Keys) {
            //field_to_check=
            string spellfield_name = k;
            string spellcheck_data = spellcheckFields[k];
   
            Task<AzureAiResponse<Dictionary<string, string>>> spag_task =spellchecker.PerformCustomSpellcheck<Dictionary<string,string>>(
                retrySpellCheckPrompt,
                spellcheck_data, 
                spellfield_name,
                cancellationToken
                );

            List_spellchecks.Add(spag_task);
        }
        await Task.WhenAll(List_spellchecks);

        string promptengineering_byhand = """
            Vacancies must satisfy the following requirements: 
            
            1)	The training standard title selected for the vacancy should be relevant for the job role described in the vacancy description and title. Many vacancies are submitted where the standard/framework being offered is not the most appropriate for the role. This could take the form of an apprenticeship with the title 'Apprenticeship in Engineering' with a description related to an engineering firm, but where the standard/framework associated to it is for 'Chef', which is clearly inconsistent. In such case, this is a violation of the requirement.

            In specific cases such as for apprenticeships with training such as Team Leader, Customer Service, Business Administrator, Content Creator, IT support or similar, it is possible for such training to be associated to a wide range of industries and job titles and be very broad in the description while remaining valid.  Some duties may differ in these roles from expectations associated to the job title and specification because of the broad nature of the training, and the specific needs or roles within a specific employer organisation and thus can be ignored.

            Please also ignore any cases where the title would be generally considered similar (e.g. “Early years educator” compared to  “Reception teacher”  or “Content Creator” compared to “Digital Marketer”) where the role is within the correct industry or activity as the training.

            2)	The level of the course aligns - so a Level 4 role outlined in the text has an associated Level 4 course attached as training. 

            3)	The vacancy description (between the short description, full description and employer description) should be representative of what the role will involve and cover as many aspects of it as possible and should be reasonably related to the rest of the text. 

            4)	The description, employer description and short description when considered jointly should be sufficiently detailed to make it very clear to potential applicants exactly what will be expected of them if successful in the role. 

            5)	The description should outline specific duties that the candidate would carry out on a day-to-day basis.  These duties may be very different to the training depending on the level of the qualification, and the specific qualification in question. 

            6)	The text must not have any incomplete sections, including for the employer description. Any text which is abridged (e.g. with '...' ) or similar logic should be identified as not meeting this requirement.

            Some fields are optional and thus can be empty and exempted from this requirement if they are empty, as outlined in the following list of optional fields:
             - Training description. 
            - Additional training description.
            - ThingsToConsider 
            - additionalQuestion1
            - additional Question2 
            -applicationInstructions 
            -wageAdditionalInformation 
            -CompanyBenefitsInformation 

            Ignore any other issues not on this list (e.g. Discrimination) as these will be checked separately.
            """;
        string promptengineering_copilot= """
            Vacancies must satisfy the following requirements:
            
            1. Training–Role Consistency
            PASS (return NULL) if:
            •	The training standard/framework is relevant to the described job role. 
            •	The standard is broad and legitimately applicable across many industries (e.g., Team Leader, Customer Service, Business Administrator, Content Creator, IT Support). Such vacancies may contain duties differing from standard expectations but remain acceptable. 
            •	The job title differs in wording but is similar or synonymous to the standard title (e.g., “Early Years Educator,” “Early Years Teacher,” “Reception Teacher”). 
            •	The job title and the standard appear aligned in a way a human reviewer would judge reasonable, even if titles are not identical.
            FAIL if:
            •	The training standard/framework is clearly mismatched to the job role.
            Example: Engineering job description paired with a Chef standard. 
         
            2. Course Level Alignment
            PASS (return NULL) if:
            •	The apprenticeship level in the text matches the level of the attached training standard. 
            FAIL if:
            •	The role describes a Level N apprenticeship, but the linked standard is not Level N. 
            

            3. Description Quality & Coherence
            PASS (return NULL) if ALL required fields:
            (Short description, full description, employer description)
            •	Provide a clear, representative picture of the role and its expectations. 
            •	Contain sufficient detail for an applicant to understand the job. 
            •	Outline specific day to day duties appropriate for the qualification level. 
            •	Are coherent and mutually related. 
            FAIL if ANY required field:
            •	Lacks sufficient detail.
            •	Does not describe the duties or expectations clearly.
            •	Is poorly aligned with the other fields.
            

            4. Completeness of Required Fields
            PASS (return NULL) if:
            •	All required description fields are complete and do not contain ellipses (“...”) or placeholders. 
            •	The training description may be empty without issue. 
            •	The additional training description may be empty without issue.
            •	The ThingsToConsider field is optional and may also be empty without issue.
            •	Question1 is considered optional and thus may be empty without issue.
            •	Question2 is considered optional and thus may be empty without issue.
            •	applicationInstructions is considered optional and thus may be empty without issue.
            •	wageAdditionalInformation is considered optional and thus may be empty without issue.
            •	CompanyBenefitsInformation is considered optional and thus may be empty without issue.

            FAIL if ANY required field:
            •	Contains incomplete text, abrupt endings, or ellipses (“...”).
            •	Shows evidence of being abridged or truncated.
            •	Is missing or substantively empty and is not an optional field.
            
            Ignore any other issues not on this list (e.g. Discrimination) as these will be checked separately.

            """;


        var contentEvaluationPrompt_MattConfig = new AzureAiClientPrompt(configuration.MissingContentPrompt.SystemPrompt,promptengineering_byhand , configuration.MissingContentPrompt.UserInstruction);
        var contentEvaluationPrompt_CopilotConfig = new AzureAiClientPrompt(configuration.MissingContentPrompt.SystemPrompt, promptengineering_copilot, configuration.MissingContentPrompt.UserInstruction);

        List < AzureAiResponse < Dictionary<string, string>>> List_SpellcheckResults = [];
        foreach (Task<AzureAiResponse<Dictionary<string, string>>> x in List_spellchecks) {
            List_SpellcheckResults.Add(x.Result);
        }

        // Retry Content Evaluation task
        //var contentEvaluationTask_MattPromptEngineered = azureAiClient.PerformCheckAsync<Dictionary<string, string>>(contentEvaluationPrompt_MattConfig, fieldsToCheck, cancellationToken);
        //var contentEvaluationTask_CopilotPromptEngineered= azureAiClient.PerformCheckAsync<Dictionary<string, string>>(contentEvaluationPrompt_CopilotConfig, fieldsToCheck, cancellationToken);

        //await Task.WhenAll(contentEvaluationTask_CopilotPromptEngineered, contentEvaluationTask_MattPromptEngineered);






        return new AiReviewResultV1
        return CreateResponse(fields, spellcheckTask.Result, discriminationTask.Result, contentEvaluationTask.Result);
    }

    private VacancyAiReviewResponse CreateResponse(
        Dictionary<string, string> fields,
        AzureAiResponse<Dictionary<string, string?>> spellcheckResult,
        AzureAiResponse<Dictionary<string, string?>> discriminationResult,
        AzureAiResponse<Dictionary<string, string?>> contentEvaluationResult)
    {
        var (status, manualReviewRequired, errors) = checker.AssessResponse(fields, spellcheckResult, discriminationResult, contentEvaluationResult);
        
        return new VacancyAiReviewResponse
        {
            Version = 1,
            ManualReviewRequired = manualReviewRequired,
            Status = status,
            SpellcheckResult = spellcheckResult,
            DiscriminationResult = discriminationResult,
            ContentEvaluationResult = contentEvaluationResult,
            Errors = errors,
            SpellcheckResult = spellcheckTask.Result,
            DiscriminationResult = discriminationTask.Result,
            ContentEvaluationResult = contentEvaluationTask.Result,
            RetrySpellChecks=List_SpellcheckResults,
            //ContentEvalResult_Manual=contentEvaluationTask_MattPromptEngineered.Result,
            //ContentEvalResult_Copilot=contentEvaluationTask_CopilotPromptEngineered.Result,
            
        };
    }
}