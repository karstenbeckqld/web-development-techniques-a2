using System;
using System.Collections.Generic;
using Xunit;
using MCBA.Models;
using MCBA.Enums;
using MCBA.Interfaces;
using MCBA.Factories;
using Humanizer;
using MCBA.Data;

namespace MCBA.Tests.ModelTests
{
    public class AccountTests : BaseTest
    {
        private readonly IAccountFactory _accountFactory;
        private readonly ITransactionFactory _transactionFactory;
        private readonly IBillPayFactory _billPayFactory;
        private readonly TestTools _testTools;
        private readonly MCBAContext _context;

        public AccountTests()
        {
            _accountFactory = Resolve<IAccountFactory>();
            _transactionFactory = Resolve<ITransactionFactory>();
            _billPayFactory = Resolve<IBillPayFactory>();
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
        }

        // used to check the avaliable balance of accounts (savings and checking)
        [Theory]
        [InlineData('S', 500, 500)]
        [InlineData('C', 500, 200)]
        public void GetAvailableBalanceTest(char accountType, decimal balance, decimal expectedAvailableBalance)
        {
            // Arrange
            var account = new Account(1, accountType, 1, balance, new List<ITransaction>(), new List<BillPay>());

            // Act
            var availableBalance = account.GetAvailableBalance();

            // Assert
            Assert.Equal(expectedAvailableBalance, availableBalance);
        }

        // test use to simulate the chaneg in balance
        [Theory]
        [InlineData(500, 100, 600)]
        [InlineData(1000, 1000, 2000)]
        public void AddAmount_Test(decimal initialBalance, decimal amountToAdd, decimal expectedBalance)
        {
            // Arrange
            var account = new Account(1, 'S', 1, initialBalance, new List<ITransaction>(), new List<BillPay>());

            // Act
            account.AddAmount(amountToAdd);

            // Assert
            Assert.Equal(expectedBalance, account.Balance);
        }
       
        // test that will attempt to add an invalid amount to the account 
        [Theory]
        [InlineData(500, -100)]
        [InlineData(500, 0)]
        public void AddAmount_Test_Fail(decimal initialBalance, decimal amountToAdd)
        {
            // Arrange
            var account = new Account(1, 'S', 1, initialBalance, new List<ITransaction>(), new List<BillPay>());

            //Assert
            Assert.Throws<ArgumentException>(() => account.AddAmount(amountToAdd));
        }

        // test usedto simulate withdrawing from an accounts
        [Theory]
        [InlineData(500, 100, 400)]
        public void WithdrawAmount_Test(decimal initialBalance, decimal amountToWithdraw, decimal expectedBalance)
        {
            // Arrange
            var account = new Account(1, 'S', 1, initialBalance, new List<ITransaction>(), new List<BillPay>());

            // Act
            account.WithdrawAmount(amountToWithdraw);

            // Assert
            Assert.Equal(expectedBalance, account.Balance);
        }

        // test usedto simulate a failure in withdrawing an invalid amount from an account
        [Theory]
        [InlineData(500, -100)]
        public void WithDrawAmount_Test_Fail(decimal initialBalance, decimal amountToWithdraw)
        {
            // Arrange
            var account = new Account(1, 'S', 1, initialBalance, new List<ITransaction>(), new List<BillPay>());

            //Assert
            Assert.Throws<ArgumentException>(() => account.WithdrawAmount(amountToWithdraw));
        }

        // test used to add a transaction to the list of transactions in an account - no transfer
        [Theory]
        [InlineData(200, 'W', "Test withdraw")]
        [InlineData(20, 'D', "Test deposit")]
        [InlineData(200, 'D', "Test deposit")]
        public void AddTransaction_ToAccount(decimal amount, char transactionType, string comment)
        {
            // Arrange
            var account = new Account(1, 'S', 1, 500.00m, new List<ITransaction>(), new List<BillPay>());
            ;

            // Act
            account.AddTransaction(amount, transactionType, comment, null);

            // Assert
            Assert.Single(account.Transactions);
            Assert.Equal(amount, account.Transactions[0].Amount);
            Assert.Equal(transactionType, account.Transactions[0].TransactionType);
            Assert.Equal(comment, account.Transactions[0].Comment);
        }

        // test used to simulate the transferring of funds between accounts
        [Theory]
        [InlineData(200, 'T', "Test transfer", 4101)]
        [InlineData(300, 'T', "Test withdraw", null)]
        public void AddTransaction_UsingAccountNumber(decimal amount, char transactionType, string comment, int destinationAccountNumber)
        {
            // Arrange
            var account = new Account(1, 'S', 1, 200, new List<ITransaction>(), new List<BillPay>());
            var destinationAccount = new Account(destinationAccountNumber, 'C', 2, 300, new List<ITransaction>(), new List<BillPay>());

            // Act
            account.AddTransaction(amount, transactionType, comment, destinationAccount);

            // Assert
            Assert.Single(account.Transactions);
            Assert.Equal(amount, account.Transactions[0].Amount);
            Assert.Equal(transactionType, account.Transactions[0].TransactionType);
            Assert.Equal(comment, account.Transactions[0].Comment);
            Assert.Equal(destinationAccountNumber, account.Transactions[0].DestinationAccount?.AccountNumber);
        }

