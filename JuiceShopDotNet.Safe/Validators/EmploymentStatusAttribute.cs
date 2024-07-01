using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Validators;

public class EmploymentStatusAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("Employment Status must be provided");

        var asString = value.ToString();

        if (asString == "Employed" || asString == "Self-Employed" || asString == "Unemployed")
            return ValidationResult.Success;
        else
            return new ValidationResult($"{asString} is not a valid employment status.");
    }
}
