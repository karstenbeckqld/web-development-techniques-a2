using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA.Factories;
using Xunit;
using Microsoft.AspNetCore.Http;

using SimpleHashing.Net;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MCBA.Tests.ControllerTests
{
    public class LoginControllerTests
    {
        private readonly TestTools _testTools;
        private readonly MCBAContext _context;
        private readonly LoginController _controller;

        public LoginControllerTests()
        {
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
            _controller = GetController(_context);


        }

        // this test will log the user in and will ensure that the "user" has been redirected to the customer index page
        [Fact]
        public async Task Login_ValidLogin_Redirect_Test()
        {
            
            _testTools.GetSeedData(_context);

            var loginViewModel = new LoginViewModel { LoginID = "12345678", PasswordHash = "abc123" };

            // Act
            var result = await _controller.Login(loginViewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Customer", result.ControllerName);
            
        }

        // this test will attempt to login with incorrect information and will not be redirected
        // the modelstate should also be invalid
        [Fact]
        public async Task Login_InvalidLogin_Test()
        {


            _testTools.GetSeedData(_context);

            var login = _testTools.CreateLogin();
            var loginViewModel = new LoginViewModel
            {
                LoginID = login.LoginID,
                PasswordHash = "wrongpassword" // Incorrect password
            };

            // Act
            var result = await _controller.Login(loginViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        // this test is sumulating logging  and is ensuring that the user has been redirected to the "Home" index
        [Fact]
      public async Task Logout_Test()
        {

            // Arrange

            var customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
            _controller.HttpContext.Session.SetInt32(nameof(Customer.CustomerID), customer.CustomerID);
           
            // Act
            var result = _controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);

            // Verify that the session has been cleared
            Assert.Empty(_controller.HttpContext.Session.Keys);
        }


        // getLoginController used to egt the login Controller and the session
        public LoginController GetController(MCBAContext context)
        {
            var controller = new LoginController(context)
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


