using JetBrains.Annotations;
using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using Xunit;

namespace MCBA.Tests.UtilTests;

public class ExecuteTransactionTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;
    private readonly TransferViewModel _transferViewModelData;

    public ExecuteTransactionTests()
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

    // This test checks if the ExecuteTransaction class successfully performs a deposit. It does so by comparing the
    // before and after balances of the account.
    [Fact]
    public async Task Test_Execute_Deposit_Valid()
    {
        var originalBalance = _customer.Accounts.FirstOrDefault(x => x.AccountNumber == 4100).Balance;

        var expectedBalanceAfterDeposit = originalBalance + 40m;

        await new ExecuteTransaction(_context).Execute('D', _transferViewModelData, _customer.Accounts[0], null);

        var account = _context.Account.FirstOrDefault(x => x.AccountNumber == 4100);
        
        Assert.Equal(expectedBalanceAfterDeposit, account.Balance);
    }

    // This test checks if the ExecuteTransaction class successfully performs a withdraw. It does so by comparing the
    // before and after balances of the account.
    [Fact]
    public async Task Test_Execute_Withdraw_Valid()
    {
        var originalBalance = _customer.Accounts.FirstOrDefault(x => x.AccountNumber == 4100).Balance;

        var expectedBalanceAfterWithdraw = originalBalance - 40m;

        await new ExecuteTransaction(_context).Execute('W', _transferViewModelData, _customer.Accounts[0], null);

        var account = _context.Account.FirstOrDefault(x => x.AccountNumber == 4100);
        Assert.Equal(expectedBalanceAfterWithdraw, account.Balance);
    }

    // This test checks if the ExecuteTransaction class successfully performs a transfer. It does so by comparing the
    // before and after balances of the account.
    [Fact]
    public async Task Test_Execute_Transfer_Valid()
    {
        var originalSourceBalance = _customer.Accounts.FirstOrDefault(x => x.AccountNumber == 4100).Balance;
        var originalDestinationBalance = _customer.Accounts.FirstOrDefault(x => x.AccountNumber == 4101).Balance;

        var expectedSourceBalanceAfterTransfer = originalSourceBalance - 40m;
        var expectedDestinationBalanceAfterTransfer = originalDestinationBalance + 40m;

        _transferViewModelData.DestinationAccount = _customer.Accounts[1];
        _transferViewModelData.DestinationAccountId = _customer.Accounts[1].AccountNumber;

        await new ExecuteTransaction(_context).Execute('T', _transferViewModelData, _customer.Accounts[0],
            _customer.Accounts[1]);

        var sourceAccount = _context.Account.FirstOrDefault(x => x.AccountNumber == 4100);
        var destinationAccount = _context.Account.FirstOrDefault(x => x.AccountNumber == 4101);
        Assert.Equal(expectedSourceBalanceAfterTransfer, sourceAccount.Balance);
        Assert.Equal(expectedDestinationBalanceAfterTransfer, destinationAccount.Balance);
    }

    // This test checks if the ExecuteTransaction class successfully handles the application of service charges. It does
    // so by performing a given amount of transactions that definitely will incur a service charge for the various
    // account types and compares the before and after balances.
    [Theory]
    [InlineData('D', 5)]
    [InlineData('W', 5)]
    [InlineData('T', 5)]
    public async Task Test_ServiceCharges2_Valid(char transactionType, int numberOfTransactions)
    {
        var expectedBalanceAfterTransfers = 0m;
        
        var sourceAccount = _customer.Accounts[0];

        sourceAccount.Balance = 100m;

        _transferViewModelData.Amount = 10m;
        _transferViewModelData.DestinationAccount = _customer.Accounts[1];
        _transferViewModelData.DestinationAccountId = _customer.Accounts[1].AccountNumber;

        switch (transactionType)
        {
            case 'D':
                expectedBalanceAfterTransfers = sourceAccount.Balance +50m;
                for (var i = 0; i < numberOfTransactions; i++)
                { 
                    PerformTransaction('D', _transferViewModelData, null);
                }
                break;
            case 'W':
                expectedBalanceAfterTransfers = sourceAccount.Balance - 50.15m;
                for (var i = 0; i < numberOfTransactions; i++)
                {
                    PerformTransaction('W', _transferViewModelData, null);
                }
                break;
            case 'T':
                expectedBalanceAfterTransfers = sourceAccount.Balance - 50.30m;
                for (var i = 0; i < numberOfTransactions; i++)
                {
                    PerformTransaction('T', _transferViewModelData, _customer.Accounts[1]);
                }
                break;
        }
        
        var sourceBalanceAfterTransaction = _customer.Accounts[0].Balance;

        Assert.Equal(expectedBalanceAfterTransfers, sourceBalanceAfterTransaction);
    }

    private async void PerformTransaction(char transactionType, TransferViewModel transferViewModelData, [CanBeNull] Account destinationAcocunt)
    {
        await new ExecuteTransaction(_context).Execute(
            transactionType,
            transferViewModelData,
            _customer.Accounts[0],
            destinationAcocunt);
    }
}