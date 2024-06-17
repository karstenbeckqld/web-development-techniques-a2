using System;

namespace MCBA.Interfaces
{
    public interface ITransaction
    {
        int TransactionID { get; set; }
        char TransactionType { get; set; }
        int AccountNumber { get; set; }
        int? DestinationAccountNumber { get; set; }
        decimal Amount { get; set; }
        string? Comment { get; set; }
        DateTime TransactionTimeUTC { get; set; }
    }
}