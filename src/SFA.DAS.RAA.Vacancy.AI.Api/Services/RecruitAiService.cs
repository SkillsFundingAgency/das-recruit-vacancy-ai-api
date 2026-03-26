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

        string contenteval_custom_userheader_manual = new("""
            Vacancies must satisfy the following requirements: 
            1)	The training standard title selected for the vacancy should be relevant for the job role described in the vacancy description and title. Many vacancies are submitted where the standard/framework being offered is not the most appropriate for the role. This could take the form of an apprenticeship with the title 'Apprenticeship in Engineering' with a description related to an engineering firm, but where the standard/framework associated to it is for 'Chef', which is clearly inconsistent. In such case, this is a violation of the requirement.

            In specific cases such as for apprenticeships with training such as Team Leader, Customer Service, Business Administrator, Content Creator, IT support or similar, it is possible for such training to be associated to a wide range of industries and job titles and be very broad in the description while remaining valid.  Some duties may differ in these roles from expectations associated to the job title and specification because of the broad nature of the training, and the specific needs or roles within a specific employer organisation and thus can be ignored.

            Please also ignore any cases where the title would be generally considered similar (e.g. “Early years educator” compared to  “Reception teacher”  or “Content Creator” compared to “Digital Marketer”) where the role is within the correct industry or activity as the training.

            2)	The level of the course aligns - so a Level 4 role outlined in the text has an associated Level 4 course attached as training. 

            3)	The vacancy description (between the short description, full description and employer description) should be representative of what the role will involve and cover as many aspects of it as possible and should be reasonably related to the rest of the text. 

            4)	The description, employer description and short description when considered jointly should be sufficiently detailed to make it very clear to potential applicants exactly what will be expected of them if successful in the role. 

            5)	The description should outline specific duties that the candidate would carry out on a day-to-day basis.  These duties may be very different to the training depending on the level of the qualification, and the specific qualification in question. 

            6)	The text must not have any incomplete sections, including for the employer description. Any text which is abridged (e.g. with '...' ) or similar logic should be identified as not meeting this requirement.

            Some fields are optional and thus can be empty and exempted from this requirement if they are empty, as outlined in the following list:
             - Training description. 
            - Additional training description.
            - ThingsToConsider 
            - additionalQuestion1
            - additional Question2 
            -applicationInstructions 
            -wageAdditionalInformation 
            -CompanyBenefitsInformation 
            
            """);
        
        var contentEvaluationPrompt_humanopt = new AzureAiClientPrompt(
            configuration.MissingContentPrompt.SystemPrompt,
            [contenteval_custom_userheader_manual, configuration.MissingContentPrompt.UserInstruction],
            configuration.Temperature.MissingContent
            );


        string contenteval_custom_userheader_copilot=new("""

            Vacancies must satisfy the following requirements
           
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
            •	 ThingsToConsider is optional and may also be empty without issue.
            •	Question1 is considered optional and thus may be empty without issue.
            •	Question2 is considered optional and thus may be empty without issue.
            •	applicationInstructions is considered optional and thus may be empty without issue.
            •	wageAdditionalInformation is considered optional and thus may be empty without issue.
            •	CompanyBenefitsInformation is considered optional and thus may be empty without issue.

            FAIL if ANY required field:
            •	Contains incomplete text, abrupt endings, or ellipses (“...”).
            •	Shows evidence of being abridged or truncated.
            •	Is missing or substantively empty and is not an optional field.            
            """);
        var contentEvaluationPrompt_copilotopt = new AzureAiClientPrompt(
            configuration.MissingContentPrompt.SystemPrompt,
            [contenteval_custom_userheader_copilot, configuration.MissingContentPrompt.UserInstruction],
            configuration.Temperature.MissingContent
            );

        string spellcheck_altprompt= """
                        Correct any spelling / grammar in the JSON document and provide explanation of any errors identified. 
            This includes:
            - Typos or misspellings
            - Improper use of grammar that significantly change the meaning or are not valid sentences.
            - Incorrect use of verb tenses / grammar structures.
            

            Please do not consider the following as spelling / grammar mistakes in this specification:
            - Empty documents or empty lists - these fields may be considered optional.            
            - Incorrect use of US English Spellings as opposed to UK English spellings.
            - Use of informal language
            - minor readability issues / idiom where the text is gramatically valid, but required some additional clarity to aid flow / readability.
            - Use of shortening punctuation such as "&" instead of "and" which have no significant changes to the text.           
            - incorrect use of spacing (e.g. two spaces instead of one) or use oftabs.
            - use of valid shortenings.            
            - use of html tags (associated to the input format of the data - these can be ignored), 
            - use of \n or \t tags or  '&nbsp' / '&amp' or similar markdown/html tags (associated to the input format of the data - these can be ignored)          
            - any minor change not strictly needed for correctness of the text.
            - any other issue not explicitly related to spelling / grammar such as discrimination or choice of language used.


            Return the same JSON structure as the input, each field value should either be null with no further explanation / commentary if there were no errors for that field or return an explanation if an issue is identified.            
            """;
        var spellingPrompt_altconfig = new AzureAiClientPrompt(
            configuration.SpellingCheckPrompt.SystemPrompt,
            [spellcheck_altprompt, configuration.SpellingCheckPrompt.UserInstruction],
            configuration.Temperature.MissingContent
            );
        

        string DiscrimCheck_altprompt=new("""
            Please review the JSON document provided and identify if any discrimination is present within the document. 
            Return the same JSON structure, each field value should either be null if there is no discrimination for that field with no additional explanation, or if discrimination is identified, please provide an explanation. 
            Discrimination may take the form of an explicit age requirement in the role (e.g. available to applicants aged 18 or over, 18+ etc), or language such as 'recent graduate' or 'recent school leaver' which implies a vacancy is not open to all possible applicants. 
            Any descriptions which imply they are seeking 'young' staff may also be considered implicitly discriminatory against older applicants. 
            This can also include requirements which are identified as discriminating against gender (e.g. 'women only'), gender reassignment status, ethnicity, religion or disability status (e.g. requiring 'able-bodied' staff).
            """);

        var discriminationPrompt_altconfig = new AzureAiClientPrompt(
            configuration.DiscriminationPrompt.SystemPrompt,
            [DiscrimCheck_altprompt, configuration.DiscriminationPrompt.UserInstruction],
            configuration.Temperature.Discrimination
            );

        var contentEvaluationTask_HumanOpt = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(contentEvaluationPrompt_humanopt, fields, cancellationToken);
        var contentEvaluationTask_copilotOpt = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(contentEvaluationPrompt_copilotopt, fields, cancellationToken);
        var altspellingtask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(spellingPrompt_altconfig, fields, cancellationToken);

        var altdiscrimtask = azureAiClient.PerformCheckAsync<Dictionary<string, string?>>(discriminationPrompt_altconfig, fields, cancellationToken);

        await Task.WhenAll(contentEvaluationTask_HumanOpt,contentEvaluationTask_copilotOpt,altspellingtask,altdiscrimtask);
        return CreateResponse(fields, spellcheckTask.Result, discriminationTask.Result, contentEvaluationTask.Result, contentEvaluationTask_HumanOpt.Result, contentEvaluationTask_copilotOpt.Result,altspellingtask.Result,altdiscrimtask.Result);
    }

    private VacancyAiReviewResponse CreateResponse(
        Dictionary<string, string> fields,
        AzureAiResponse<Dictionary<string, string?>> spellcheckResult,
        AzureAiResponse<Dictionary<string, string?>> discriminationResult,
        AzureAiResponse<Dictionary<string, string?>> contentEvaluationResult,
        AzureAiResponse<Dictionary<string,string?>>? contentEvalTaskManualOptResult,
        AzureAiResponse<Dictionary<string, string?>>? contentEvalTaskCopilotOptResult,
        AzureAiResponse<Dictionary<string,string?>>? altSpellingTest,
        AzureAiResponse<Dictionary<string,string?>>? altDiscrimTest
        )
        
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
            ContentEvalTaskManualOptResult=contentEvalTaskManualOptResult,
            ContentEvalTaskCopilotOptResult=contentEvalTaskCopilotOptResult,
            SpellingTaskManualOpt = altSpellingTest,
            DiscrimTaskManualOpt=altDiscrimTest,
            Errors = errors,
        };
    }
}