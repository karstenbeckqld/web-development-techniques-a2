# Readme

This file contains the documentation for Assignment 2, Web Development Technologies, SP2 2023.

URL to GitHub Repository: https://github.com/rmit-wdt-sp2-2023/s3909581-s3893749-s3912792-a2

## Contributing Students:

Karsten Beck, Student ID: s3912792   
Jack George Harris, Student ID: s3893749   
Maxwell Trounce, Student ID: s3909581


## The MCBA Project:
The MCBA project contains the main functionality for the ASP.NET MVC application, comprising all relevant models, views
and controllers. The models mirror the database tables in the following way:

Customer.cs -> Customer table   
Login.cs -> Login table   
Account.cs -> Account table   
Transaction.cs -> Transaction table   
BillPay.cs -> BillPay table   
Payee.cs -> Payee table   
Plus required view models to display data appropriately:   
ErrorViewModel, CustomerViewModel, LoginViewModel, TransactionsViewModel and TransferViewModel

We could not implement the class library from assignment 1 for any database related activity as it contained its own ORM
and wasn't compatible with the requirements for not having any SQL code; using only the Entity Framework. We were,
however, able to reuse the dependency injection from Assignment 1, as well as a majority of the web service code for
this assignment.

#### Dependency Injection:
```csharp
public interface IBalanceCheck
{
    public bool CheckMinBalance(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge);
}


public class BalanceValidationLogic
{
    private readonly IBalanceCheck _balanceCheck;

    public BalanceValidationLogic(IBalanceCheck balanceCheck)
    {
        _balanceCheck = balanceCheck;
    }

    public BalanceValidationLogic()
    {
        _balanceCheck = new BalanceValidator();
    }

    public bool PerformBalanceValidation(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge)
    {
        return _balanceCheck.CheckMinBalance(sourceBalance, accountType, amount, serviceCharge);
    }
}


 public TransactionController(MCBAContext context)
    {
        _context = context;
        _balanceValidationLogic = new BalanceValidationLogic(new BalanceValidator());
    }
```

#### Web Service
```csharp
public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            //Seed Data example referenced from Web Dev Tutorials
            using var context = new MCBAContext(serviceProvider.GetRequiredService<DbContextOptions<MCBAContext>>());

            // Look for customers.
            if (context.Customer.Any())
                return; // DB has already been seeded.

            using var client = new HttpClient();
            
            var json = client.GetStringAsync("https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/").Result;

            List<Customer> jsonData = JsonConvert.DeserializeObject<List<Customer>>(json, new JsonSerializerSettings
            {
                DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
            });

            foreach (var customer in jsonData)
            {
                customer.ProfilePicture = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + "/Models/test.jpg");
                context.Customer.Add(customer);
                context.Login.Add(customer.login);

                foreach (var account in customer.Accounts)
                {
                    account.Balance = 0;
                    account.Balance += account.Transactions.Sum(transaction => transaction.Amount);

                    context.Add(account);

                    foreach (var transaction in account.Transactions)
                    {
                        transaction.TransactionType = 'D';
                        transaction.AccountNumber = account.AccountNumber;
                        transaction.TransactionTimeUTC = TimeZoneInfo.ConvertTimeToUtc(transaction.TransactionTimeUTC);

                        context.Transaction.Add(transaction);
                    }
                }
            }
        }
    }
```

## View Models

### ErrorViewModel
The ErrorViewModel gets used by the HomeController:
```csharp
 [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
```
It is created when an error occurs while executing an IActionResult issued by a controller during running of the
application.

### CustomerViewModel
The CustomerViewModel is required to enable a customer to view/edit their details and to update their profile picture,
as well as change their password. Because the latter requires accessing two different tables in the database, the view
model got created.
```csharp
public class CustomerViewModel
{
    public int CustomerId { get; set; }
    
    [Display(Name="Current Password")]
    public string PasswordHash { get; set; }
    
    public string Name { get; set; }
    
    [...]
    
    public string NewPassword { get; set; }
    
    public string NewPasswordConfirm { get; set; }
    
    public byte[] ProfilePicture { get; set; }
```
This model allows the application to send and receive login data, as well as customer data to and from a view. In
particular, the current and newly created password when a user changes their password and the selected profile picture.

### LoginViewModel
The LoginViewModel gets used by the LoginController's POST method to receive data from the corresponding view to enable
customer login.
```csharp
 [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
     if (ModelState.IsValid)
        {
            var login = await _context.Login.FindAsync(loginViewModel.LoginID);

            if (login is null || string.IsNullOrEmpty(loginViewModel.PasswordHash) ||
                !_sSimpleHash.Verify(loginViewModel.PasswordHash, login.PasswordHash))
            {
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View();
            }

            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.Name), login.Customer.Name);
        }
    }
```

