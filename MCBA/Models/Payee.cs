using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using MCBA.Interfaces;

namespace MCBA.Models
{
    public class Payee : IPayee
    {
        [Required] public int PayeeID { get; set; }


        [Required(ErrorMessage = "Payee name field must not be empty"), StringLength(50)]
        public string Name { get; set; }


        [Required(ErrorMessage = "Address field must not be empty"), StringLength(50)]
        public string Address { get; set; }


        [Required(ErrorMessage = "City field must not be empty"), StringLength(40)]
        public string City { get; set; }

        // regex state must be one of the following:
        [Required, RegularExpression(@"^[NO]{2}|[QLD]{3}|[NT]{2}|[ACT]{3}|[WA]{2}|[SA]{2}|[VIC]{3}|[TAS]{3}")]
        public string State { get; set; }

        [Required(ErrorMessage = "Post code field must be 4 digits"), StringLength(4)]
        [RegularExpression(@"^[0-9]{4}", ErrorMessage = "Invalid Input Postcode must only include numbers")]
        public string PostCode { get; set; }


        // regex must be (0x)xx xxx xxx
        [Required(ErrorMessage = "Phone number field must not be empty")]
        [StringLength(14)]
        [RegularExpression(@"^[(]0[0-9]{1}[)][0-9]{2}\s[0-9]{3}\s[0-9]{3}",
            ErrorMessage = "Phone number must be formatted: (0x)xx xxx xxx")]
        public string Phone { get; set; }

        public Payee()
        {
        }

        public Payee(int payeeID, string name, string address, string city, string state, string postCode, string phone)
        {
            if (name.Length <= 0 || name.Length > 50)
            {
                throw new ArgumentException("Invalid name, Must not be empty, must be less than 50 characters",
                    nameof(name));
            }

            if (address.Length <= 0 || address.Length > 50)
            {
                throw new ArgumentException(" Invalid Address, Must not be empty, must be less than 50 characters",
                    nameof(address));
            }

            if (city.Length <= 0 || city.Length > 40)
            {
                throw new ArgumentException("invalid city, must be not be empty, must be less than 40 characters",
                    nameof(city));
            }

            if (!Regex.IsMatch(state, "^|[NO]{2}|[QLD]{3}|[NT]{2}|[ACT]{3}|[WA]{2}|[SA]{2}|[VIC]{3}|[TAS]{3}"))
            {
                throw new ArgumentException(" invalid State, must not be empty, must be less than 3 characters",
                    nameof(state));
            }

            if (postCode.Length <= 0 || postCode.Length != 4)
            {
                throw new ArgumentException(" invalid postcode, Length must be 4", nameof(postCode));
            }

            if (!Regex.IsMatch(phone, "^[(]0[0-9]{1}[)][0-9]{2}\\s[0-9]{3}\\s[0-9]{3}"))
            {
                throw new ArgumentException("Invalid phone, must be formatted: (0x)xx xxx xxx", nameof(phone));
            }

            PayeeID = payeeID;
            Name = name;
            Address = address;
            City = city;
            State = state;
            PostCode = postCode;
            Phone = phone;
        }
    }
}