using System;
using Xunit;
using Autofac;
using MCBA.Interfaces;
using MCBA.Models;
using SimpleHashing.Net;
using Microsoft.EntityFrameworkCore;
using MCBA.Data;

namespace MCBA.Tests.ModelTests
{
    public class LoginTests : BaseTest
    {
        private readonly ILoginFactory _loginFactory;
        private TestTools _testTools;
        private readonly MCBAContext _context;

        public LoginTests()
        {
            // Resolve the ILoginFactory using Autofac
            _loginFactory = Resolve<ILoginFactory>();
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
        }

        // test used to create a Login with valid paramaters
        [Theory]
        [InlineData("12345678", 1, "hash123", true)]
        [InlineData("12225678", 2, "password456", false)]
        public void CreateLogin_ValidParameters(string loginID, int customerID, string passwordHash, bool lockedAccount)
        {

            ISimpleHash simpleHash = new SimpleHash();
            string hashedPassword = simpleHash.Compute(passwordHash);

            var login = _loginFactory.CreateLogin(loginID, customerID, hashedPassword, lockedAccount);

            // Assert
            Assert.NotNull(login);
            Assert.Equal(loginID, login.LoginID);
            Assert.Equal(customerID, login.CustomerID);
            Assert.Equal(hashedPassword, login.PasswordHash);
            Assert.Equal(lockedAccount, login.LockedAccount);
        }

        // test usedto attempt to create a login with invalid parameters
        [Theory]
        [InlineData("", 1, "hash123", true)] // invalid loginID
        [InlineData("U1234567", 0, "password456", false)] // invalid customerID
        [InlineData(null, 1, "password456", false)]// invalid loginID
        public void CreateLogin_InvalidParameters(string loginID, int customerID, string passwordHash, bool lockedAccount)
        {
            if (customerID == 1)
            {
                ISimpleHash simpleHash = new SimpleHash();
                string hashedPassword = simpleHash.Compute(passwordHash);
            }

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _loginFactory.CreateLogin(loginID, customerID, passwordHash, lockedAccount));
        }

        // test used to check if a login (by loginID) is in the database
        [Fact]
        public void check_If_LoginInDB()
        {
            // Arrange
            using var context = _testTools.GetSeedData(_testTools.getContext());


            var retrievedLogin = context.Login.FirstOrDefault(p => p.LoginID == "12345678");

            // Assert
            Assert.NotNull(retrievedLogin);
            Assert.Equal(2100, retrievedLogin.CustomerID); 
            Assert.Equal("12345678", retrievedLogin.LoginID); 

        }

        // test used to create a login, add it  to the database and then check that it has been properly added
        [Fact]
        public void Add_Login_Checkif_inDb()
        {
            // Arrange

            var newLogin = _testTools.CreateLogin();
            //Act
            _context.Login.Add(newLogin);
            _context.SaveChanges();

            var retrievedTransaction = _context.Login.FirstOrDefault(p => p.LoginID == "12345677");

            // Assert
            Assert.NotNull(newLogin); 
            Assert.Equal("12345677", newLogin.LoginID); 
            Assert.Equal(5000, newLogin.CustomerID); 
        }



    }
}