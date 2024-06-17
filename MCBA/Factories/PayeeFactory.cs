using MCBA.Interfaces;
using MCBA.Models;

namespace MCBA.Factories
{
    public class PayeeFactory : IPayeeFactory
    {
        public IPayee CreatePayee(int payeeID, string name, string address, string city, string state, string postCode, string phone)
        {
            // Implement the logic to create a Payee instance here.
            // For example:
            return new Payee(payeeID, name, address, city, state, postCode, phone);
        }
    }
}