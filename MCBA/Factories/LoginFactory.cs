using MCBA.Interfaces;
using MCBA.Models;


namespace MCBA.Factories
{
    public class LoginFactory : ILoginFactory
    {
        public ILogin CreateLogin(string loginID, int customerID, string passwordHash, bool lockedAccount)
        {
            // Implement the logic to create a Login instance here.
            // For example:
            return new Login(loginID, customerID, passwordHash, lockedAccount);
        }
    }
}