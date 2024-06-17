using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace MCBA.Controllers;

// The StatementController is responsible to display the user's statements.
[AuthoriseCustomer]
public class StatementController : TransactionController
{
    private const int PageSize = 4;

    private TransactionsViewModel _transactionsViewModel;

    public StatementController(MCBAContext context) : base(context)
    {
        _transactionsViewModel = new TransactionsViewModel();
    }

    public async Task<IActionResult> Index(int? accountNumber, string sortOrder, int? page)
    {
        var pageNumber = page ?? 1;

        if (sortOrder == null)
        {
            sortOrder = "date_desc";
        }

        // To be able to sort for dates, we define a ViewBag variable and set a default value to "Date" for a default
        // sort of descending.
        // Adapted from: https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/sorting-filtering-and-paging-with-the-entity-framework-in-an-asp-net-mvc-application
        ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

        // First get the account for the provided account number.
        var accounts = await _context.Account
            .Include(x => x.Transactions)
            .Where(x => x.AccountNumber == accountNumber)
            .ToListAsync();

        // Second, get the transactions for the provided account number
        var transactions = await _context.Transaction
            .Where(x => x.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUTC)
            .ToListAsync();

        // Now we can sort the list of transactions according to the values passed by the ViewBag variables.
        transactions = SortTransactions(transactions, sortOrder);

        // Here we convert the transactions into a paged list.
        var pagedList = transactions.ToPagedList(pageNumber, PageSize);

        // So that we get a page count even for odd numbers of transactions, we round the result of transaction count
        // divided by page size up.
        var pageCount = (int)Math.Ceiling((double)transactions.Count / PageSize);

        // Now we add all values to the view model. The account number is necessary, so that the sorting action link
        // can return it to the controller.
        var data = new TransactionsViewModel
        {
            Accounts = accounts,
            Transactions = pagedList,
            NumPages = pageCount,
            CustomerID = accounts[0].CustomerID,
            AccountNumber = accountNumber,
            SortOrder = sortOrder,
            Page = pageNumber
        };

        // Only used to get access to the TransactionsViewModel for testing.
        _transactionsViewModel = data;

        return View(data);
    }

    public async Task<IActionResult> GetByPage(int? accountNumber, string sortOrder, int? page)
    {
        var pageNumber = page ?? 1;

        // Second, get the transactions for the provided account number
        var transactions = await _context.Transaction
            .Where(x => x.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUTC)
            .ToListAsync();

        transactions = SortTransactions(transactions, sortOrder);

        // Here we convert the transactions into a paged list.
        var pagedList = transactions.ToPagedList(pageNumber, PageSize);

        var output = new List<Transaction>();

        foreach (var transaction in pagedList)
        {
            var newT = new Transaction();

            newT.TransactionTimeUTC = transaction.TransactionTimeUTC;
            newT.Amount = transaction.Amount;
            newT.AccountNumber = transaction.AccountNumber;
            newT.DestinationAccountNumber = transaction.DestinationAccountNumber;
            newT.Comment = transaction.Comment;
            newT.TransactionType = transaction.TransactionType;
            newT.DestinationAccountNumber = transaction.DestinationAccountNumber;

            output.Add(newT);
        }

        return Json(output);
    }


    public async Task<IActionResult> MostRecent(int? accountNumber, int? limit, string sortOrder)
    {
        // Second, get the transactions for the provided account number
        var transactions = await _context.Transaction
            .Where(x => x.AccountNumber == accountNumber)
            .ToListAsync();

        var output = new List<Transaction>();

        int count = 0;
        foreach (var transaction in transactions)
        {
            if (count <= limit)
            {
                var newT = new Transaction();

                newT.TransactionTimeUTC = transaction.TransactionTimeUTC;
                newT.Amount = transaction.Amount;
                newT.AccountNumber = transaction.AccountNumber;
                newT.DestinationAccountNumber = transaction.DestinationAccountNumber;
                newT.Comment = transaction.Comment;
                newT.TransactionType = transaction.TransactionType;
                newT.DestinationAccountNumber = transaction.DestinationAccountNumber;

                output.Add(newT);
                count++;
            }
        }

        return Json(SortTransactions(output, sortOrder));
    }

    public async Task<IActionResult> GetBalanceUptoPage(int? accountNumber, int? page, string sortOrder)
    {
        //This method and chart does not support reversing!
        if (sortOrder == "date_desc")
        {
            return Json(0);
        }

        var pageNumber = page ?? 1;

        pageNumber -= 1;

        int limit = pageNumber * PageSize;

        // Second, get the transactions for the provided account number
        var transactions = await _context.Transaction
            .Where(x => x.AccountNumber == accountNumber)
            .ToListAsync();

        var count = 0;
        decimal balance = 0;

        while (count < limit & count < transactions.Count)
        {
            if (transactions[count].TransactionType == 'D')
            {
                balance += transactions[count].Amount;
            }
            else if (transactions[count].TransactionType == 'T' &&
                     transactions[count].DestinationAccountNumber == null)
            {
                balance += transactions[count].Amount;
            }
            else
            {
                balance -= transactions[count].Amount;
            }

            count++;
        }


        return Json(balance);
    }


    private List<Transaction> SortTransactions(List<Transaction> transactions, String order)
    {
        switch (order)
        {
            case "Date":
                transactions = Sort(transactions, "asc");
                break;
            case "date_desc":
                transactions = Sort(transactions, "desc");
                break;
            default:
                transactions = Sort(transactions, "asc");
                break;
        }

        return transactions;
    }


    // The below method is only used for testing purposes to gain access to the TransactionsViewModel.
    public TransactionsViewModel GetResultData()
    {
        return _transactionsViewModel;
    }
}