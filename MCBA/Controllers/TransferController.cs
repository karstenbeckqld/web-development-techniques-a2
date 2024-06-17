using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

[AuthoriseCustomer]
public class TransferController:TransactionController
{
    private TransferViewModel _transferViewModel;

    public TransferController(MCBAContext context) : base(context)
    {
        _transferViewModel = new TransferViewModel();
    }
 
    public async Task<IActionResult> Index(int? id)
    {
        var account = await _context.Account.FindAsync(id);

        if (account is null) return RedirectToAction("Index","Customer");

        _transferViewModel = SetTransferViewModelProperties(id, account, _transferViewModel);

        return View(_transferViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(TransferViewModel transferViewModel, char? accountType)
    {
       var destinationAccount = new Account();
        
        var account = await _context.Account.FindAsync(transferViewModel.ID);
        
        if (account is null) return RedirectToAction("Index", "Customer");

        if (transferViewModel.DestinationAccountId.ToString().Length > 4)
        {
            ModelState.AddModelError("Error", "Account numbers must not exceed 4 digits");
        }
        
        try
        {
            destinationAccount = await _context.Account.FindAsync(transferViewModel.DestinationAccountId);
        }
        catch (Exception e)
        {
           Console.WriteLine("No destination account found: " + e);
        }

        if (destinationAccount is null)
        {
            ModelState.AddModelError("Error", "The entered destination account number does not exist.");
        }
        
        transferViewModel.Account = account;
        transferViewModel.DestinationAccount = destinationAccount;
        transferViewModel.ControllerName = "Transfer";
        transferViewModel.AccountType = account.AccountType;
        transferViewModel.CustomerID = account.CustomerID;
        
        CheckInput(transferViewModel);
        
        var balanceCheck = CheckBalance(transferViewModel, account, ChargeTypes.WithdrawServiceCharge);
        
        if (balanceCheck.TryGetValue("Error", out var value)) 
        {
            ModelState.AddModelError("Error", value);
        }
        
        if (transferViewModel.DestinationAccountId is not null 
            && transferViewModel.ID == transferViewModel.DestinationAccountId)
        {
            ModelState.AddModelError("Error", "Destination account cannot be the same as source account.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Amount = transferViewModel.Amount;
            ViewBag.Comment = transferViewModel.Comment;
            ViewBag.DestinationAccountId = transferViewModel.DestinationAccountId;

            return View(transferViewModel);
        }

        return RedirectToAction("Index","Confirmation", transferViewModel);
    }
}