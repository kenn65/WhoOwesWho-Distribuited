using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.WebApp.CoreBusiness.Validation
{
    public class MinItemsAttribute : ValidationAttribute
    {
        private readonly int _min;

        public MinItemsAttribute(int min)
        {
            _min = min;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IEnumerable<string> list && list.Count() >= _min)
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage ?? $"Please select at least {_min} item(s).");
        }
    }
}

