namespace MCBA.Interfaces
{
    public interface IBillPay
    {
        int BillPayID { get; }
        int AccountNumber { get; }
        int PayeeID { get; }
        decimal Amount { get; }
        DateTime ScheduleDate { get; }
        char Period { get; }
        bool LockedPayment { get; }
    }
}