using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Validators;

public class CreditCardCvcAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("Credit card CVC number must be provided");

        var asString = value.ToString();

        if (asString.Length < 3 || asString.Length > 4)
            return new ValidationResult($"{asString} is not a valid credit card CVC number.");

        int number = 0;

        if (!int.TryParse(asString, out number))
            return new ValidationResult($"{asString} is not a valid credit card CVC number.");

        if (number < 0)
            return new ValidationResult($"{asString} is not a valid credit card CVC number.");
        else
            return ValidationResult.Success;
    }
}
