using Azure.Core;
using WhoOwesWho.PaymentService.Models;
using WhoOwesWho.PaymentService.Repositories;

namespace WhoOwesWho.PaymentService.Services.ServiceBus.Resolvers
{
    public interface IEventResolverService
    {
        Task<bool> CreateEventAsync(EventMessageRequestModel request);
    }

    public class EventResolverService(IPaymentSecurityService paymentSecurityService, IPaymentCacheRepository paymentCacheRepository) : IEventResolverService
    {
        public async Task<bool> CreateEventAsync(EventMessageRequestModel request)
        {
            try
            {
                if (!await paymentSecurityService.ValidateApiKey(request.ApiKey))
                {
                    if (!await paymentSecurityService.ValidateApiKey(request.ApiKey))
                    {
                        throw new UnauthorizedAccessException("Invalid API Key");
                    }
                }
                await paymentCacheRepository.SaveActiveEventAsync(request);
                request.Settled = true;
                await paymentCacheRepository.SaveInactiveEventAsync(request);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return false;
            }
        }
    }

}
