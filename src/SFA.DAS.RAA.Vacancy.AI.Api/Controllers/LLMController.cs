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
        var llmoutput= await llm.ExecLLM(inputvacancy);
        return Ok(llmoutput);
    }
}