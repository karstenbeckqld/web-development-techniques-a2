using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;

namespace MCBA.Controllers;

[AuthoriseCustomer]
public class BillPaysController : Controller
{
    private readonly MCBAContext _context;

    private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;


    public BillPaysController(MCBAContext context) => _context = context;


    // GET: BillPays
    public async Task<IActionResult> Index()
    {
        var customer = await _context.Customer.Include(x => x.Accounts)
            .FirstOrDefaultAsync(x => x.CustomerID == CustomerID);

        return View(customer);
    }

    // GET: BillPays/Create
    public IActionResult Create(int customerId)
    {
        var accounts = _context.Account.Where(x => x.CustomerID == CustomerID);

        ViewData["AccountNumber"] = new SelectList(accounts, "AccountNumber", "AccountNumber");
        ViewData["PayeeID"] = new SelectList(_context.Payee, "PayeeID", "Name");

        var Model = new BillPayViewModel { CustomerID = customerId, ScheduleDate = DateTime.Now.ToLocalTime() };

        return View(Model);
    }

    // POST: BillPays/Create
    // To protect from over posting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillPayViewModel billPay, int customerId)
    {
        billPay.CustomerID = customerId;

        // Because ScheduleDate returns datetime-local, we convert the datetime to UTC here.
        billPay.ScheduleDate = billPay.ScheduleDate.ToUniversalTime();

        if (billPay.Amount <= 0.01m)
        {
            ModelState.AddModelError("Error", "Entered amount must be greater than $0.");
            ViewData["AccountNumber"] =
                new SelectList(_context.Account, "AccountNumber", "AccountNumber", billPay.AccountNumber);
            ViewData["PayeeID"] = new SelectList(_context.Payee, "PayeeID", "Address", billPay.PayeeID);
            return View(billPay);
        }

        billPay.LockedPayment = false;

        if (ModelState.IsValid)
        {
            var addBill = new BillPay
            {
                BillPayID = billPay.BillPayID,
                AccountNumber = billPay.AccountNumber,
                Amount = billPay.Amount,
                PayeeID = billPay.PayeeID,
                Account = billPay.Account,
                ScheduleDate = billPay.ScheduleDate,
                Period = billPay.Period,
                LockedPayment = billPay.LockedPayment,
            };

            _context.Add(addBill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["AccountNumber"] =
            new SelectList(_context.Account, "AccountNumber", "AccountNumber", billPay.AccountNumber);
        ViewData["PayeeID"] = new SelectList(_context.Payee, "PayeeID", "Address", billPay.PayeeID);

        return View(billPay);
    }

    // GET: BillPays/Edit/5
    public async Task<IActionResult> Edit(int? id, int customerId)
    {
        if (id == null || _context.BillPay == null)
        {
            return NotFound();
        }

        var billPay = await _context.BillPay.FindAsync(id);

        if (billPay == null)
        {
            return NotFound();
        }

        var model = SetBillPayViewModelProperties(customerId, billPay);

        ViewData["AccountNumber"] =
            new SelectList(_context.Account, "AccountNumber", "AccountNumber", billPay.AccountNumber);
        ViewData["PayeeID"] = new SelectList(_context.Payee, "PayeeID", "Address", billPay.PayeeID);

        return View(model);
    }

    // POST: BillPays/Edit/5
    // To protect from over posting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BillPayViewModel billPayModel)
    {
        if (id != billPayModel.BillPayID)
        {
            return NotFound();
        }

        if (billPayModel.Amount <= 0.01m)
        {
            ModelState.AddModelError("Error", "Entered amount must be greater than $0.01");

            ViewData["AccountNumber"] =
                new SelectList(_context.Account, "AccountNumber", "AccountNumber", billPayModel.AccountNumber);
            ViewData["PayeeID"] = new SelectList(_context.Payee, "PayeeID", "Address", billPayModel.PayeeID);

            return View(billPayModel);
        }

        if (ModelState.IsValid)
        {
            var updateBill = new BillPay
            {
                BillPayID = billPayModel.BillPayID,
                AccountNumber = billPayModel.AccountNumber,
                Amount = billPayModel.Amount,
                PayeeID = billPayModel.PayeeID,
                Account = billPayModel.Account,
                ScheduleDate = billPayModel.ScheduleDate.ToUniversalTime(),
                Period = billPayModel.Period,
                LockedPayment = billPayModel.LockedPayment,
            };

            try
            {
                _context.Update(updateBill);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillPayExists(billPayModel.BillPayID))
                {
                    return NotFound();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["AccountNumber"] =
            new SelectList(_context.Account, "AccountNumber", "AccountNumber", billPayModel.AccountNumber);
        ViewData["PayeeID"] = new SelectList(_context.Payee, "PayeeID", "Address", billPayModel.PayeeID);

        return View(billPayModel);
    }

    // GET: BillPays/Delete/5
    public async Task<IActionResult> Delete(int? id, int CustomerID)
    {
        if (id == null || _context.BillPay == null)
        {
            return NotFound();
        }

        var billPay = await _context.BillPay
            .Include(b => b.Account)
            .Include(b => b.payee)
            .FirstOrDefaultAsync(m => m.BillPayID == id);
        billPay.CustomerID = CustomerID;

        if (billPay == null)
        {
            return NotFound();
        }

        return View(billPay);
    }

    // POST: BillPays/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.BillPay == null)
        {
            return Problem("Entity set 'MCBAContext.BillPay'  is null.");
        }

        var billPay = await _context.BillPay.FindAsync(id);
        if (billPay != null)
        {
            _context.BillPay.Remove(billPay);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult CreatePayee(int customerId)
    {
        var model = new PayeeViewModel()
        {
            CustomerID = customerId
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayee(PayeeViewModel payeeViewModel)
    {
        if (ModelState.IsValid)
        {
            var payee = new Payee
            {
                Name = payeeViewModel.Name,
                Address = payeeViewModel.Address,
                City = payeeViewModel.City,
                State = payeeViewModel.State,
                PostCode = payeeViewModel.PostCode,
                Phone = payeeViewModel.Phone
            };

            _context.Add(payee);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Create));
        }

        return View();
    }

    private bool BillPayExists(int id)
    {
        return _context.BillPay.Any(e => e.BillPayID == id);
    }

    // The SetBillPayViewModelProperties method facilitates the reuse of setting the properties for the BillPayViewModel.
    private BillPayViewModel SetBillPayViewModelProperties(int? customerId, BillPay billPayData)
    {
        var data = new BillPayViewModel
        {
            BillPayID = billPayData.BillPayID,
            AccountNumber = billPayData.AccountNumber,
            Amount = billPayData.Amount,
            PayeeID = billPayData.PayeeID,
            Account = billPayData.Account,
            ScheduleDate = billPayData.ScheduleDate.ToLocalTime(),
            Period = billPayData.Period,
            LockedPayment = billPayData.LockedPayment,
            CustomerID = customerId
        };


        return data;
    }
}