### TransactionsViewModel
The TransactionViewModel gets used to display the statements page.
```csharp
public class TransactionsViewModel
{
    public List<Account> Accounts { get; set; }
    public IPagedList<Transaction> Transactions { get; set; }
    public int NumPages { get; set; }
    public int? AccountNumber { get; set; }
}
```
Because we use a paged approach for this display, it sends a paged list to the view, including the number of pages, plus
a list of accounts (it only sends one account, but the approach with List&lt;Account&gt; seemed the most reasonable to
implement) to fulfil the requirement to also display the account balance in the statements view.

### TransferViewModel
A requirement was to display a confirmation page for each type of transfer (Deposit, WithDraw, Transfer). Therefore, we
implemented the TransferViewModel. It gets used by the DepositController, WithdrawController and TransferController to
receive view data from the corresponding views.

```csharp
[HttpPost]
    public async Task<IActionResult> Index(TransferViewModel transferViewModel)
    {
        var account = await _context.Account.FindAsync(transferViewModel.ID);
        var destinationAccount = await _context.Account.FindAsync(transferViewModel.DestinationAccountId);

       [...]
        
        if (!ModelState.IsValid)
        {
            ViewBag.Amount = transferViewModel.Amount;
            ViewBag.Comment = transferViewModel.Comment;
            ViewBag.DestinationAccountId = transferViewModel.DestinationAccountId;
            return View(transferViewModel);
        }

        return RedirectToAction("Index","Confirmation", transferViewModel);
    }
```
This way, information from the Account and Transaction tables can get combined and send on to the ConfirmationController.
```csharp
public class TransferViewModel
{
    public int? ID { get; set; }
    
    [Column(TypeName = "money"), DataType(DataType.Currency)]
    public decimal Amount { get; set; }
    
    [StringLength(30)]
    public string Comment { get; set; }
    
    public char AccountType { get; set; }
    
    public Account Account { get; set; }
    
    public Account DestinationAccount { get; set; }
    
    public int? DestinationAccountId { get; set; }
    public string ControllerName { get; set; }
}
```
Because the ConfirmationController receives data from the Deposit-, Withdraw- and TransferControllers, we added a value
in the view model, named ControllerName, which lets the ConfirmationController decide which transaction type to execute.

```csharp
  switch (transferViewModel.ControllerName)
            {
                case "Deposit":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.DepositType,
                        transferViewModel,
                        transferViewModel.Account,
                        null);
                    break;
                case "Withdraw":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.WithdrawType,
                        transferViewModel,
                        transferViewModel.Account,
                        null);
                    break;
                case "Transfer":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.TransferType,
                        transferViewModel,
                        transferViewModel.Account,
                        transferViewModel.DestinationAccount);
                    break;
            }
```

### BillPayViewModel and PayeeViewModel
Because of the requirement of displaying the profile image in the menu bar at all times, we had to implement the 
BillPayViewModel and the PayeeViewModel. THis allowed us to pass around the Customer ID value, so that Layout.cshtml 
receives a reference to the customer object and can display the image.   
These view models are simply copies of their respective models with the CustomerID property added.

## Utilities
For specific tasks we created some utility classes.
### AuthoriseCustomerAttribute
```csharp
public class AuthoriseCustomerAttribute: Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var customerId = context.HttpContext.Session.GetInt32(nameof(Customer.CustomerID));
        if (!customerId.HasValue)
        {
            context.Result = new RedirectToActionResult("Login", "Login", null);
        }
    }
}
```
The AuthoriseCustomerAttribute gets applied to the CustomerController to ensure only a logged in customer can access
their data. This class has been adapted from the lecture 6 code, kindly provided by Dr Mathhew Bolger.

### ChargeTypes
ChargeTypes is solely a central storage class for the service charge amounts required by the business rules.

### TransactionTypes
TransactionTypes provides a central storage for the various transaction types used during the application. As we decided
not to alter the database schema to accommodate for integers and not chars, we've implemented this class instead of an
enum.

### ExtensionMethods
The ExtensionMethods class provides some handy methods to perform validations and has been kindly provided by Dr Matthew
Bolger during the lectures and tutorials. We've added the GetDisplayName method to augment the BillPays and Customer
classes.
```csharp
 public static string GetDisplayName(this Enum value)
    {
        return value.GetType()
          .GetMember(value.ToString())
          .First()
          .GetCustomAttribute<DisplayAttribute>()
          ?.GetName();
    }
```

