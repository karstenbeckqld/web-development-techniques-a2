using MCBA.Interfaces;
using MCBA.Models;
using MCBA.Factories;
using SimpleHashing.Net;
using Newtonsoft.Json;
using MCBA.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace MCBA.Tests
{
    // this class is used to create a 'Live' test customer that has transactions, billpay, login 
    public class TestTools
    {
        private readonly DbContextOptions<MCBAContext> _dbContextOptions;

        private readonly IPayeeFactory _payeeFactory;
        private readonly ICustomerFactory _customerFactory;
        private readonly IAccountFactory _accountFactory;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IBillPayFactory _billPayFactory;
        private readonly ILoginFactory _loginFactory;

        // Importing and setting factories -> in memory database
        public TestTools()
        {
             _payeeFactory = new PayeeFactory();
            _accountFactory = new AccountFactory(); 
            _transactionFactory = new TransactionFactory();
            _billPayFactory = new BillPayFactory();
            _loginFactory = new LoginFactory();
            _customerFactory = new CustomerFactory(_accountFactory);

            _dbContextOptions = new DbContextOptionsBuilder<MCBAContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        }

        // used to create a list of accounts
        public List<IAccounts> CreateMultipleAccounts()
        {
            var accountList = new List<IAccounts>();

            // Hardcoded account data
            var accountDataList = new List<(int accountNumber, char accountType, int customerID, decimal balance, bool hasTransactions)>
            {
                (4100, 'S', 1, 1000.00m, true),
                (4101, 'C', 1, 5000.00m, false)
            };

            foreach (var accountData in accountDataList)
            {
                var account = CreateAccount(accountData.accountNumber, accountData.accountType, accountData.customerID, accountData.balance);
                accountList.Add(account);
            }

            return accountList;
        }

        // used to Create an account with sample data
        public Account CreateAccount(
           int accountNumber = 4100, char accountType = 'C', int customerID = 1, decimal balance = 100m, bool hasTransactions = true)
        {
            List<ITransaction> transactions = null;
            List<BillPay> bills = null;

            if (hasTransactions)
            {
                transactions = new List<ITransaction>
                {
                    _transactionFactory.CreateTransaction(4100, 'D', accountNumber, null, 500.00m, "Deposit", DateTime.UtcNow),
                    _transactionFactory.CreateTransaction(4101, 'W', accountNumber, null, 200.00m, "Withdrawal", DateTime.UtcNow)
                };

                bills = new List<BillPay>()
                {
                    _billPayFactory.CreateBillPay(1, 4100, 1, 100.50m, DateTime.UtcNow.AddMinutes(5), 'O', true),
                    _billPayFactory.CreateBillPay(2, 4101, 2, 50m, DateTime.UtcNow.AddMinutes(10), 'M', false)
                };
            }
            return (Account)_accountFactory.CreateAccount(accountNumber, accountType, customerID, balance, transactions, bills);
        }


        // used to create a sample login
        public Login CreateLogin()
        {
            ISimpleHash simpleHash = new SimpleHash();
            string hashedPassword = simpleHash.Compute("abc123");

            var login = (Login)_loginFactory.CreateLogin("12345677", 5000, hashedPassword, false);

            return login;
        }


        // used to create a sample payee
        public Payee CreatePayee()
        {
            return (Payee)_payeeFactory.CreatePayee(3, "water bill", "123 st", "Town", "NSW", "2000", "(04)12 345 678");
        }

        // used to load seed data into the database - for testings
        public MCBAContext GetSeedData(MCBAContext context)
        {
            if (context.Customer.Any())
                return context; // DB has already been seeded.


            using var client = new HttpClient();

            // Now we obtain the JSON string by calling the GetStringAsync() method on the client and providing the url of
            // the web service.
            var json = client.GetStringAsync("https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/").Result;


            List<Customer> jsonData = JsonConvert.DeserializeObject<List<Customer>>(json, new JsonSerializerSettings
            {
                DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
            });

            foreach (var customer in jsonData)
            {
                customer.ProfilePicture = null;
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

            // adding sample Billpays
            context.BillPay.AddRange(
                new BillPay
                {
                    AccountNumber = 4100,
                    PayeeID = 1,
                    Amount = 100m,
                    ScheduleDate = DateTime.Parse("2023-08-29 01:39:07"),
                    Period = 'O',
                    LockedPayment = false

                },
                new BillPay
                {
                    AccountNumber = 4100,
                    PayeeID = 1,
                    Amount = 25m,
                    ScheduleDate = DateTime.Parse("2023-10-29 01:39:07"),
                    Period = 'M',
                    LockedPayment = false
                }) ;
            // adding sample Payees
            context.Payee.AddRange(
                new Payee
                {
                    Name = "PowerBill",
                    Address = "1 Power Street",
                    City = "Brisbane",
                    State = "QLD",
                    PostCode = "1111",
                    Phone = "(04)11 222 333"
                },
                new Payee
                {
                    Name = "WaterBill",
                    Address = "2 Water Street",
                    City = "Brisbane",
                    State = "QLD",
                    PostCode = "1111",
                    Phone = "(04)11 333 111"
                });
            // /saving and returning the context
            context.SaveChanges();

            return context;
        }

        public MCBAContext getContext() { return new MCBAContext(_dbContextOptions); }
    }

    // creating a sample Test session - used in various controller methods
    // Reference:
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.isession?view=aspnetcore-7.0
    public class TestSession : ISession
        {
            private Dictionary<string, byte[]> _sessionData = new Dictionary<string, byte[]>();

            // Set a unique identifier for the session.
            public string Id { get; } = "TestId"; 

            // Assume the session is always available.
            public bool IsAvailable => true; 

            public IEnumerable<string> Keys => _sessionData.Keys;
            
        // clears the session
            public void Clear()
            {
                _sessionData.Clear();
            }

            public Task CommitAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask; 
            }

            public Task LoadAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask; 
            }

            public void Remove(string key)
            {
                _sessionData.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                _sessionData[key] = value;
            }

            public bool TryGetValue(string key, out byte[] value)
            {
                return _sessionData.TryGetValue(key, out value);
            }
        }
}
    