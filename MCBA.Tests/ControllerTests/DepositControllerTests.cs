using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MCBA.Tests.ControllerTests;

public class DepositControllerTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;
    private readonly TransferViewModel _transferViewModelData;


    public DepositControllerTests()
    {
        var testTools = new TestTools();
        _context = testTools.GetSeedData(testTools.getContext());
        _customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
        _transferViewModelData = new TransferViewModel
        {
            ID = 4100,
            Amount = 18,
            Comment = "test",
            AccountType = 'S',
            Account = _customer.Accounts[0],
            DestinationAccount = null,
            DestinationAccountId = null,
            ControllerName = "Deposit"
        };
    }

    // This test will perform a successful deposit into account 4100.
    [Fact]
    public async Task Test_Deposit_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);
        
        var result = await controller.Index(_transferViewModelData);
        
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Confirmation", redirectResult.ControllerName);
    }

    // This test will try to perform deposits with invalid amounts and comments and is meant to produce an invalid
    // model state.
    [Theory]
    [InlineData(4100, 0, "Test amount zero", 'S')]
    [InlineData(4100, -10, "Test amount minus ten", 'S')]
    [InlineData(4100, 10, "Test comment too long for model setup", 'S')]
    [InlineData(4100, 12.345, "Test too many decimal places", 'S')]
    public async Task Test_Deposit_Invalid(int accountNumber, decimal amount, string comment, char accountType)
    {
        var controller = GetController(_context);
    
        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);

        _transferViewModelData.ID = accountNumber;
        _transferViewModelData.Amount = amount;
        _transferViewModelData.Comment = comment;
        _transferViewModelData.AccountType = accountType;
        
        var result = await controller.Index(_transferViewModelData) as ViewResult;
       
        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
    }

    // GetController gets used to create the controller HTTP Context and create a new test session.
    private DepositController GetController(MCBAContext context)
    {
        var controller = new DepositController(context)
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