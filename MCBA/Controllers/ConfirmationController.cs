using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

// The ConfirmationController serves as a check mechanism for the customer to validate that the customer really wants
// to perform a transaction.

[AuthoriseCustomer]
public class ConfirmationController : Controller
{
    private readonly MCBAContext _context;

    public ConfirmationController(MCBAContext context) => _context = context;

    public IActionResult Index(TransferViewModel transferViewModel)
    {
        return View(transferViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        TransferViewModel transferViewModel, 
        string value, 
        int? destinationAccountNumber)
    {
        
        var sourceAccount = await _context.Account.FindAsync(transferViewModel.ID);

        transferViewModel.Account = sourceAccount;

        if (destinationAccountNumber is not null)
        {
            var destinationAccount = await _context.Account.FindAsync(destinationAccountNumber);
            transferViewModel.DestinationAccount = destinationAccount;

        }

        if (value == "Yes")
        {
            switch (transferViewModel.ControllerName)
            {
                case "Deposit":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.DepositType,
                        transferViewModel,
                        transferViewModel.Account,
                        null);
                    break;
                case "Withdraw":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.WithdrawType,
                        transferViewModel,
                        transferViewModel.Account,
                        null);
                    break;
                case "Transfer":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.TransferType,
                        transferViewModel,
                        transferViewModel.Account,
                        transferViewModel.DestinationAccount);
                    break;
            }
        }
        else
        {
            Console.WriteLine("Failed");
        }
        
        return RedirectToAction("Index", "Customer");
    }
}