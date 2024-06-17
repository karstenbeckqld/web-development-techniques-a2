using MCBA.Data;
using MCBA.Models;

namespace MCBA.Utils;

// The ExecuteTransaction class performs the transactions on the database. It serves as a central hub for the various
// transaction methods (Deposit, Withdraw, Transfer)
public class ExecuteTransaction
{
    private readonly MCBAContext _context;

    public ExecuteTransaction(MCBAContext context) => _context = context;

    // The Execute() method takes in all required parameters to perform a transaction. It first checks for the
    // transaction type not to be of type 'D' and calculates the numbers of transactions for the given account. If the
    // transaction type is of either type 'W' or 'T', it returns the number of transactions of that type. This provides
    // subsequent methods to determine of a fee is applicable or not.
    // The subsequent switch statement then calls the respective methods of the Account model to perform the transactions.
    // A service charge is only applied if the number of transactions for a 'W' or 'T' transaction type is 2 or greater.
    public async Task Execute(char toggle, 
        TransferViewModel transferViewModel, 
        Account account,
        Account? destinationAccount)
    {
        var numOfTransactions = 0;

        if (!toggle.Equals(TransactionTypes.DepositType))
        {
            numOfTransactions = _context.Transaction
                .Where(x => x.AccountNumber == transferViewModel.ID)
                .Count(y => y.TransactionType != TransactionTypes.DepositType);
        }

        switch (toggle)
        {
            case TransactionTypes.DepositType:
                account.AddAmount(transferViewModel.Amount);
                account.AddTransaction(transferViewModel.Amount, 
                    TransactionTypes.DepositType,
                    transferViewModel.Comment, 
                    null);
                break;
            case TransactionTypes.WithdrawType:
                if (numOfTransactions >= 2)
                {
                    account.WithdrawAmount(ChargeTypes.WithdrawServiceCharge);
                    account.AddTransaction(ChargeTypes.WithdrawServiceCharge, 
                        TransactionTypes.ServiceChargeType,
                        "Service Charge", 
                        null);
                }

                account.WithdrawAmount(transferViewModel.Amount);
                account.AddTransaction(transferViewModel.Amount, 
                    TransactionTypes.WithdrawType,
                    transferViewModel.Comment,
                    null);
                break;
            case TransactionTypes.TransferType:
                if (numOfTransactions >= 2)
                {
                    account.WithdrawAmount(ChargeTypes.TransferServiceCharge);
                    account.AddTransaction(ChargeTypes.TransferServiceCharge, 
                        TransactionTypes.ServiceChargeType,
                        "Service Charge", 
                        null);
                }

                account.WithdrawAmount(transferViewModel.Amount);
                account.AddTransaction(transferViewModel.Amount, 
                    TransactionTypes.TransferType,
                    transferViewModel.Comment, 
                    destinationAccount);

                if (destinationAccount is not null)
                {
                    destinationAccount.AddTransaction(transferViewModel.Amount, 
                        TransactionTypes.TransferType, 
                        transferViewModel.Comment, 
                        null);
                    destinationAccount.AddAmount(transferViewModel.Amount);
                }
                break;
        }
        
        await _context.SaveChangesAsync();
    }
}