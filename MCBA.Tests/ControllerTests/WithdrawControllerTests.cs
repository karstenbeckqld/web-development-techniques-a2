using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MCBA.Tests.ControllerTests;

public class WithdrawControllerTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;
    private readonly TransferViewModel _transferViewModelData;


    public WithdrawControllerTests()
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
            ControllerName = "Withdraw"
        };
    }

    // This test will perform a valid withdraw from a savings account and checks on the correct redirection.
    [Fact]
    public async Task Test_Withdraw_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);
        
        var result = await controller.Index(_transferViewModelData);
        
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Confirmation", redirectResult.ControllerName);
    }

    // THis test performs invalid withdraws from a savings account, applying invalid amounts, comments and too large
    // amounts to be withdrawn. It tests for an invalid model state.
    [Theory]
    [InlineData(4100, 0, "Test amount zero", 'S')]
    [InlineData(4100, -10, "Test amount minus ten", 'S')]
    [InlineData(4100, 10, "Test comment too long for model setup", 'S')]
    [InlineData(4100, 12.345, "Test too many decimal places", 'S')]
    [InlineData(4100, 1001, "Test too large amount", 'S')]
    public async Task Test_Withdraw_Invalid(int accountNumber, decimal amount, string comment, char accountType)
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
    private WithdrawController GetController(MCBAContext context)
    {
        var controller = new WithdrawController(context)
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