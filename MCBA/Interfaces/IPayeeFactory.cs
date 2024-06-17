using MCBA.Models;

namespace MCBA.Interfaces
{
    public interface IPayeeFactory
    {
        IPayee CreatePayee(int payeeID, string name, string address, string city, string state, string postCode, string phone);
    }
}