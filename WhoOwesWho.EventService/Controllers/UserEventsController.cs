using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Services;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]/{userId}/events")]
    [ApiController]
    public class UserEventsController(IDataQueryService dataQueryService) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserEventsAsync(string userId, [FromQuery] bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataQueryService.GetEventByUserAsync(userId, token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
