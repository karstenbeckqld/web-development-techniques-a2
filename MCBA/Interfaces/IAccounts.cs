using MCBA.Interfaces;
using MCBA.Models;

namespace MCBA.Interfaces
{
    public interface IAccounts
    {
       public int AccountNumber { get; set; }
        public char AccountType { get; set; }
        public int CustomerID { get; set; }
        public decimal Balance { get; set; }
        
        decimal GetAvailableBalance();
        void AddAmount(decimal value);
        void WithdrawAmount(decimal value);
        void AddTransaction(decimal value, char type, string? comment, Account? destinationAccount);
    }
}