using MCBA.Models;

namespace MCBA.Interfaces
{
    public interface IBillPayFactory
    {
        public BillPay CreateBillPay(int billPayID, int accountNumber, int payeeID, decimal amount, DateTime scheduleDate, char period, bool lockedPayment);
    }
}