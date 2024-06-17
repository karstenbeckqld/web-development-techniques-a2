using System.ComponentModel.DataAnnotations;

namespace MCBA.Models;

public class PayeeViewModel
{
    [Required] public int PayeeID { get; set; }

    [Required(ErrorMessage = "Payee name field must not be empty"), StringLength(50)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Address field must not be empty"), StringLength(50)]
    public string Address { get; set; }

    [Required(ErrorMessage = "City field must not be empty"), StringLength(40)]
    public string City { get; set; }

    [Required, RegularExpression(@"^[NO]{2}|[QLD]{3}|[NT]{2}|[ACT]{3}|[WA]{2}|[SA]{2}|[VIC]{3}|[TAS]{3}")]
    public string State { get; set; }

    [Required(ErrorMessage = "Post code field must be 4 digits"), StringLength(4)]
    [RegularExpression(@"^[0-9]{4}", ErrorMessage = "Invalid Input Postcode must only include numbers")]
    public string PostCode { get; set; }

    [Required(ErrorMessage = "Phone number field must not be empty")]
    [StringLength(14)]
    [RegularExpression(@"^[(]0[0-9]{1}[)][0-9]{2}\s[0-9]{3}\s[0-9]{3}",
        ErrorMessage = "Phone number must be formatted: (0x)xx xxx xxx")]
    public string Phone { get; set; }

    public int CustomerID { get; set; }
}