### ExecuteTransaction
The ExecuteTransaction class is the central hub to execute any transaction (Deposit, Withdraw, Transfer) on the database.
It's central functionality is a switch statement that decides, which transaction to perform.
```csharp
  switch (toggle)
        {
            case TransactionTypes.DepositType:
                account.AddAmount(transferViewModel.Amount);
                account.AddTransaction(transferViewModel.Amount, 
                    TransactionTypes.DepositType,
                    transferViewModel.Comment, 
                    null);
                break;
            case TransactionTypes.WithdrawType:
                if (numOfTransactions >= 2)
                {
                    account.WithdrawAmount(ChargeTypes.WithdrawServiceCharge);
                    account.AddTransaction(ChargeTypes.WithdrawServiceCharge, 
                        TransactionTypes.ServiceChargeType,
                        "Service Charge", 
                        null);
                }

                account.WithdrawAmount(transferViewModel.Amount);
                account.AddTransaction(transferViewModel.Amount, 
                    TransactionTypes.WithdrawType,
                    transferViewModel.Comment,
                    null);
                break;
            case TransactionTypes.TransferType:
                if (numOfTransactions >= 2)
                {
                    account.WithdrawAmount(ChargeTypes.TransferServiceCharge);
                    account.AddTransaction(ChargeTypes.TransferServiceCharge, 
                        TransactionTypes.ServiceChargeType,
                        "Service Charge", 
                        null);
                }

                account.WithdrawAmount(transferViewModel.Amount);
                account.AddTransaction(transferViewModel.Amount, 
                    TransactionTypes.TransferType,
                    transferViewModel.Comment, 
                    destinationAccount);

                if (destinationAccount is not null)
                {
                    destinationAccount.AddTransaction(transferViewModel.Amount, 
                        TransactionTypes.TransferType, 
                        transferViewModel.Comment, 
                        null);
                    destinationAccount.AddAmount(transferViewModel.Amount);
                }
                break;
```

## Controllers

### TransactionController
The TransactionController is the parent class of the Deposit-, Withdraw- and TransferControllers. It provides the SetTransferViewModelProperties
method, the CheckBalance method, the CheckInput method and the Sort method to its child classes. The SetTransferViewModelProperties
method sets specific parameters for the TransferViewModel, so that the can get passed on to the ConfirmationController.
```csharp
 protected static TransferViewModel SetTransferViewModelProperties(int? id, Account account,
        TransferViewModel transferViewModel)
    {
        transferViewModel.ID = id;
        transferViewModel.Account = account;
        transferViewModel.AccountType = account.AccountType;
        return transferViewModel;
    }
```
The CheckInput method checks for input validating against the business rules and sets the ModelState accordingly.
```csharp
 public void CheckInput(TransferViewModel transferViewModel)
    {
        if (transferViewModel.Amount <= 0)
        {
            ModelState.AddModelError("Error", "The entered amount must be positive.");
        }

        if (transferViewModel.Amount.HasMoreThanTwoDecimalPlaces())
        {
            ModelState.AddModelError("Error", "The entered amount cannot have more than 2 decimal places.");
        }

        if (transferViewModel.Comment is not null && transferViewModel.Comment.Length > 30)
        {
            ModelState.AddModelError("Error", "The comment cannot exceed 30 characters.");
        }
    }
```
The CheckBalance method uses the dependency injected BalanceValidator to determine if the amounts withdrawn or transferred
don't violate the business rules.
```csharp
public class BalanceValidator : IBalanceCheck
{
    public bool CheckMinBalance(decimal sourceBalance, string accountType, decimal amount, decimal serviceCharge)
    {
        var result = true;

        switch (accountType)
        {
            case "C" when sourceBalance - amount - serviceCharge < 300:
                result = false;
                throw new InsufficientFundsException("Transfer not allowed. Account balance must not go below $300.");
            case "S" when sourceBalance - amount - serviceCharge < 0:
                result = false;
                throw new InsufficientFundsException("Transfer not allowed. Account balance must not go below $0.");
            default:
                return result;
        }
    }
}
```
It returns a Dictionary with the associated error message, so that the child class can
display an appropriate message. The message gets determined by the BalanceValidator calling the InsufficientFundsException.
```csharp
public Dictionary<string,string> CheckBalance(TransferViewModel transferViewModel, Account account, decimal serviceCharge)
    {
        var result = new Dictionary<string,string>();
       
        try
        {
            var balanceValidation = _balanceValidationLogic.PerformBalanceValidation(
                account.Balance,
                account.AccountType.ToString(),
                transferViewModel.Amount,
                serviceCharge);
        }
        catch (InsufficientFundsException e)
        {
            result.Add("Error",e.Message);
        }

        return result;
    }
```
For the StatementController the TransactionController provides a Sort method that aids in sorting the statements by date
ascending and descending.
```csharp
protected List<Transaction> Sort(List<Transaction> transactionsList, string sortType)
    {
        if (sortType.Equals("asc"))
        {
            transactionsList = transactionsList
                .OrderBy(t => t.TransactionTimeUTC)
                .ToList();
        }

        if (sortType.Equals("desc"))
        {
            transactionsList = transactionsList
                .OrderByDescending(t => t.TransactionTimeUTC)
                .ToList();
        }

        return transactionsList;
    }
```
### DepositController
The DepositController collects all data to deposit an amount into a bank account. It is linked to the account that it
gets called from, performs input checks according to the business rules and redirects the user to the ConfirmationController
to deposit money into the account.

