using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.EncryptionService.Services;
using WhoOwesWho.EncryptionService.Validators;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;

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
                if (string.IsNullOrEmpty(text))
                {
                    return BadRequest(new ProtectionResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.TextArgumentError
                    });
                }
                return Ok(await encryptionService.Encrypt(text)); //DataProtection is not used due to key ring issues
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProtectionResponseModel
                {
                    Message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("unprotect/{text}")]
        public async Task<IActionResult> Unprotect(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return BadRequest(new ProtectionResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.TextArgumentError
                    });
                }
                if (text.Contains(" "))
                {
                    text = text.Replace(" ", "+");
                }
                return Ok(await encryptionService.Decrypt(text));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProtectionResponseModel
                {
                    Message = e.Message
                });
                //return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
            }
        }

        //[HttpPost]
        //[Route("cookies/protect")]
        //public async Task<IActionResult> ProtectCookies([FromBody] CookiesRequestModel request)
        //{
        //    try
        //    {
        //        var validationResult = await validator.ValidateAsync(request);
        //        if (!validationResult.IsValid)
        //        {
        //            return BadRequest(new EncryptedCookiesResponseModel
        //            {
        //                Message = validationResult.Errors.First().ErrorMessage
        //            });
        //        }

        //        return Ok(await encryptionService.EncryptCookies(request));
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest($"Message: {e.Message} StackTrace: {e.StackTrace}");
        //    }
        //}
    }
}
