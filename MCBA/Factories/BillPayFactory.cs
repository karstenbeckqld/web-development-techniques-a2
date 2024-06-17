using MCBA.Interfaces;
using MCBA.Models;

namespace MCBA.Factories
{
    public class BillPayFactory : IBillPayFactory
    {
        public BillPay CreateBillPay(int billPayID, int accountNumber, int payeeID, decimal amount, DateTime scheduleDate, char period, bool lockedPayment)
        {
            return new BillPay(billPayID, accountNumber, payeeID, amount, scheduleDate, period, lockedPayment);
        }
    }
}