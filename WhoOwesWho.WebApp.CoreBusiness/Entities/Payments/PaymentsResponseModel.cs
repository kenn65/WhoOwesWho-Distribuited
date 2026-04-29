using System;
using System.Collections.Generic;
using System.Text;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Base;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;

namespace WhoOwesWho.WebApp.CoreBusiness.Entities.Payments
{
    public class PaymentsResponseModel : ResponseModelBase
    {
        public EventModel? Event { get; set; }
        public IEnumerable<UserPaymentResponseModel>? Payments { get; set; }
        public IEnumerable<UserBalanceResponseModel>? Balances { get; set; }
        public IEnumerable<WhoOwesWhoModel>? WhoOwesWho { get; set; }
    }
}
