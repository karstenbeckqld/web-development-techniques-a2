using X.PagedList;

namespace MCBA.Models;

public class TransactionsViewModel
{
    public List<Account> Accounts { get; set; }
    public IPagedList<Transaction> Transactions { get; set; }
    public int NumPages { get; set; }
    public int? AccountNumber { get; set; }
    public string SortOrder { get; set; }
    public int CustomerID { get; set; }
    public int Page { get; set; }
}