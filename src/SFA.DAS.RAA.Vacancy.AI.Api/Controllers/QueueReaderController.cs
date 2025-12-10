using System;
using Microsoft.AspNetCore.Mvc;

using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;
using System.Threading.Tasks;
namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class QueueReaderController(IQueueReader Reader) : ControllerBase
{
    [HttpPost(Name = "ReadQueue")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReadQueue([FromBody] string simplemessage)
    {
        await Reader.ReadMessages();
        return Ok("Message retrieved from Queue");
    }
}
