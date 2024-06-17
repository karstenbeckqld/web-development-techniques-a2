using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

[AuthoriseCustomer]
public class WithdrawController : TransactionController
{
    private TransferViewModel _transferViewModel;

    public WithdrawController(MCBAContext context) : base(context)
    {
        _transferViewModel = new TransferViewModel();
    }

    public async Task<IActionResult> Index(int? id)
    {
        var account = await _context.Account.FindAsync(id);

        if (account is null) return RedirectToAction("Index", "Customer");

        _transferViewModel = SetTransferViewModelProperties(id, account, _transferViewModel);

        return View(_transferViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Index(TransferViewModel transferViewModel)
    {
        var account = await _context.Account.FindAsync(transferViewModel.ID);

        if (account is null) return RedirectToAction("Index", "Customer");

        transferViewModel.Account = account;
        transferViewModel.AccountType = account.AccountType;
        transferViewModel.ControllerName = "Withdraw";
        transferViewModel.CustomerID  = account.CustomerID;

        CheckInput(transferViewModel);

        var balanceCheck = CheckBalance(transferViewModel, account, ChargeTypes.WithdrawServiceCharge);
        
        if (balanceCheck.TryGetValue("Error", out var value)) 
        {
            ModelState.AddModelError("Error", value);
        }
        
        if (!ModelState.IsValid)
        {
            ViewBag.Amount = transferViewModel.Amount;
            ViewBag.Comment = transferViewModel.Comment;
            return View(transferViewModel);
        }
        
        return RedirectToAction("Index", "Confirmation",transferViewModel);
    }
}