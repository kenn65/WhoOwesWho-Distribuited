using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EventService.Auxiliaries;
using WhoOwesWho.EventService.Models;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController(
        IEventService eventService, 
        IDataMutationService dataModificationService, 
        IDataQueryService dataSelectionService, 
        IUnprotectValueMessageSender unprotectValueMessageSender, IConfiguration configuration) : ControllerBase
    {
        [HttpPost]
        [Route("single/create")]
        [Authorize]
        public async Task<IActionResult> CreateEventAsync(EventRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await dataModificationService.CreateEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("single/get")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, Duration = 0, NoStore = true)]
        public async Task<IActionResult> GetEventAsync([Required] string id, bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventAsync(Guid.Parse(id), token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("single/get/user")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, Duration = 0, NoStore = true)]
        public async Task<IActionResult> GetEventByUserAsync([Required] string userId, bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventByUserAsync(userId, token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("all/active/get")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, Duration = 0, NoStore = true)]
        public async Task<IActionResult> GetAllActiveEventsAsync(bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventsAsync(token));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
        
        [HttpGet]
        [Authorize]
        [Route("all/inactive/get/user")]
        public async Task<IActionResult> GetAllEventsByUserAsync([Required] string userId, bool active = false)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                var id = await unprotectValueMessageSender.SendAsync(new UnprotectValueRequestModel
                {
                    ApiKey = configuration["EncryptionMicroService:Security:ApiKey"]!,
                    Text = userId
                });

                var allEvents = (await dataSelectionService.GetEventsAsync(token, active)).ToList();
                return Ok(allEvents.Where(e => e.Settled && e.Users!.Any(u => u.Id == Guid.Parse(id))));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
        
        [HttpGet]
        [Route("all/inactive/get")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, Duration = 0, NoStore = true)]
        public async Task<IActionResult> GetAllInactiveEventsAsync(bool active = false)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventsAsync(token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
        
        [HttpPatch]
        [Route("single/update")]
        [Authorize]
        public async Task<IActionResult> UpdateEventAsync([FromBody] EventRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await dataModificationService.UpdateEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpDelete]
        [Route("single/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteEventAsync([Required] Guid id)
        {
            try
            {
                return Ok(await dataModificationService.DeleteEventAsync(id));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("single/assignmet/check")]
        [Authorize]
        public async Task<IActionResult> CheckAssignmentAsync(string? userId, bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetAssignmentAsync(userId!, token));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpGet]
        [Route("assignment/users")]
        [Authorize]
        public async Task<IActionResult> GetAssignmentUsers([Required] string eventId, bool active = true)
        {
            try
            {
                var token = HttpContext.ToTokenValue();
                return Ok(await dataSelectionService.GetEventUsersAsync(eventId, token, active));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }


        [HttpPost]
        [Route("single/assign")]
        [Authorize]
        public async Task<IActionResult> AssignAsync([FromBody] AssignmentRequestModel request)
        {
            try
            {
                request.Token = HttpContext.ToTokenValue();
                return Ok(await eventService.AssignAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpPost]
        [Route("single/unassign")]
        [Authorize]
        public async Task<IActionResult> UnassignAsync([FromBody] UnassignmentRequestModel request)
        {
            try
            {
                return Ok(await dataModificationService.UnassignFromEventAsync(request));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("single/settle")]
        public async Task<IActionResult> SettleEventAsync([FromBody] SettleEventRequestModel request)
        {
            try
            {
                var response = await dataModificationService.SettleEventAsync(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }
}
