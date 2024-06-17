using MCBA.Controllers;
using MCBA.Data;
using MCBA.Exceptions;
using MCBA.Models;
using Xunit;

namespace MCBA.Tests.ControllerTests;

public class BalanceValidatorTests
{
    private readonly MCBAContext _context;
    private readonly Customer _customer;


    public BalanceValidatorTests()
    {
        var testTools = new TestTools();
        _context = testTools.GetSeedData(testTools.getContext());
        _customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
    }
    
    // THis test checks transactions that don't violate the business rules and don't throw an InsufficientFundsException.
    // The test is performed for savings and cheque accounts.
    [Theory]
    [InlineData(400, "C", 99.90, 0.10)]
    [InlineData(400, "C", 100, 0)]
    [InlineData(100, "S", 99.90, 0.10)]
    [InlineData(100, "S", 99.95, 0.05)]
    public void CheckMinBalance_Does_Not_Throw_Exception(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge)
    {
        var validator = new BalanceValidator();
        
        var result = validator.CheckMinBalance(sourceBalance, accountType, amount, serviceCharge);

        Assert.True(result);
            
    }

    // This test checks the boundary values that will lead to a failure for the transaction by throwing an
    // InsufficientFundsException. The test is performed for savings and cheque accounts.
    [Theory]
    [InlineData(301, "C", 1, 0.05)]
    [InlineData(301, "C", 2, 0)]
    [InlineData(100, "S", 100, 0.05)]
    [InlineData(100, "S", 100, 0.10)]
    public void CheckMinBalance_Throws_Exception(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge)
    {
        var validator = new BalanceValidator();

        Assert.Throws<InsufficientFundsException>(() =>
            validator.CheckMinBalance(sourceBalance, accountType, amount, serviceCharge));
    }
}