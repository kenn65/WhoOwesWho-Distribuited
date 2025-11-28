using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WhoOwesWho.CurrencyService.Services;

namespace WhoOwesWho.CurrencyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrencyController(ICurrencyService currencyService) : ControllerBase
    {
        [HttpGet]
        [Route("all/get")]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await currencyService.GetCurrenciesAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("single/get")]
        [Authorize]
        public async Task<IActionResult> GetCurrencyAsync(string iso)
        {
            try
            {
                return Ok(await currencyService.GetCurrencyAsync(iso));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("exchange/rate")]
        [Authorize]
        public async Task<IActionResult> GetExchangeRate([Required] string paymentCurrencyIso, [Required] string eventCurrencyIso)
        {
            try
            {
                return Ok(await currencyService.GetExchangeRateAsync(paymentCurrencyIso, eventCurrencyIso));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }

}
