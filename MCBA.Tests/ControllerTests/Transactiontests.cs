using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MCBA.Tests.ControllerTests
{
    public class Transactiontests
    {
        private readonly TestTools _testTools;
        private readonly MCBAContext _context;
        private readonly TransferController _controller;

        public Transactiontests()
        {
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
            _controller = GetController(_context);
        }

        [Fact]
        public async Task test()
        {
         
            var customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
            _controller.HttpContext.Session.SetInt32(nameof(Customer.CustomerID), customer.CustomerID);

            var transferViewModelData = new TransferViewModel
            {
                ID = 4100,
                Amount =10,
                Comment ="test",
                AccountType ='S',
                Account = customer.Accounts[0],
                DestinationAccount = customer.Accounts[1],
                DestinationAccountId = customer.Accounts[1].AccountNumber,
                ControllerName = "Transfer"
            };

            var result = await _controller.Index(transferViewModelData, 'S');

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Confirmation", redirectResult.ControllerName);

        }


        public TransferController GetController(MCBAContext context)
        {
            var controller = new TransferController(context)
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

    }
}
