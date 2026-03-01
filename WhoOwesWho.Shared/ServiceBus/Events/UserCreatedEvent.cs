using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoOwesWho.Models.ServiceBus.Events
{
    public record UserCreatedEvent(
        Guid UserId,
        string FullName,
        string Email
    );
}
