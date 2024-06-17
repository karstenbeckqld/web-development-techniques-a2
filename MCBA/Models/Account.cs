using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MCBA.Enums;
using MCBA.Interfaces;
using Microsoft.Identity.Client;

namespace MCBA.Models
{
    public class Account : IAccounts
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AccountNumber { get; set; }


        [Required, RegularExpression(@"[C]|[S]"), Column(TypeName = "char")]
        public char AccountType { get; set; }


        [Required] public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; }


        [Required, Column(TypeName = "money"), DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Balance { get; set; }


        public virtual List<Transaction> Transactions { get; set; }

        public virtual List<BillPay> Bills { get; set; }

        public decimal GetAvailableBalance()
        {
            return (AccountType == ('S') ? Balance : Balance - 300);
        }

        public void AddAmount(decimal value)
        {
            if(value <= 0)
            {
                throw new ArgumentException("Value must not be less than 0", nameof(value));
            }

            Balance += value;
        }

        public void WithdrawAmount(decimal value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Value must not be less than 0", nameof(value));
            }

            Balance -= value;
        }

        public void AddTransaction(decimal value, char type, string? comment, Account? destinationAccount)
        {
            Transactions.Add(
                new Transaction
                {
                    TransactionType = type,
                    Amount = value,
                    TransactionTimeUTC = DateTime.UtcNow,
                    Comment = comment,
                    DestinationAccount = destinationAccount
                }
            );
        }
        public Account()
        {

        }

        public Account(int accountNumber, char accountType, int customerID, decimal balance, List<ITransaction> transactions, List<BillPay> bills)
        {
            if (accountType != 'S' && accountType != 'C' || accountType.GetType() != typeof(char))
            {
                throw new ArgumentException("Invalid accountType, must be 'C' or 'S'", nameof(accountType));
            }
            if (balance <= 0)
            {
                throw new ArgumentException("Invalid balance, must be greater than 0", nameof(balance));
            }
            if(customerID <= 0 || customerID.GetType() != typeof(int))
            {
                throw new ArgumentException("Invalid Customer ID, must be a valid Integer above 0", nameof(customerID));
            }

            AccountNumber = accountNumber;
            AccountType = accountType;
            CustomerID = customerID;
            Balance = balance;
            Transactions = transactions.ConvertAll(t => (Transaction)t);
            Bills = bills;
        }
    }
}