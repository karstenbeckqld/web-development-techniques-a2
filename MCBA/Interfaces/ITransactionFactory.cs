namespace MCBA.Interfaces
{
    public interface ITransactionFactory
    {
        ITransaction CreateTransaction(int transactionID, char transactionType, int accountNumber, int? destinationAccountNumber, decimal amount, string? comment, DateTime transactionTimeUTC);
    }
}