using System;
using Xunit;
using Autofac;
using MCBA.Interfaces;
using MCBA.Models;
using MCBA.Data;

namespace MCBA.Tests.ModelTests
{
    public class PayeeTests : BaseTest
    {
        private readonly IPayeeFactory _payeeFactory;
        private readonly TestTools _testTools;
        private readonly MCBAContext _context;
        public PayeeTests()
        {
            // Resolve the IPayeeFactory using Autofac
            _payeeFactory = Resolve<IPayeeFactory>();
            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
        }

        // this test will create a payee with valid informations
        [Theory]
        [InlineData(1, "Electric bill", "123  St", "City", "VIC", "3000", "(04)01 234 567")]
        [InlineData(2, "Water bill", "123 st", "Town", "NSW", "2000", "(04)12 345 678")]
        public void CreatePayee_ValidParameters(int payeeID, string name, string address, string city, string state, string postCode, string phone)
        {
            // Act
            var payee = _payeeFactory.CreatePayee(payeeID, name, address, city, state, postCode, phone);

            // Assert
            Assert.NotNull(payee);
            Assert.Equal(payeeID, payee.PayeeID);
            Assert.Equal(name, payee.Name);
            Assert.Equal(address, payee.Address);
            Assert.Equal(city, payee.City);
            Assert.Equal(state, payee.State);
            Assert.Equal(postCode, payee.PostCode);
            Assert.Equal(phone, payee.Phone);
        }

        // test will attempt to create a payee with various invalid data
        [Theory]
        [InlineData(1, "", "123 Main St", "City", "VIC", "3000", "(04)01 234 567")] // invalid name
        [InlineData(2, "John Smith", "", "City", "NSW", "00", "(04)12 345 678")] // invalid postcode
        [InlineData(2, "John Smith", "Address", "City", "NSW", "2000", "04 12 345 678")] // invalid phone
        public void CreatePayee_InvalidParameters(int payeeID, string name, string address, string city, string state, string postCode, string phone)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _payeeFactory.CreatePayee(payeeID, name, address, city, state, postCode, phone));
        }

        // test will check if a payee exists in the database
        [Fact]
        public void checkIfPayeeInDB()
        {
            // Arrange
            using var context = _testTools.GetSeedData(_testTools.getContext());

            //Act
            var retrievedPayee = context.Payee.FirstOrDefault(p => p.PayeeID == 1);

            // Assert
            Assert.NotNull(retrievedPayee);
            Assert.Equal("PowerBill", retrievedPayee.Name);
            Assert.Equal("1 Power Street", retrievedPayee.Address);

        }

        // this test will create a payee, add it to the database and then check that it has been added properly
        [Fact]
        public void add_checkifPayeeInDB()
        {
            // Arrange
            var newPayee = new Payee
            {
                Name = "New Payee",
                Address = "New Address",
                City = "New City",
                State = "NSW",
                PostCode = "1234",
                Phone = "(02) 1234 5678"
            };
            // ACt
            _context.Payee.Add(newPayee);
            _context.SaveChanges();

            var retrievedPayee = _context.Payee.FirstOrDefault(p => p.PayeeID == newPayee.PayeeID);

            // Assert
            Assert.NotNull(retrievedPayee);
            Assert.Equal(newPayee.Name, retrievedPayee.Name);
            Assert.Equal(newPayee.Address, retrievedPayee.Address);

        }


    }
}