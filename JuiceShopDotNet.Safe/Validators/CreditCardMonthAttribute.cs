using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Validators;

public class CreditCardMonthAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("Credit card month must be provided");

        var asString = value.ToString();

        if (asString.Length != 2)
            return new ValidationResult($"{asString} is not a valid credit card month.");

        int month = 0;

        if (!int.TryParse(asString, out month))
            return new ValidationResult($"{asString} is not a valid credit card month.");

        if (month < 1 || month > 12)
            return new ValidationResult($"{asString} is not a valid credit card month.");
        else
            return ValidationResult.Success;
    }
}
