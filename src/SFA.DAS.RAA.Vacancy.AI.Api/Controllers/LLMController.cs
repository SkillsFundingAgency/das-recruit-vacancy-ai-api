using Microsoft.AspNetCore.Mvc;


namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType<AICheckReturnResultObject>(StatusCodes.Status200OK)]
public class LlmController : ControllerBase
{
    [HttpPost(Name = "RunLLM")]
    public IActionResult RunLLM([FromBody] InputObject inputvacancy)
    {
        
        Console.WriteLine(inputvacancy);
        Console.WriteLine("Title: " + inputvacancy.Title);
        LLMExec llmcode = new(); // call class constructor to llmexec
        string llmoutput=llmcode.ExecLLM(inputvacancy);
        return Ok(llmoutput);
    }

}