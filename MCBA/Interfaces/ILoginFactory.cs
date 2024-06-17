namespace MCBA.Interfaces
{
    public interface ILoginFactory
    {
        ILogin CreateLogin(string loginID, int customerID, string passwordHash, bool lockedAccount);
    }
}