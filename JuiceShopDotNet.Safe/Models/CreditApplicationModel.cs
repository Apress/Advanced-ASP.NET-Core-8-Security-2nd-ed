using JuiceShopDotNet.Safe.Validators;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Models;

public class CreditApplicationModel
{
    [Required]
    public string FullName { get; set; }

    [DataType(DataType.Date)]
    [Required]
    public DateTime Birthdate { get; set; }

    [Required]
    [RegularExpression("^\\d{3}-\\d{2}-\\d{4}$", ErrorMessage = "Please include your Social Security Number in XXX-XX-XXXX format")]
    public string SocialSecurityNumber { get; set; }

    [Required]
    [EmploymentStatus]
    public string EmploymentStatus { get; set; }

    [Required]
    [Range(15000, 30000, ErrorMessage = "We can only provide credit to people earning between $15,000 and $30,000 a year")]
    public int Income { get; set; }
}
