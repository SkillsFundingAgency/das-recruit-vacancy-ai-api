using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Models;
using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;


namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlmController(ILLMExec llm) : ControllerBase
{
    [HttpPost(Name = "RunLLM")]
    [ProducesResponseType<AICheckReturnResultObject>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunLLM([FromBody] InputObject inputvacancy)
    {
        try{
            var llmoutput = await llm.ExecLLM(inputvacancy);

            //HTTP 429 / LLM ERROR guarding behaviour - the LLM errors don't crash but this should be triggered as a special failure
            // If ANY errors exist, find them
            if (llmoutput.Errors.Count > 0) {  // we need to find at least one error
                string ErrorRecordString = "ERRORS FROM LLM";
                foreach (ErrorReturnObject error in llmoutput.Errors) {
                    ErrorRecordString += ("{" + error.Check + " : " + error.Errormsg + "},");
                }
                return Problem("LLM return errors::: " + ErrorRecordString);
            }


            return Ok(llmoutput);
        }
        catch(Exception ex){  // generalised exception catching
            return Problem(ex.Message);
        }
        

    }
}