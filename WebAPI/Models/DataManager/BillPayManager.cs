
using MCBA.Models;
using WebAPI.Data;
using WebAPI.Models.Repository;

namespace WebAPI.Models.DataManager;

public class BillPayManager : IDataRepository<BillPay, int>
{
    private readonly WebAPIContext _context;

    public BillPayManager(WebAPIContext context)
    {
        _context = context;
    }

    public BillPay Get(int id)
    {
        return _context.BillPay.Find(id);
    }

    public IEnumerable<BillPay> GetAll()
    {
        return _context.BillPay.ToList();
    }

    public int Add(BillPay billPay)
    {
        _context.BillPay.Add(billPay);
        _context.SaveChanges();

        return billPay.BillPayID;
    }

    public int Delete(int id)
    {
        _context.BillPay.Remove(_context.BillPay.Find(id));
        _context.SaveChanges();

        return id;
    }

    public int Update(int id, BillPay billpay)
    {
        _context.Update(billpay);
        _context.SaveChanges();

        return id;
    }

    // load all billpays for a certain account
    public IEnumerable<BillPay> LoadAllBillpay(int id)
    {
           return _context.BillPay.Where(billPay => billPay.AccountNumber == id).ToList();  
    }
}