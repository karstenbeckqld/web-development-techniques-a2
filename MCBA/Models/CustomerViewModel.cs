using System.ComponentModel.DataAnnotations;

namespace MCBA.Models;

public class CustomerViewModel
{
    public int CustomerID { get; set; }
    
    [Display(Name="Current Password")]
    public string PasswordHash { get; set; }
    
    public string Name { get; set; }
    
    public string TFN { get; set; }
   
    public string Address { get; set; }
    
    public string City { get; set; }
    
    public string State { get; set; }
    
    public string PostCode { get; set; }
    
    public string Mobile { get; set; }
    
    public string NewPassword { get; set; }
    
    public string NewPasswordConfirm { get; set; }
    
    public byte[] ProfilePicture { get; set; }

    public override string ToString()
    {
        return "\nCustomerViewModel Content:"
               + "\nCustomerId: " + CustomerID
               + "\nPasswordHash: " + PasswordHash
               + "\nNewPassword: " + NewPassword
               + "\nNewPasswordConfirm: " + NewPasswordConfirm;
    }
}