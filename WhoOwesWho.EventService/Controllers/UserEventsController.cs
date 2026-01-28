using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Services.Gateways;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]/{userId}/events")]
    [ApiController]
    public class UserEventsController(
        IDataQueryService dataQueryService,
        IEncryptionGatewayService encryptionGatewayService
        ) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserEventsAsync(string userId, [FromQuery] bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                var id = await encryptionGatewayService.UnprotectAsync(userId);
                var allEvents = (await dataQueryService.GetEventsAsync(token, active)).ToList();

                return Ok(active 
                    ? allEvents.Where(e => e.Settled && e.Users!.Any(u => u.Id == Guid.Parse(id)))
                    : allEvents.Where(e => !e.Settled && e.Users!.Any(u => u.Id == Guid.Parse(id))));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
