
using MCBA.Controllers;
using MCBA.Interfaces;

namespace MCBA.Models;


// The BalanceValidationLogic class forms the injector class for the Dependency Injection design pattern used in the
// Perform... classes. It creates an object of the BalanceCheck service class that gets injected into the PerformTransfer
// and PerformWithdrawal classes.
public class BalanceValidationLogic
{
    private readonly IBalanceCheck _balanceCheck;

    public BalanceValidationLogic(IBalanceCheck balanceCheck)
    {
        _balanceCheck = balanceCheck;
    }

    public BalanceValidationLogic()
    {
        _balanceCheck = new BalanceValidator();
    }

    public bool PerformBalanceValidation(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge)
    {
        return _balanceCheck.CheckMinBalance(sourceBalance, accountType, amount, serviceCharge);
    }
}