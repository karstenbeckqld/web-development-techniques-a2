using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MCBA.Interfaces;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.RegularExpressions;

namespace MCBA.Models
{
    public class Customer : ICustomer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerID { get; set; }


        // Set maximum length of name, name must not be blank
        [Required(ErrorMessage = "Name field must not be empty"), StringLength(50)]
        public string Name { get; set; }


        // Regex must be xxx xxx xxx
        [StringLength(11), MaybeNull]
        [RegularExpression(@"^[0-9]{3}\s[0-9]{3}\s[0-9]{3}",
            ErrorMessage = "Invalid Input, TFN must be Formatted: xxx xxx xxx")]
        public string TFN { get; set; }


        [StringLength(50), MaybeNull] public string Address { get; set; }


        [StringLength(40), MaybeNull] public string City { get; set; }


        // Regex - must be 1 of:
        [StringLength(3), MaybeNull]
        [RegularExpression(@"^[QLD]{3}|[NT]{2}|[ACT]{3}|[WA]{2}|[SA]{2}|[VIC]{3}|[TAS]{3}")]
        public string State { get; set; }


        [StringLength(4), MaybeNull] public string PostCode { get; set; }


        //regex must be 04xx xxx xxx
        [StringLength(12), MaybeNull]
        [RegularExpression(@"^04[0-9]{2}\s[0-9]{3}\s[0-9]{3}",
            ErrorMessage = "Invalid Input, Mobile number must be Formatted: 04xx xxx xxx")]
        public string Mobile { get; set; }

        [MaybeNull] public byte[] ProfilePicture { get; set; }


        [ValidateNever] public virtual Login login { get; set; }

        public virtual List<Account> Accounts { get; set; }

        [NotMapped] public List<IAccounts> IAccounts { get; set; }


        public Customer()
        {
        }

        public Customer(int customerID, string name, string tfn, string address, string city, string state,
            string postCode, string mobile, byte[] profilePicture, List<IAccounts> accounts, ILogin ilogin)
        {
            if (customerID <= 0)
            {
                throw new ArgumentException("Invalid customer ID, must be greater than 0", nameof(customerID));
            }

            if (name == null || name.Length > 50)
            {
                throw new ArgumentException("Invalid Name, must not be null must be less than 50 characters",
                    nameof(name));
            }

            if (!Regex.IsMatch(tfn, @"^[0-9]{3}\s[0-9]{3}\s[0-9]{3}"))
            {
                throw new ArgumentException("Invalid TFN, must be formatted to xxx xxx xxx", nameof(tfn));
            }

            if (address.Length > 50)
            {
                throw new ArgumentException("Invalid Address, Length must be less than 50", nameof(address));
            }

            if (city.Length > 40)
            {
                throw new ArgumentException("Invalid city, length must be less than 40 characters", nameof(city));
            }

            if (!Regex.IsMatch(state, "^|[QLD]{3}|[NT]{2}|[ACT]{3}|[WA]{2}|[SA]{2}|[VIC]{3}|[TAS]{3}"))
            {
                throw new ArgumentException("Invalid state, Must be valid", nameof(state));
            }

            if (postCode.Length > 4)
            {
                throw new ArgumentException("Invlalid postcode, length must be 4", nameof(postCode));
            }

            if (!Regex.IsMatch(mobile, @"^04[0-9]{2}\s[0-9]{3}\s[0-9]{3}"))
            {
                throw new ArgumentException("Invalid mobile nubmer, must be formatted to: 04xx xxx xxx",
                    nameof(mobile));
            }

            if (accounts == null || accounts.Count == 0)
            {
                throw new ArgumentException("Invalid accounts, Must not be null!", nameof(accounts));
            }

            CustomerID = customerID;
            Name = name;
            TFN = tfn;
            Address = address;
            City = city;
            State = state;
            PostCode = postCode;
            Mobile = mobile;
            if (profilePicture != null)
            {
                ProfilePicture = profilePicture;
            }

            Accounts = accounts.ConvertAll(t => (Account)t);
            login = (Login)ilogin;
        }
    }
}