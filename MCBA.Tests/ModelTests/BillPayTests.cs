using System;
using Xunit;
using Autofac;
using MCBA.Interfaces;
using MCBA.Models;
using static System.Net.Mime.MediaTypeNames;
using MCBA.Controllers;
using MCBA.Data;

namespace MCBA.Tests.ModelTests
{
    public class BillPayTests : BaseTest
    {
        private readonly IBillPayFactory _billPayFactory;
        private readonly TestTools _testTools;
        private readonly MCBAContext _context;
   
        public BillPayTests()
        {
            // Resolve the IBillPayFactory using Autofac
            _billPayFactory = Resolve<IBillPayFactory>();
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
        }

        // test used to create a billpay with valid paramaters
        [Theory]
        [InlineData(1, 1001, 2001, 100.50, "2023-08-10", 'O', true)]
        [InlineData(2, 1002, 2002, 50.75, "2023-08-15", 'M', false)]
        public void CreateBillPay_ValidParameters(int billPayID, int accountNumber, int payeeID, decimal amount, string scheduleDate, char period, bool lockedPayment)
        {
            // Arrange
            DateTime scheduleDateTime = DateTime.Parse(scheduleDate);

            // Act
            var billPay = _billPayFactory.CreateBillPay(billPayID, accountNumber, payeeID, amount, scheduleDateTime, period, lockedPayment);

            // Assert
            Assert.NotNull(billPay);
            Assert.Equal(billPayID, billPay.BillPayID);
            Assert.Equal(accountNumber, billPay.AccountNumber);
            Assert.Equal(payeeID, billPay.PayeeID);
            Assert.Equal(amount, billPay.Amount);
            Assert.Equal(scheduleDateTime, billPay.ScheduleDate);
            Assert.Equal(period, billPay.Period);
            Assert.Equal(lockedPayment, billPay.LockedPayment);
        }

        //test used to attempt to create a billpay with incorrect paramaters
        [Theory]
        [InlineData(1, 0, 2001, 100.50, "02/01/2023 08:45:00 PM", 'O', true)] // invalid accountNumber
        [InlineData(2, 1002, 0, 50.75, "2023-08-15", 'Z', false)] // invalid period
        [InlineData(3, 1003, 2003, -50.75, "2023-08-20", 'O', false)] // invalid amount
        public void CreateBillPay_InvalidParameters(int billPayID, int accountNumber, int payeeID, decimal amount, string scheduleDate, char period, bool lockedPayment)
        {
            // Arrange
            DateTime scheduleDateTime = DateTime.Parse(scheduleDate);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _billPayFactory.CreateBillPay(billPayID, accountNumber, payeeID, amount, scheduleDateTime, period, lockedPayment));
        }

        // test usedto check if a billpay exists in the database
        [Fact]
        public void checkIfBillPayInDB()
        {
            // Arrange
            using var context = _testTools.GetSeedData(_testTools.getContext());
            var retrievedBillPay = context.BillPay.FirstOrDefault(p => p.AccountNumber == 4100);

            // Assert
            Assert.NotNull(retrievedBillPay); 
            Assert.Equal(4100, retrievedBillPay.AccountNumber);
            Assert.Equal('O', retrievedBillPay.Period); 
        }

        // test used to create a billpay and add it to the mock database and then check that it has been properly added
        [Fact]
        public void Add_BillPay_Checkif_inDb()
        {
         
           //Arrange
            var newBillPay = new BillPay
            
            {
                AccountNumber = 4202,
                PayeeID = 1,
                Amount = 100m,
                ScheduleDate = DateTime.Parse("2023-08-29 01:39:07"),
                Period = 'O',
                LockedPayment = false
            };
            // Act
            _context.BillPay.Add(newBillPay);
            _context.SaveChanges();

            var retrievedBillPay = _context.BillPay.FirstOrDefault(p => p.AccountNumber == 4202);

            // Assert
            Assert.NotNull(retrievedBillPay); 
            Assert.Equal('O', retrievedBillPay.Period); 
            Assert.Equal(100m, retrievedBillPay.Amount); 
        }

    }
}