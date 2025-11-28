using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EncryptionService.Services;
using WhoOwesWho.Models.Models;

namespace WhoOwesWho.EncryptionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncryptionController(IEncryptionService encryptionService)
        : ControllerBase
    {
        [HttpGet]
        [Route("encrypt")]
        public async Task<IActionResult> Encrypt(string text)
        {
            try
            {
                return Ok(await encryptionService.Encrypt(text));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpGet]
        [Route("decrypt")]
        public async Task<IActionResult> Decrypt(string text)
        {
            try
            {
                return Ok(await encryptionService.Decrypt(text));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpPost]
        [Route("cookies/encrypt")]
        public async Task<IActionResult> EncryptCookies([FromBody] CookiesRequestModel request)
        {
            try
            {
                return Ok(await encryptionService.EncryptCookies(request));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpGet]
        [Route("cookies/decrypt")]
        public async Task<IActionResult> DecryptCookies(string userId, string userEmailAddress, string admin)
        {
            try
            {
                return Ok(await encryptionService.DecryptCookies(userId, userEmailAddress, admin));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpGet]
        [Route("protect")]
        public async Task<IActionResult> Protect(string text)
        {
            try
            {
                return Ok(await encryptionService.Encrypt(text)); //Protectorservice is not used due to key ring issues
                //return Ok(await protectorService.Protect(text));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpGet]
        [Route("unprotect")]
        public async Task<IActionResult> Unprotect(string text)
        {
            try
            {
                if (text.Contains(" "))
                {
                    text = text.Replace(" ", "+");
                }
                return Ok(await encryptionService.Decrypt(text));
                //return Ok(await protectorService.Unprotect(text));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpPost]
        [Route("cookies/protect")]
        public async Task<IActionResult> ProtectCookies([FromBody] CookiesRequestModel request)
        {
            try
            {
                return Ok(await encryptionService.EncryptCookies(request));
                //return Ok(await protectorService.ProtectCookies(request));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpGet]
        [Route("cookies/unprotect")]
        public async Task<IActionResult> UnprotectCookies(string userId, string userEmailAddress, string admin)
        {
            try
            {
                return Ok(await encryptionService.DecryptCookies(userId, userEmailAddress, admin));
                //return Ok(await protectorService.UnProtectCookies(userId, userEmailAddress, admin));
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }
    }
}