### WithdrawController
The WithdrawController inherits from the TransactionController as well and otherwise works similar to the DepositController
in redirecting the customer to the Confirmation Controller.

### TransferController
The TransferController again inherits from the TransactionController. In addition to the other two siblings, it performs
more checks, ie. if the destination account number is too long, or even exists. The remainder of the class is similar to
the other two siblings in regard to redirecting the user to the ConfirmationController.
```csharp
 if (transferViewModel.DestinationAccountId.ToString().Length > 4)
        {
            ModelState.AddModelError("Error", "Account numbers must not exceed 4 digits");
        }
        
        try
        {
            destinationAccount = await _context.Account.FindAsync(transferViewModel.DestinationAccountId);
        }
        catch (Exception e)
        {
           Console.WriteLine("No destination account found: " + e);
        }

        if (destinationAccount is null)
        {
            ModelState.AddModelError("Error", "The entered destination account number does not exist.");
        }
```

### ConfirmationController
The ConfirmationController is the central hub for the Deposit-, Withdraw- and TransferControllers and gets called from
each of these controllers for the customer to confirm their transaction. It displays the transaction to the customer and
allows for them to accept or decline it.  
If the customer accepts the transaction, the Controller uses the ControllerName property of the TransferViewModel to
determine which type of transaction to perform and then passes the required parameters to the ExecuteTransaction class.
```csharp
 if (value == "Yes")
        {
            switch (transferViewModel.ControllerName)
            {
                case "Deposit":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.DepositType,
                        transferViewModel,
                        transferViewModel.Account,
                        null);
                    break;
                case "Withdraw":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.WithdrawType,
                        transferViewModel,
                        transferViewModel.Account,
                        null);
                    break;
                case "Transfer":
                    await new ExecuteTransaction(_context).Execute(
                        TransactionTypes.TransferType,
                        transferViewModel,
                        transferViewModel.Account,
                        transferViewModel.DestinationAccount);
                    break;
            }
        }
```
If the user decides to not go ahead with the transaction, a redirect to the user's home screen takes place.
```csharp
 return RedirectToAction("Index", "Customer");
```

### StatementsController
The StatementsController provides the customer a paginated list of their statements. It gets called directly from the
user's home screen and inherits from TransactionController to use the Sort method provided there.
```csharp
        var pageNumber = page ?? 1;

        ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

        var accounts = await _context.Account
            .Include(x => x.Transactions)
            .Where(x => x.AccountNumber == accountNumber)
            .ToListAsync();

        var transactions = await _context.Transaction
            .Where(x => x.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUTC)
            .ToListAsync();

        switch (sortOrder)
        {
            case "Date":
                transactions = Sort(transactions, "asc");
                break;
            case "date_desc":
                transactions = Sort(transactions, "desc");
                break;
            default:
                transactions = Sort(transactions, "desc");
                break;
        }

        // Here we convert the transactions into a paged list.
        var pagedList = transactions.ToPagedList(pageNumber, PageSize);
        var pageCount = (int)Math.Ceiling((double)transactions.Count / PageSize);

        var data = new TransactionsViewModel
        {
            Accounts = accounts,
            Transactions = pagedList,
            NumPages = pageCount,
            AccountNumber = accountNumber
        };

        return View(data);
```
It receives the account and transaction data from the logged in user and used the X.PagedList NuGet package to compile
a paged list of these transactions. The number of items per page is set as a private variable. The controller then
accumulates the data in a TransactionViewModel and sends it to the view.   
In the view we use the PagedListPager HTML helper to display the page numbers to switch between the pages.
```csharp
  @Html.PagedListPager((IPagedList)Model.Transactions, page => 
  Url.Action("Index", new { accountNumber = Model.Accounts[0].AccountNumber, page }), 
  Bootstrap4PagedListRenderOptions.Default)
```

