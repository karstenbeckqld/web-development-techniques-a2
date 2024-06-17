using System;
using System.Collections.Generic;
using Xunit;
using Autofac;
using MCBA.Interfaces;
using MCBA.Models;
using Microsoft.Identity.Client;
using MCBA.Tests;
using Microsoft.EntityFrameworkCore;
using MCBA.Data;

namespace MCBA.Tests.ModelTests
{
    public class CustomerTests : BaseTest
    {
        private readonly ICustomerFactory _customerFactory;
        private readonly MCBAContext _context;
        private TestTools _testTools;

        public CustomerTests()
        {

            _customerFactory = Resolve<ICustomerFactory>();
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());

        }

        // test used to create a customer with valid paramaters
        [Theory]
        [InlineData(1, "John Doe", "123 456 789", "123 Main St", "Cityville", "VIC", "3000", "0412 123 456", new byte[] { 1, 2, 3 })]
        [InlineData(2, "Jane Smith", "987 654 321", "456 Elm St", "Townsville", "NSW", "2000", "0412 456 789", new byte[] { 1, 2, 3 })]
        public void CreateCustomer_ValidParameters(int customerId, string name, string tfn, string address, string city, string state, string postCode, string mobile, byte[]? profilePicture)
        {
            // Act
            List<IAccounts> accounts = _testTools.CreateMultipleAccounts();
            
            var customer = _customerFactory.CreateCustomer(customerId, name, tfn, address, city, state, postCode, mobile, profilePicture, accounts, _testTools.CreateLogin());

            // Assert
            Assert.NotNull(customer);
            Assert.Equal(customerId, customer.CustomerID);
            Assert.Equal(name, customer.Name);
            Assert.Equal(tfn, customer.TFN);
            Assert.Equal(address, customer.Address);
            Assert.Equal(city, customer.City);
            Assert.Equal(state, customer.State);
            Assert.Equal(postCode, customer.PostCode);
            Assert.Equal(mobile, customer.Mobile);
            Assert.Equal(profilePicture, customer.ProfilePicture);
        }

        // test used to create a customer with various incorrect parameters
        [Theory]
        [InlineData(1, "123556789101231213212312112321321321321231321321213213212131", "987 654 321", "", "Townsville", "NSW", "2000", "0412 123 456", new byte[] { 1, 2, 3 }, null)] /// name too long
        [InlineData(-1, "John Doe", "987 654 321", "address", "Townsville", "NSW", "2000", "0412 123 456", new byte[] { 1, 2, 3 }, null)]// negative customerId
        [InlineData(2, "John Doe", "987654321", "address", "Townsville", "NSW", "2000", "0412 123 465", new byte[] { 1, 2, 3 }, null)]// wrong TFN regex
        [InlineData(2, "John Doe", "987 654 321", "address", "Townsville", "NSWa", "2000", "0412 123 456", new byte[] { 1, 2, 3 }, null)]// wrong state
        [InlineData(2, "John Doe", "987 654 321", "address", "Townsville", "NSW", "202200", "0412 123 456", new byte[] { 1, 2, 3 }, null)]// wrong postcode
        [InlineData(2, "John Doe", "987 654 321", "address", "Townsville", "NSW", "2000", "04xx xxx xxx", new byte[] { 1, 2, 3 }, null)]// wrong mobile
        [InlineData(2, "John Doe", "987 654 321", "123556789101231213212312112321321321321231321321213213212131", "Townsville", "NSW", "2000", "0412 123 456", new byte[] { 1, 2, 3 }, null)]// address too long
        [InlineData(2, "John Doe", "987 654 321", "address", "123556789101231213212312112321321321321231321321213213212131", "NSW", "2000", "0412 123 456", new byte[] { 1, 2, 3 }, null)]// city too long
        public void CreateCustomer_InvalidParameters(
            int customerId, string name, string tfn, string address, string city, string state, string postCode, string mobile, byte[]? profilePicture, List<IAccounts> accounts)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _customerFactory.CreateCustomer(
                customerId, name, tfn, address, city, state, postCode, mobile, profilePicture, accounts, _testTools.CreateLogin()));
        }


        // test used to check if a customer is in the databse
        [Fact]
        public void check_If_CustomerInDB()
        {
            // Arrange

            var retrievedCustomer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            // Assert
            Assert.NotNull(retrievedCustomer); 
            Assert.Equal(2100, retrievedCustomer.CustomerID); 
            Assert.Equal("Matthew Bolger", retrievedCustomer.Name); 

        }

        // test used to create a customer, add it to the databsse and then check that it has been properly added
        [Fact]
        public void Add_Customer_Checkif_inDb()
        {
            // Arrange

    
            var newCustomer = new Customer

            {
                CustomerID = 5000,
                Name = "Test",
                Address = "Test addr",
                PostCode = "1234",
                City = "city",
                Accounts = new List<Account> { new Account { } },
                login = _testTools.CreateLogin()
  
            };
            // Act
            _context.Customer.Add(newCustomer);
            _context.SaveChanges();

            var retrievedCustomer = _context.Customer.FirstOrDefault(p => p.CustomerID == 5000);

            // Assert
            Assert.NotNull(retrievedCustomer); 
            Assert.Equal("Test", retrievedCustomer.Name); 
            Assert.Equal(5000, retrievedCustomer.CustomerID); 
        }

    }
}