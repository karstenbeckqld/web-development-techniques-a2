namespace MCBA.Interfaces;

// The IBalanceCheck interface is the blueprint for the service class (BalanceValidator) that gets injected by the
// BalanceValidationLogic Injector class into the PerformTransaction and PerformWithdrawal classes through
// Dependency Injection.  
public interface IBalanceCheck
{
    public bool CheckMinBalance(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge);
}