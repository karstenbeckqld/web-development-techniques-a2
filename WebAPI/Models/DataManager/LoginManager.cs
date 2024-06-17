
using MCBA.Models;
using WebAPI.Data;
using WebAPI.Models.Repository;

namespace WebAPI.Models.DataManager;

public class LoginManager : IDataRepository<Login, int>
{
    private readonly WebAPIContext _context;

    public LoginManager(WebAPIContext context)
    {
        _context = context;
    }

    public Login Get(int id)
    {
        return _context.Login.FirstOrDefault(login => login.CustomerID == id);
    }

    public IEnumerable<Login> GetAll()
    {
        return _context.Login.ToList();
    }

    public int Add(Login login)
    {
        _context.Login.Add(login);
        _context.SaveChanges();

        return login.CustomerID;
    }

    public int Delete(int id)
    {
        _context.Login.Remove(_context.Login.Find(id));
        _context.SaveChanges();

        return id;
    }

    public int Update(int id, Login login)
    {
        _context.Update(login);
        _context.SaveChanges();

        return id;
    }
}