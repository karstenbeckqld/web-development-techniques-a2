using MCBA.Models;

namespace MCBA.Interfaces
  {
    public interface ILogin
    {
        string LoginID { get; }
        int CustomerID { get; }
        string PasswordHash { get; set; }
        bool LockedAccount { get; }

        void ChangePassword(string password);
    }

}