        // test used to create an account with vlaid paramaters
        [Theory]
        [InlineData(1, 'S', 1, 1000.00, true)]
        [InlineData(2, 'C', 2, 5000.00, false)]
        public void CreateAccount_ValidParameters(int accountNumber, char accountType, int customerID, decimal balance, bool hasTransactions)
        {
            // Arrange
            List<ITransaction> transactions = new List<ITransaction>();
            List<BillPay> bills = new List<BillPay>();

            if (hasTransactions)
            {
                transactions.Add(_transactionFactory.CreateTransaction(1, 'D', accountNumber, null, 500.00m, "Deposit", DateTime.UtcNow));
                transactions.Add(_transactionFactory.CreateTransaction(2, 'W', accountNumber, null, 200.00m, "Withdrawal", DateTime.UtcNow));
            }

            // Act
            var account = new Account(accountNumber, accountType, customerID, balance, transactions, bills);

            // Assert
            Assert.NotNull(account);
            Assert.Equal(accountNumber, account.AccountNumber);
            Assert.Equal(accountType, account.AccountType);
            Assert.Equal(customerID, account.CustomerID);
            Assert.Equal(balance, account.Balance);
            Assert.Equal(transactions, account.Transactions);
            Assert.Equal(bills, account.Bills);
        }

        // test used to create an account with some kind of invalid paramater
        [Theory]
        [InlineData(1, 'X', 1, 1000.00, true)]// invalid acctype
        [InlineData(2, 'C', 0, 5000.00, false)]// invalid customerId
        [InlineData(2, 'C', 0, -5000.00, false)]// invalid balance
        public void CreateAccount_InvalidParameters(int accountNumber, char accountType, int customerID, decimal balance, bool hasTransactions)
        {
            // Arrange
            List<ITransaction> transactions = new List<ITransaction>();
            List<BillPay> bills = new List<BillPay>();

            if (hasTransactions)
            {
                // Create some sample transactions
                transactions.Add(_transactionFactory.CreateTransaction(1, 'D', accountNumber, null, 500.00m, "Deposit", DateTime.UtcNow));
                transactions.Add(_transactionFactory.CreateTransaction(2, 'W', accountNumber, null, 200.00m, "Withdrawal", DateTime.UtcNow));
            }

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Account(accountNumber, accountType, customerID, balance, transactions, bills));
        }

        // test used to add a Billpayment with valid paramaters
        [Theory]
        [InlineData(100, 'B', "Bill payment 1")]
        [InlineData(200, 'B', "Bill payment 2")]
        public void AddBillPayment_Test(decimal amount, char transactionType, string comment)
        {
            // Arrange
            var account = new Account(1, 'S', 1, 500.00m, new List<ITransaction>(), new List<BillPay>());
            var payee = new Payee { PayeeID = 1, Name = "Utility Company" };
            var billPay = _billPayFactory.CreateBillPay(1, account.AccountNumber, payee.PayeeID, amount, DateTime.UtcNow, 'O', false);

            // Act
            account.Bills.Add(billPay);
            account.AddTransaction(amount, transactionType, comment, null);

            // Assert
            Assert.Single(account.Transactions);
            Assert.Equal(amount, account.Transactions[0].Amount);
            Assert.Equal(transactionType, account.Transactions[0].TransactionType);
            Assert.Equal(comment, account.Transactions[0].Comment);
            Assert.Null(account.Transactions[0].DestinationAccount);
            Assert.Contains(billPay, account.Bills);
        }

        // test used to attempt to add a billpay to the list of transactions with invalid paramteters
        [Theory]
        [InlineData(0, 1, 2, 100, 'O', false)]// invalid billID
        [InlineData(1, 0, 2, 100, 'M', false)]// invalid aaccnum
        [InlineData(1, 1, 0, 100, 'O', false)]// invalid payeeID
        [InlineData(1, 1, 1, -100, 'M', false)]// invalid amount
        [InlineData(1, 1, 1, 100, 'X', false)] // invalid acc type
        public void AddBillPayment_Test_Invalidparamaters(int billID, int accnum, int payeeID, decimal amount, char type, bool locked)
        {
            var account = new Account(1, 'S', 1, 500.00m, new List<ITransaction>(), new List<BillPay>());

            Assert.Throws<ArgumentException>(() => account.Bills.Add(new BillPay(billID, accnum, payeeID, amount, DateTime.UtcNow, type, locked)));
        }

        // check usedto see if there is an account in the mock database
        [Fact]
        public void checkIfAccountInDB()
        {
            // Arrange

            var retrievedAccount = _context.Account.FirstOrDefault(p => p.AccountNumber == 4100);

            // Assert
            Assert.NotNull(retrievedAccount); 
            Assert.Equal('S', retrievedAccount.AccountType); 
            Assert.Equal(2100, retrievedAccount.CustomerID); 
        }


        // test used to add an account to the databae and then confirm that it has been properly added
        [Fact]
        public void Add_acc_checkif_inDb()
        {

            // Act
            var newAccount = new Account
            {
                AccountNumber = 4202,
                AccountType = 'S',
                CustomerID = 2100,
                Transactions = new List<Transaction> { new Transaction() { } }
            };

            _context.Account.Add(newAccount);
            _context.SaveChanges();

            var retrievedAccount = _context.Account.FirstOrDefault(p => p.AccountNumber == 4202);

            // Assert
       
            Assert.NotNull(retrievedAccount); 
            Assert.Equal('S', retrievedAccount.AccountType); 
            Assert.Equal(2100, retrievedAccount.CustomerID); 
        }

    }
}