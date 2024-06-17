using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace MCBA.Tests.ControllerTests
{
    public class BillPayControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly TestTools _testTools;
        private readonly MCBAContext _context;
        private readonly BillPaysController _controller;

        public BillPayControllerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
            _controller = GetController(_context);
        }

        // this test will create a Payee and will assert that the Redirect has properly occured
        [Fact]
        public async Task CreatePayeeTest_Redirect()
        {
            var payee = _testTools.CreatePayee();

            var payeeModel = new PayeeViewModel
            {
                Name = payee.Name,
                Address = payee.Address,
                City = payee.City,
                State = payee.State,
                PostCode = payee.PostCode,
                Phone = payee.Phone
            };

            var result = await _controller.CreatePayee(payeeModel) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Create", result.ActionName);

        }

        // this tests simulates the creation of an invalid Payee and checks that the model state is invalid
        [Fact]
        public async Task Create_Invalid_Payee_Test()
        {
            var payee = _testTools.CreatePayee();

            var payeeModel = new PayeeViewModel
            {
                Name = payee.Name,
                Address = payee.Address,
                City = payee.City,
                State = payee.State,
                PostCode = payee.PostCode,
                Phone = payee.Phone,
                PayeeID = 3
            };

            payeeModel.Phone = string.Empty;

            // Add model error to ModelState to simulate invalid ModelState -> without this the test would fail - this is used
            // to simulate the error that would be present as the payee.Phone string is null 

            _controller.ModelState.AddModelError("Phone", "The Name field is required.");

            // Act
            var result = await _controller.CreatePayee(payeeModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);

        }

        // This will delete the first BillPay in the database and will attempt to bull it back - this value
        // should be null
        [Fact]
        public async Task DeleteBillPay_Test()
        {
            var result = await _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var deletedBillPay = await _context.BillPay.FindAsync(1);
            Assert.Null(deletedBillPay);
        }



        // This test should never occur, however it has been included to ensure that the result would not be found
        // and the result is null.
        [Fact]
        public async Task DeleteBillPay_InvalidID()
        {
            //Act
            var result = await _controller.DeleteConfirmed(-1) as NotFoundResult;

            // Assert
            Assert.Null(result);

        }


        // This test will update the BillPay amount.
        [Fact]
        public async Task Edit_BillPay_Test()
        {
            var initialBillPay = await _context.BillPay.FirstOrDefaultAsync(x=>x.BillPayID == 1);

            var billPayModel = SetBillPayViewModelProperties(null, initialBillPay);
            billPayModel.Amount = 50m;
            //billPay.Amount = 50;

            _context.Entry(initialBillPay).State = EntityState.Detached;

            // calling edit method from controller to update the bill payment
            var result = await _controller.Edit(1, billPayModel) as RedirectToActionResult;
            
            Assert.NotNull(result);
            Assert.Equal(nameof(BillPaysController.Index), result.ActionName);
            Assert.Null(result.ControllerName);

            // getting the updated bill payment from the context
            var updatedBillPay = await _context.BillPay.FindAsync(1);

            // checking the values of the updated bill payment - only the Amount is really needed however others are
            // included to show other parameters have not been changed
            Assert.NotNull(updatedBillPay);
            Assert.Equal(1, updatedBillPay.BillPayID);
            Assert.Equal(1, updatedBillPay.PayeeID);
            Assert.Equal(50m, updatedBillPay.Amount);
            Assert.Equal('O', updatedBillPay.Period);
            Assert.False(updatedBillPay.LockedPayment);
        }



        // This test will attempt to update a billPay with an amount that is too small (0.01) causing the amount
        // to not update.
        [Fact]
        public async Task Edit_BillPay_Invalid_State()
        {
            
            var customer = await _context.Customer.FirstOrDefaultAsync(p => p.CustomerID == 2100);
            
            _controller.HttpContext.Session.SetInt32(nameof(Customer.CustomerID), customer.CustomerID);

            var originalBillPay = await _context.BillPay.FirstOrDefaultAsync(p => p.BillPayID == 1);

            var billPay = new BillPay
            {
                BillPayID = 1,
                AccountNumber = 4100,
                PayeeID = 1,
                Amount = 0m, // Invalid amount, less than or equal to 0.01m
                ScheduleDate = DateTime.Parse("2023-08-29 01:39:07"),
                Period = 'O',
                LockedPayment = false
            };

            var billPayModel = SetBillPayViewModelProperties(null, billPay);

            // Act
            var result = await _controller.Edit(1, billPayModel);

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.NotEqual(0m, originalBillPay.Amount);
            Assert.Equal("Entered amount must be greater than $0.01", 
                _controller.ModelState["Error"].Errors[0].ErrorMessage);
            
        }



        // GetController used to create the controller HTTP Context and create a new test session

        public BillPaysController GetController(MCBAContext context)
        {
            var controller = new BillPaysController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Session = new TestSession()
                    }
                }
            };
            return controller;
        }
        
        private BillPayViewModel SetBillPayViewModelProperties(int? customerId, BillPay billPayData)
        {
            var data = new BillPayViewModel
            {
                BillPayID = billPayData.BillPayID,
                AccountNumber = billPayData.AccountNumber,
                Amount = billPayData.Amount,
                PayeeID = billPayData.PayeeID,
                Account = billPayData.Account,
                ScheduleDate = billPayData.ScheduleDate,
                Period = billPayData.Period,
                LockedPayment = billPayData.LockedPayment,
                CustomerID = customerId
            };


            return data;
        }
    }
}
