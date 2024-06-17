using System.Diagnostics;
using MCBA.Data;
using MCBA.Utils;
using Microsoft.EntityFrameworkCore;

namespace MCBA.Controllers;

// The BillPayBackgroundService is a server service to execute scheduled bill payments in the background. It is
// registered in Program.cs and has the server process open bill payments that are not locked every minute. If a bill
// payment is once off, it gets deleted after execution. 
public class BillPayBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BillPayBackgroundService> _logger;

    public BillPayBackgroundService(IServiceProvider services, ILogger<BillPayBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BillPay Background Service is running");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBillPayAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception occured when processing background service");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                _logger.LogInformation("BillPay BackgroundProcess is waiting a minute");
            }
        }
    }

    private async Task ProcessBillPayAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MCBAContext>();

        var customers = await context.Customer.Include(x => x.Accounts).ToListAsync(cancellationToken);

        foreach (var customer in customers)
        {
            foreach (var account in customer.Accounts)
            {
                foreach (var bill in account.Bills)
                {
                    // ensures the bill is not locked
                   if(bill.Period != 'L')
                    {
                        // if scheduled date has passed
                        if (bill.ScheduleDate < DateTime.UtcNow)
                        {
                            // Use Payee name for comment. Therefore, we get the associated payee database entry at this point.
                            var payee = await context.Payee.FindAsync(new object[] { bill.PayeeID },
                                cancellationToken: cancellationToken);

                            Debug.WriteLine(account.GetAvailableBalance() + "  " + bill.Amount + "  " +
                                            (account.GetAvailableBalance() - bill.Amount));
                            
                            // checking that the bill amount can be subtracted from the account
                            if (account.GetAvailableBalance() - bill.Amount > 0)
                            {

                                // To meet the comment length requirement of max. 30 characters for a transaction, we
                                // truncate the payee name to the length necessary so that the name plus the 'BillPay:' does
                                // not exceed that limit.
                                if (payee is not null)
                                {
                                    var payeeName = Truncate(payee.Name, 20);
                                    account.AddTransaction(bill.Amount, TransactionTypes.BillPayType,
                                        $"BillPay: {payeeName}", null);
                                }
                                else
                                {
                                    account.AddTransaction(bill.Amount, TransactionTypes.BillPayType,
                                        $"BillPay For: {bill.payee.PayeeID}", null);
                                }
                                
                                if (bill.Period == 'O' || bill.Period == 'F')
                                    await DeleteBillPay(bill.BillPayID, context);
                                
                                if (bill.Period == 'M')

                                    bill.ScheduleDate = DateTime.UtcNow.AddMonths(1);
                                
                                account.WithdrawAmount(bill.Amount);
                            }
                            else
                            {
                                bill.Period = 'F';
                            }
                        }
                    }
                }
            }
        }
        
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("BillPay Background Service complete");
    }

    // Truncate method derived from https://stackoverflow.com/questions/2776673/how-do-i-truncate-a-net-string
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private static async Task DeleteBillPay(int? id, MCBAContext context)
    {
        var billPay = await context.BillPay.FindAsync(id);
        if (billPay != null)
        {
            context.BillPay.Remove(billPay);
        }

        await context.SaveChangesAsync();
    }
}