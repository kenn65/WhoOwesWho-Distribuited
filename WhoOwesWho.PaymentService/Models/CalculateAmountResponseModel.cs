namespace WhoOwesWho.PaymentService.Models
{
    public class CalculateAmountResponseModel
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
