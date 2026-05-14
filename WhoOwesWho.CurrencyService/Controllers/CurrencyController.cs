using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using WhoOwesWho.CurrencyService.Services;
using WhoOwesWho.Shared.Auxiliaries;
using WhoOwesWho.Shared.Models;
using WhoOwesWho.Shared.Models.Base;

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
                var response = await currencyService.GetCurrenciesAsync();
                return Ok(new EnumerableWrapperResponseModel<IEnumerable<CurrencyResponseModel>>
                {
                    Data = response
                });
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
                if (string.IsNullOrWhiteSpace(iso)) 
                {
                    return BadRequest(new CurrencyResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.CurrencyIsoError
                    });
                }
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
                if (string.IsNullOrWhiteSpace(paymentCurrencyIso))
                {
                    return BadRequest(new ExchangeRateResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.PaymentCurrencyIsoError
                    });
                }

                if (string.IsNullOrWhiteSpace(eventCurrencyIso))
                {
                    return BadRequest(new ExchangeRateResponseModel
                    {
                        Message = Constants.RequestArgumentErrorMessages.PaymentCurrencyIsoError
                    });
                }

                return Ok(await currencyService.GetExchangeRateAsync(paymentCurrencyIso, eventCurrencyIso));
            }
            catch (Exception e)
            {
                return BadRequest(e.StackTrace);
            }
        }
    }

}
