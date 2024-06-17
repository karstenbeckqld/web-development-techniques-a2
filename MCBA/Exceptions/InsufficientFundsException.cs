namespace MCBA.Exceptions;

// The InsufficientFundsException is a means to tell the frontend that the account that is supposed to have money taken
// out of, does not have enough funds to support this. This does not necessarily mean the balance i $0, but that the
// business  rules had been broken. 
public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(string message)
        : base(string.Format(message))
    {

    }
}