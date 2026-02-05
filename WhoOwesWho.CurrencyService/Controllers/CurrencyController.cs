using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.CurrencyService.Services;

namespace WhoOwesWho.CurrencyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrencyController(ICurrencyService currencyService) : ControllerBase
    {
        [HttpGet]
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
        [Route("{iso}")]
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
        [Route("{paymentCurrencyIso}/{eventCurrencyIso}")]
        [Authorize]
        public async Task<IActionResult> GetExchangeRate(string paymentCurrencyIso, string eventCurrencyIso)
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
