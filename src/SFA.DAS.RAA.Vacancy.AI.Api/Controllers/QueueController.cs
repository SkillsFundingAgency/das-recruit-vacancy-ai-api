using System;
using Microsoft.AspNetCore.Mvc;

using SFA.DAS.RAA.Vacancy.AI.Api.LLM.Services;
using System.Threading.Tasks;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class QueueController(IQueueHandlerSimple Handler) : ControllerBase
{
    [HttpPost(Name = "AddSimpleMessageToQueue")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReadMessageFromQueue([FromBody] string simplemessage)
    {
        await Handler.AddMessageAsync(simplemessage);
        return Ok("eue");
    }
}
