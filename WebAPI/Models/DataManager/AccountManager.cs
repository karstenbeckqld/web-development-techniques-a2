
using Castle.Core.Resource;
using MCBA.Models;
using WebAPI.Data;
using WebAPI.Models.Repository;

namespace WebAPI.Models.DataManager;

public class AccountManager : IDataRepository<Account, int>
{
    private readonly WebAPIContext _context;

    public AccountManager(WebAPIContext context)
    {
        _context = context;
    }

    public Account Get(int id)
    {
        return _context.Account.FirstOrDefault(login => login.CustomerID == id);
    }

    public IEnumerable<Account> GetAll()
    {
        return _context.Account.ToList();
    }

    public int Add(Account account)
    {
        _context.Account.Add(account);
        _context.SaveChanges();

        return account.AccountNumber;
    }

    public int Delete(int id)
    {
        _context.Account.Remove(_context.Account.Find(id));
        _context.SaveChanges();

        return id;
    }

    public int Update(int id, Account account)
    {
        _context.Update(account);
        _context.SaveChanges();

        return id;
    }

    public IEnumerable<Account> GetAllAccounts(int id)
    {
        return _context.Account.Where(account => account.CustomerID == id);
    }
}