using MCBA.Models;

namespace MCBA.Interfaces
{
    public interface IAccountFactory
    {
        IAccounts CreateAccount(int accountNumber, char accountType, int customerID,  decimal balance, List<ITransaction> transactions, List<BillPay> bills);
    }
}