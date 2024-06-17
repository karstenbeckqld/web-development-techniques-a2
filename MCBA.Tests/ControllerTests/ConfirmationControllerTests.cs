using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MCBA.Tests.ControllerTests;

public class ConfirmationControllerTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;
    private readonly TransferViewModel _transferViewModelData;


    public ConfirmationControllerTests()
    {
        var testTools = new TestTools();
        _context = testTools.GetSeedData(testTools.getContext());
        _customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
        _transferViewModelData = new TransferViewModel
        {
            ID = 4100,
            Amount = 40,
            Comment = "test",
            AccountType = 'S',
            Account = _customer.Accounts[0],
            DestinationAccount = null,
            DestinationAccountId = null
        };
    }

    [Fact]
    public async Task Test_Confirmation_Deposit_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);

        _transferViewModelData.ControllerName = "Deposit";
        
        var result = await controller.Index(_transferViewModelData, "Yes", null);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Customer", redirectResult.ControllerName);
    }
    
    [Fact]
    public async Task Test_Confirmation_Withdraw_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);
        
        _transferViewModelData.ControllerName = "Withdraw";

        var result = await controller.Index(_transferViewModelData, "Yes", null);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Customer", redirectResult.ControllerName);
    }
    
    [Fact]
    public async Task Test_Confirmation_Transfer_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);
        
        _transferViewModelData.ControllerName = "Transfer";

        var result = await controller.Index(_transferViewModelData, "Yes", 4101);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Customer", redirectResult.ControllerName);
    }
    
    [Fact]
    public async Task Test_Confirmation_SetValueToNo_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);
        
        _transferViewModelData.ControllerName = "Transfer";

        var result = await controller.Index(_transferViewModelData, "No", 4101);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Customer", redirectResult.ControllerName);
    }

    private ConfirmationController GetController(MCBAContext context)
    {
        var controller = new ConfirmationController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new TestSession()
                }
            }
        };
        return controller;
    }
}