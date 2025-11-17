using Microsoft.AspNetCore.Mvc;

using TestWebAPI.LLMWrapper;
using TestWebAPI.LLMExecutable;
using TestWebAPI.HelperObjects;
namespace TestWebAPI.Controllers
{    
    [ApiController]
    [Route("api/[controller]")]
    public class LLMController : ControllerBase
    {
        [HttpPost(Name = "RunLLM")]
        public IActionResult RunLLM([FromBody] InputObject inputvacancy)
        {
            
            Console.WriteLine(inputvacancy);
            Console.WriteLine("Title: " + inputvacancy.VacancySnapshot_Title);
            LLMExec llmcode = new(); // call class constructor to llmexec
            string llmoutput=llmcode.ExecLLM("Apprenticeship vacacny in Nursing requiring staff aged 18+");
            return Ok(llmoutput);
        }

    }
}