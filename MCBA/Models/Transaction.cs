using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using MCBA.Interfaces;

namespace MCBA.Models
{
    public class Transaction : ITransaction
    {
        [Required] public int TransactionID { get; set; }


        [Required(ErrorMessage = "Transaction type must be selected")]
        [RegularExpression(@"[D]|[W]|[T]|[S]|[B]"), Column(TypeName = "char")]
        [DisplayName("Transaction Type")]
        public char TransactionType { get; set; }


        [Required, DisplayName("Account Number")]
        [ForeignKey("Account")]
        public int AccountNumber { get; set; }

        public virtual Account Account { get; set; }


        [ForeignKey("DestinationAccount"), DisplayName("Destination Account Number"), MaybeNull]
        public int? DestinationAccountNumber { get; set; }

        public virtual Account DestinationAccount { get; set; }

        [Required(ErrorMessage = "Transaction amount must not be empty")]
        [Column(TypeName = "money"), DataType(DataType.Currency),
         Range(0.01, double.MaxValue, ErrorMessage = "Transaction amount must be greater than $0.01")]
        public decimal Amount { get; set; }


        [StringLength(30), MaybeNull] public string Comment { get; set; }


        // DateTIme automatically converts to DateTime2 in database 
        [Required(ErrorMessage = "Date field must not be empty")]
        public DateTime TransactionTimeUTC { get; set; }

        [NotMapped] public IAccounts IDestinationAccount { get; set; }

        public Transaction()
        {
        }

        public Transaction(int transactionID, char transactionType, int accountNumber, int? destinationAccountNumber,
            decimal amount, string? comment, DateTime transactionTimeUTC)
        {
            if (transactionType != 'D' && transactionType != 'T' && transactionType != 'W' && transactionType != 'B')
            {
                throw new ArgumentException("Invalid Transaction Type, Type must be one of 'D','W','T' or 'B'",
                    nameof(transactionType));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Invalid amount, amount must not be more than 0", nameof(amount));
            }

            if (comment.Length > 30)
            {
                throw new ArgumentException("Invalid comment, Length must not exceed 30 characters", nameof(comment));
            }

            if (accountNumber == destinationAccountNumber)
            {
                throw new ArgumentException(
                    "Invalid Transaction, accountnumber must be different from destination account numberr",
                    nameof(destinationAccountNumber));
            }

            TransactionID = transactionID;
            AccountNumber = accountNumber;
            DestinationAccountNumber = destinationAccountNumber;
            Amount = amount;
            Comment = comment;
            TransactionTimeUTC = transactionTimeUTC;
            TransactionType = transactionType;
        }
    }
}