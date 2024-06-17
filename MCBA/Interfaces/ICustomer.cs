using Microsoft.Identity.Client;
using System.Collections.Generic;

namespace MCBA.Interfaces
{
    public interface ICustomer
    {
        int CustomerID { get; set; }
        string Name { get; set; }
        string TFN { get; set; }
        string Address { get; set; }
        string City { get; set; }
        string State { get; set; }
        string PostCode { get; set; }
        string Mobile { get; set; }
        byte[] ProfilePicture { get; set; }

        List<IAccounts> IAccounts { get; set; }
    }

}