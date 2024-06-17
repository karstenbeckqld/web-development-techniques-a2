using MCBA.Interfaces;
using MCBA.Models;
using System;

namespace MCBA.Factories
{
    public class TransactionFactory : ITransactionFactory
    {
        public ITransaction CreateTransaction(int transactionID, char transactionType, int accountNumber, int? destinationAccountNumber, decimal amount, string? comment, DateTime transactionTimeUTC)
        {
            return new Transaction(transactionID, transactionType, accountNumber, destinationAccountNumber, amount, comment, transactionTimeUTC);
        }
    }
}