### CustomerController

The CustomerController handles any customer related actions, including password update because the Login model is a 
virtual entity of the Customer model. 
```csharp
 public class Customer : ICustomer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerID { get; set; }

        [...]
        
        [ValidateNever] public virtual Login login { get; set; }
```
Therefore, the CustomerController is able to handle the customer's information like address and TFN, as well as the 
customer's profile picture and the customer's login details. This can be achieved by using a combination of Get/Post 
methods to update data in the database. 

```csharp
 public async Task<IActionResult> Details()
    {
        var customer = await _context.Customer.Include(x => x.login)
            .FirstOrDefaultAsync(x => x.CustomerID == CustomerID);

        _customerViewModel = SetCustomerViewModelProperties(customer, _customerViewModel);

        return View(_customerViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Details([CanBeNull] string PasswordHash, [CanBeNull] string NewPassword,
        [CanBeNull] string NewPasswordConfirm)
    {
        var customerID = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

        var loginData = _context.Login.FirstOrDefaultAsync(x => x.CustomerID == customerID).Result;

        if (loginData is null) return RedirectToAction("Index", "Customer");

        var customer = await _context.Customer.FirstOrDefaultAsync(x => x.CustomerID == customerID);

        if (customer is null) return RedirectToAction("Index", "Customer");

        _customerViewModel = SetCustomerViewModelProperties(customer, _customerViewModel);

        if (PasswordHash is null ||
            string.IsNullOrEmpty(PasswordHash) ||
            !_sSimpleHash.Verify(PasswordHash, loginData.PasswordHash))
        {
            ModelState.AddModelError("CurrentPasswordError", "Current password is incorrect");
            return View(_customerViewModel);
        }

        if (NewPassword is null ||
            string.IsNullOrEmpty(NewPassword) ||
            NewPasswordConfirm is null ||
            string.IsNullOrEmpty(NewPasswordConfirm) ||
            !NewPassword.Equals(NewPasswordConfirm))
        {
            ModelState.AddModelError("PasswordMatchError", "New password doesn't match.");
            return View(_customerViewModel);
        }

        if (ModelState.IsValid)
        {
            ISimpleHash hash = new SimpleHash();
            var changedPassword = hash.Compute(NewPassword);
            loginData.ChangePassword(changedPassword);
            await _context.SaveChangesAsync();

        }

        return RedirectToAction("Details", "Customer");
    }
```
We have decided to implement the password change in the view that displays a customer's details as it was not only a 
requirement to have them editable separately from the customer's information, but also a good spot for implementation 
as the view only has to deal with one form.

Inside the Edit method is the logic to update a customer's details, including the profile picture. The DisplayImage 
method ensures that the image is displayed as a URL and not image byte data.

### BillPaysController
The BillPaysController handles all bill payment related functionality, including deleting and rescheduling bill payments 
that are either once off and issued, or repeating monthly. It also allows for the user to create a Payee.
```csharp
 public IActionResult CreatePayee() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayee(Payee payee)
    {
        if (ModelState.IsValid)
        {
            _context.Add(payee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create));
        }

        return View();
    }
```

### LoginController
The LoginController simply hanldes the login process for a customer and initiates a session.

```csharp
  public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {

        if (ModelState.IsValid)
        {
            var login = await _context.Login.FindAsync(loginViewModel.LoginID);

            if (login is null || string.IsNullOrEmpty(loginViewModel.PasswordHash) ||
                !_sSimpleHash.Verify(loginViewModel.PasswordHash, login.PasswordHash))
            {
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View();
            }

            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.Name), login.Customer.Name);
        }
        return RedirectToAction("Index", "Customer");
    }
```

Once the login process is successfully completed, the customer is redirected to its accounts overview page.

### BillPayBackgroundService
This controller handles the automatic execution of bill payments in the background while the server is running.

# MCBA.Tests
THis project contains the unit tests for the MCBA project. We test every controller in here apart from the 
HomeController, the BillPayBackgroundService and the DebugBarViewComponents as testing is not applicable for this 
controllers or not required as in the case of the DebugBarViewComponents. 
We have generated a total of 126 unit tests for this project, covering many edge cases and almost every necessary method.

# Admin Web API
Our Web API consists of two projects, the WebAPI project containing all relevant models and controllers, and the 
WebAPIPortal project, containing the controllers and views for the website.