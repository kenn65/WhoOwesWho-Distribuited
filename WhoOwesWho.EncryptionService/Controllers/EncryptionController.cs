using Microsoft.AspNetCore.Mvc;
using System.Text;
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
        [Route("protect/{text}")]
        public async Task<IActionResult> Protect(string text)
        {
            try
            {
                return Ok(await encryptionService.Encrypt(text)); //DataProtection is not used due to key ring issues
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        [HttpGet]
        [Route("unprotect/{text}")]
        public async Task<IActionResult> Unprotect(string text)
        {
            try
            {
                if (text.Contains(" "))
                {
                    text = text.Replace(" ", "+");
                }
                return Ok(await encryptionService.Decrypt(text));
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
            }
            catch (Exception e)
            {
                return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }


    }
}
