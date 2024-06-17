using MCBA.Interfaces;
using MCBA.Exceptions;

namespace MCBA.Controllers;

// The BalanceValidator is a helper class to inform the frontend about a breach of the business rules that a Check
// account must not have less than $300 as a balance and a Savings account not less than $0. If the check evaluates to
// one of the rules being breached, the CHeckMinBalance method will throw an exception.  
public class BalanceValidator : IBalanceCheck
{
    public bool CheckMinBalance(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge)
    {
        var result = true;

        switch (accountType)
        {
            case "C" when sourceBalance - amount - serviceCharge < 300:
                result = false;
                throw new InsufficientFundsException("Transfer not allowed. Account balance must not go below $300.");
            case "S" when sourceBalance - amount - serviceCharge < 0:
                result = false;
                throw new InsufficientFundsException("Transfer not allowed. Account balance must not go below $0.");
            default:
                return result;
        }
    }
}