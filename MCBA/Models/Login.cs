using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MCBA.Interfaces;

namespace MCBA.Models
{
    public class Login :ILogin
    {
        // Database generated LoginID 8 characters
        [Required(ErrorMessage = "Login ID must not be empty!"), Column(TypeName = "char(8)")]
        [MinLength(8, ErrorMessage = "Login ID must be 8 characters in length")]
        public string LoginID { get; set; }


        [Required]
        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }


        [Required, Column(TypeName = "char(94)")]
        public string PasswordHash { get; set; }


        public void ChangePassword(string password)
        {
            PasswordHash = password;
        }

        [Required]
        public bool LockedAccount { get; set; }

        public Login()
        {

        }

        public Login(string loginID, int customerID, string passwordHash, bool lockedAccount)
        {

            if (loginID == null) throw new ArgumentException("loginID must not be null", nameof(loginID));
            if (loginID.Length != 8) {
                throw new ArgumentException("loginID Must  be 8 characters in length", nameof(loginID));
            }
            // there should probably be a check here for passwordHash len== 94 however it'd require hard-coding in unit tests and has noy
            // been done to save space;
            if(customerID <=0)
            {
                throw new ArgumentException("Invalid customerID, must be greater than 0", nameof(customerID));
            }

            LoginID = loginID;
            CustomerID = customerID;
            PasswordHash = passwordHash;
            LockedAccount = lockedAccount;
        }


    }
}
