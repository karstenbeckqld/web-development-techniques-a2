using MCBA.Interfaces;
using Microsoft.Identity.Client;

namespace MCBA.Interfaces
{
    public interface ICustomerFactory
    {
        ICustomer CreateCustomer(int customerID, string name, string tfn, string address, string city, string state, string postCode, string mobile, byte[] profilePicture, List<IAccounts> accounts, ILogin iLogin);
    }
}
