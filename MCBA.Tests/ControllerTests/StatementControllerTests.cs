using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MCBA.Tests.ControllerTests;

public class StatementControllerTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;
    private TransactionsViewModel _transactionsViewModelData;


    public StatementControllerTests()
    {
        var testTools = new TestTools();
        _context = testTools.GetSeedData(testTools.getContext());
        _customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
        _transactionsViewModelData = new TransactionsViewModel
        {
            Accounts = _customer.Accounts,
            AccountNumber = _customer.Accounts[0].AccountNumber
        };
    }

    // This test checks for the correct number of pages and number of transactions returned from the StatementsController
    // for a given amount of transactions. Because the test setup already adds a transaction to each account, the
    // returned numbers are one higher than the transaction number created within the test. 
    [Fact]
    public async Task Statements_Test_Valid()
    {
        var controller = GetController(_context);

        var accounts = _context.Account.Where(c => c.CustomerID == _customer.CustomerID).ToList();

        for (var i = 1; i <= 5; i++)
        {
            accounts[0].AddTransaction(i, 'D', "Test", null);
        }

        await _context.SaveChangesAsync();

        await controller.Index(_customer.Accounts[0].AccountNumber, "Date", null);

        var data = controller.GetResultData();
        
        _transactionsViewModelData = new TransactionsViewModel
        {
            AccountNumber = data.AccountNumber,
            Accounts = data.Accounts,
            NumPages = data.NumPages,
            Transactions = data.Transactions
        };

        // The account already has one transaction from the TestTools, hence adding five will give two pages and six
        // transactions in total.
        Assert.Equal(2, _transactionsViewModelData.NumPages);
        Assert.Equal(6, _transactionsViewModelData.Transactions.TotalItemCount);
    }

    // GetController gets used to create the controller HTTP Context and create a new test session.
    private StatementController GetController(MCBAContext context)
    {
        var controller = new StatementController(context)
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