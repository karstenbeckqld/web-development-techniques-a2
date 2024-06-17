using MCBA.Data;
using MCBA.Exceptions;
using MCBA.Models;
using MCBA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

// The TransactionController is the parent class for the Deposit-, Withdraw-, Statement- and TransferControllers. 
[AuthoriseCustomer]
public class TransactionController : Controller
{
    
    protected readonly MCBAContext _context;

    private readonly BalanceValidationLogic _balanceValidationLogic;

    public TransactionController(MCBAContext context)
    {
        _context = context;
        _balanceValidationLogic = new BalanceValidationLogic(new BalanceValidator());
    }
    
    protected List<Transaction> Sort(List<Transaction> transactionsList, string sortType)
    {
        if (sortType.Equals("asc"))
        {
            transactionsList = transactionsList
                .OrderBy(t => t.TransactionTimeUTC)
                .ToList();
        }

        if (sortType.Equals("desc"))
        {
            transactionsList = transactionsList
                .OrderByDescending(t => t.TransactionTimeUTC)
                .ToList();
        }

        return transactionsList;
    }

    protected static TransferViewModel SetTransferViewModelProperties(int? id, Account account,
        TransferViewModel transferViewModel)
    {
        transferViewModel.ID = id;
        transferViewModel.Account = account;
        transferViewModel.AccountType = account.AccountType;
        transferViewModel.CustomerID = account.Customer.CustomerID;
        return transferViewModel;
    }

    public void CheckInput(TransferViewModel transferViewModel)
    {
        if (transferViewModel.Amount <= 0)
        {
            ModelState.AddModelError("Error", "The entered amount must be positive.");
        }

        if (transferViewModel.Amount.HasMoreThanTwoDecimalPlaces())
        {
            ModelState.AddModelError("Error", "The entered amount cannot have more than 2 decimal places.");
        }

        if (transferViewModel.Comment is not null && transferViewModel.Comment.Length > 30)
        {
            ModelState.AddModelError("Error", "The comment cannot exceed 30 characters.");
        }
    }

    public Dictionary<string,string> CheckBalance(TransferViewModel transferViewModel, Account account, decimal serviceCharge)
    {
        var result = new Dictionary<string,string>();
       
        try
        {
            var balanceValidation = _balanceValidationLogic.PerformBalanceValidation(
                account.Balance,
                account.AccountType.ToString(),
                transferViewModel.Amount,
                serviceCharge);
        }
        catch (InsufficientFundsException e)
        {
            result.Add("Error",e.Message);
        }

        return result;
    }
}