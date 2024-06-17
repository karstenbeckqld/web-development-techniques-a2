using System;
using Xunit;
using MCBA.Models;
using MCBA.Interfaces;
using MCBA.Factories;
using Microsoft.EntityFrameworkCore;
using MCBA.Data;

namespace MCBA.Tests.ModelTests
{
    public class TransactionTests
    {
        private readonly ITransactionFactory _transactionFactory;
        private TestTools _testTools;
        private readonly MCBAContext _context;


        public TransactionTests()
        {
            _transactionFactory = new TransactionFactory();
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
        }

        // this test will create some transactions
        [Theory]
        [InlineData(1, 'D', 123, null, 500.00, "Test deposit")]
        [InlineData(2, 'W', 456, null, 200.00, "Test withdraw")]
        public void CreateTransaction_ValidParameters(int transactionID, char transactionType, int accountNumber, int? destinationAccountNumber, decimal amount, string comment)
        {
            // Act
            ITransaction transaction = _transactionFactory.CreateTransaction(transactionID, transactionType, accountNumber, destinationAccountNumber, amount, comment, DateTime.UtcNow);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(transactionID, transaction.TransactionID);
            Assert.Equal(transactionType, transaction.TransactionType);
            Assert.Equal(accountNumber, transaction.AccountNumber);
            Assert.Equal(destinationAccountNumber, transaction.DestinationAccountNumber);
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(comment, transaction.Comment);
            Assert.Equal(DateTime.UtcNow.Date, transaction.TransactionTimeUTC.Date); 
        }


        // this test will attempt to create Transactions with various invalid parameters
        [Theory]
        [InlineData(1, 'W',123, null, -100.00, null)] // Invalid amount
        [InlineData(2, 'D', 123, null, 500.00, "This comment is too long, it should exceed 30 characters.")] // Invalid comment length
        [InlineData(1, 'X', 123, null, -100.00, null)] // Invalid transactiontype
        [InlineData(2, 'D', 123, 123, 500.00, "comment")] // account number and destinationn acc num == same
        public void CreateTransaction_InvalidParameters(int transactionID, char transactionType, int accountNumber, int? destinationAccountNumber, decimal amount, string comment)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                _transactionFactory.CreateTransaction(transactionID, transactionType, accountNumber, destinationAccountNumber, amount, comment, DateTime.UtcNow));
        }

        // this test will check that there is a transaction in the database
        [Fact]
        public void check_If_transactionInDB()
        {
            // Arrange
            using var context = _testTools.GetSeedData(_testTools.getContext());


            var retrievedTransaction = context.Transaction.FirstOrDefault(p => p.TransactionID == 1);

            // Assert
            Assert.NotNull(retrievedTransaction); 
            Assert.Equal(4100, retrievedTransaction.AccountNumber); 
            Assert.Equal(100m, retrievedTransaction.Amount);

        }

        //this test will create a Transaction add it to the database and then ensure that it has been properly added
        [Fact]
        public void Add_Transaction_Checkif_inDb()
        {
            // Arrange

   
            var newTransaction = new Transaction

            {
               TransactionID = 1000,
               AccountNumber = 4100,
               DestinationAccount = null,
               Amount = 200m,
               Comment = "test tx",
               TransactionTimeUTC = DateTime.UtcNow,
               TransactionType = 'D'

            };
            //Act
            _context.Transaction.Add(newTransaction);
            _context.SaveChanges();

            var retrievedTransaction = _context.Transaction.FirstOrDefault(p => p.TransactionID == 1000);

            // Assert
            Assert.NotNull(retrievedTransaction); 
            Assert.Equal(200m, retrievedTransaction.Amount); 
            Assert.Equal(4100, retrievedTransaction.AccountNumber); 
        }
    }
}