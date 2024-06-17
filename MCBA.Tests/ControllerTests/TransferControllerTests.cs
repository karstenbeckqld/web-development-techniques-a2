using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MCBA.Tests.ControllerTests;

public class TransferControllerTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;
    private readonly TransferViewModel _transferViewModelData;


    public TransferControllerTests()
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
            DestinationAccount = _customer.Accounts[1],
            DestinationAccountId = _customer.Accounts[1].AccountNumber,
            ControllerName = "Transfer"
        };
    }

    // This test performs a valid transaction from a customer's savings account to the customer's cheque account. If
    // successful, the correct redirection result is returned.
    [Fact]
    public async Task Test_Transfer_Savings_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);

        var result = await controller.Index(_transferViewModelData, 'S');
        
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Confirmation", redirectResult.ControllerName);
    }

    // This test performs a transfer from a customer's cheque account to the customer's savings account. Like above,
    // it checks on a successful redirection result.
    [Fact]
    public async Task Test_Transfer_Cheque_Valid()
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);

        _transferViewModelData.AccountType = 'C';
        
        var result = await controller.Index(_transferViewModelData, 'C');
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Confirmation", redirectResult.ControllerName);
    }

    // This test performs invalid transfers between accounts, testing for violations of amounts, balance limits and
    // comment lengths that should lead to an invalid model state.
    [Theory]
    [InlineData(4100, 0, "Test amount zero", 'S', 4101)]
    [InlineData(4100, -10, "Test amount minus ten", 'S', 4101)]
    [InlineData(4100, 10, "Test comment too long for model setup", 'S', 4101)]
    [InlineData(4100, 10000, "Test amount too large", 'S', 4101)]
    [InlineData(4100, 10.123, "Test too many decimal places", 'S', 4101)]
    [InlineData(4101, 4900, "Test amount too large", 'C', 4101)]
    [InlineData(4100, 10, "Test same account", 'S', 4100)]
    [InlineData(4100, 10, "Test non existing account", 'S', 1234)]
    [InlineData(4100, 10, "Test wrong account number", 'S', 12345678)]
    public async Task Test_Transfer_Invalid(int accountNumber, decimal amount, string comment, char accountType,
        int destinationAccountNumber)
    {
        var controller = GetController(_context);

        controller.HttpContext.Session.SetInt32(nameof(_customer.CustomerID), _customer.CustomerID);

        _transferViewModelData.ID = accountNumber;
        _transferViewModelData.Amount = amount;
        _transferViewModelData.Comment = comment;
        _transferViewModelData.AccountType = accountType;
        _transferViewModelData.DestinationAccountId = destinationAccountNumber;

        var result = await controller.Index(_transferViewModelData, accountType) as ViewResult;
        
        Assert.NotNull(result);
        Assert.False(controller.ModelState.IsValid);
    }

    // GetController gets used to create the controller HTTP Context and create a new test session.
    private TransferController GetController(MCBAContext context)
    {
        var controller = new TransferController(context)
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