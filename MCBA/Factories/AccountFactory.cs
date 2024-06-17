using MCBA.Interfaces;
using MCBA.Models;
using System.Collections.Generic;

namespace MCBA.Factories
{
    public class AccountFactory : IAccountFactory
    {
        public IAccounts CreateAccount(int accountNumber, char accountType, int customerID, decimal balance, List<ITransaction> transactions, List<BillPay> bills)
        {
            return new Account(accountNumber, accountType, customerID, balance, transactions, bills);
        }
    }
}