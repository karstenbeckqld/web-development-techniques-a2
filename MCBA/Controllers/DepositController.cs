using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MCBA.Controllers;

[AuthoriseCustomer]
public class DepositController : TransactionController
{
    private TransferViewModel _transferViewModel;

    public DepositController(MCBAContext context) : base(context)
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
        transferViewModel.ControllerName = "Deposit";
        transferViewModel.CustomerID = account.CustomerID;

        CheckInput(transferViewModel);

        if (ModelState.IsValid) return RedirectToAction("Index", "Confirmation", transferViewModel);
        
        ViewBag.Amount = transferViewModel.Amount;
        ViewBag.Comment = transferViewModel.Comment;
        
        return View(transferViewModel);
    }
}