using MCBA.Controllers;
using MCBA.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MCBA.Models;
using SimpleHashing.Net;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Castle.Core.Resource;
using System.Reflection;
using System.Reflection.PortableExecutable;

namespace MCBA.Tests.ControllerTests
{
    public class CustomerControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly TestTools _testTools;
        private static readonly ISimpleHash _sSimpleHash = new SimpleHash();
        private readonly MCBAContext _context;
        private readonly CustomerController _controller;
        private readonly Customer _customer;

        public CustomerControllerTests(ITestOutputHelper testOutputHelper) {

            _testTools = new TestTools();
            _context = _testTools.GetSeedData(_testTools.getContext());
            _controller = GetController(_context);
            _customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
            _controller.HttpContext.Session.SetInt32(nameof(Customer.CustomerID), _customer.CustomerID);
            
        }

        

        //test used to attempt to change the password with invalid information (wrong current password)
        [Fact]
        public async Task Details_changePassword_failure()
        {
            // Arrange
            

            var formData = new Dictionary<string, string>
            {
                { "PasswordHash", "wrongPassword" },
                { "NewPassword", "newPassword" },
                { "NewPasswordConfirm", "newPassword" }
            };

            // Act
            var result = await _controller.Details(formData["PasswordHash"], formData["NewPassword"], formData["NewPasswordConfirm"]);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal("Current password is incorrect", _controller.ModelState["CurrentPasswordError"].Errors[0].ErrorMessage);
        }

        // this test is used to change the password
        [Fact]
        public async Task Details_ChangePassword_Success()
        {

            // Get the existing customer's login data
            var loginData = await _context.Login.FirstOrDefaultAsync(x => x.CustomerID == _customer.CustomerID);

            var oldPasswordHash = loginData.PasswordHash;

            var formData = new Dictionary<string, string>
            {
                { "PasswordHash", "abc123" }, 
                { "NewPassword", "newPassword" },
                { "NewPasswordConfirm", "newPassword" }
            };

            // Act
            var result = await _controller.Details(formData["PasswordHash"], formData["NewPassword"], formData["NewPasswordConfirm"]);

            // Assert
            
            Assert.True(_controller.ModelState.IsValid);

            // Check if the password has been changed
            loginData = await _context.Login.FirstOrDefaultAsync(x => x.CustomerID == _customer.CustomerID);

            var newPasswordHash = loginData.PasswordHash;
            Assert.NotEqual(oldPasswordHash, newPasswordHash);
        }

        // used to update some parameters of a customer, and then call the edit method of
        // the customer controller to update - checks against newly laoded value
        // this test used its own seperate controller and context this is because of a conflict that would occur
        [Fact]
        public async Task Customer_Edit_Test_Valid()
        {

            var tools = _testTools.GetSeedData(_testTools.getContext());
            var controller = GetController(tools);


            var customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            customer.Name = "Max";

            await controller.Edit(2100, customer, null, null);

            var editedCustomer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            // checking that the 
            Assert.Equal(editedCustomer.Name, customer.Name);
            Assert.Equal("Max", editedCustomer.Name);
            Assert.Equal("Max", customer.Name);


        }

        // tesst usedto edit and update the customers
        // this test used its own seperate controller and context this is because of a conflict that would occur

        [Fact]
        public async Task Customer_Edit_Test_Invalid()
        {
           
            var context = _testTools.GetSeedData(_testTools.getContext());
            var controller = GetController(context);

            var editedCustomer = new Customer
            {
                CustomerID = 2100,
                Name = "Max",
                Address = "test street",
                TFN = "12312323131123", // invalid TFN
                City = _customer.City,
                State = "12345", // invalid State
                PostCode = _customer.PostCode,
                Mobile = _customer.Mobile,
                ProfilePicture = _customer.ProfilePicture,
                login = _customer.login,
                Accounts = _customer.Accounts
                
            };

            await controller.Edit(2100, editedCustomer, null, null);

            // pulling the customer from the database using the same CustomerID -> to ensure that the changes
            // are not saved
            var customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            // checking that the 
            Assert.NotEqual(editedCustomer.Name, customer.Name);
            Assert.NotEqual(editedCustomer.Address, customer.Name);
            Assert.NotEqual(editedCustomer.TFN, customer.Name);

        }

        // this test used its own seperate controller and context this is because of a conflict that would occur
        [Fact]
        public async Task Customer_EditProfilePic_Test_Valid()
        {
            //Arrange
            var tools = _testTools.GetSeedData(_testTools.getContext());
            var controller = GetController(tools);
            var customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);
            var originalPic = customer.ProfilePicture;

           //Act
            await controller.Edit(2100, customer, getProfilePic(1), null);

            var editedCustomer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            // checking that the 
            Assert.Equal(editedCustomer.ProfilePicture, customer.ProfilePicture);
            Assert.NotEqual(editedCustomer.ProfilePicture, originalPic);

        }


        // this test will attempt to update the users profile picture and should fail
        [Fact]
        public async Task Customer_EditProfilePic_Test_ShouldFail()
        {
            //Arrange
            var context = _testTools.GetSeedData(_testTools.getContext());
            var controller = GetController(context);
            var customer =  _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            
            var originalProfilePic = customer.ProfilePicture; // Store the original ProfilePicture

            //Act
            await controller.Edit(2100, customer, getProfilePic(2), null);

            var editedCustomer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            // Assert that the ProfilePicture has not changed (This should fail)
            Assert.Equal(originalProfilePic, editedCustomer.ProfilePicture);
        }

        [Fact]
        public async Task Customer_Edit_Delete_ProfilePic()
        {
            // Arrange

            var context = _testTools.GetSeedData(_testTools.getContext());
            var controller = GetController(context);

            var customer = _context.Customer.FirstOrDefault(p => p.CustomerID == 2100);

            var originalPic = customer.ProfilePicture;


            await controller.Edit(2100, customer, getProfilePic(1), null);

            // Act
            var result = await controller.Edit(customer.CustomerID, customer, null, "delete");

            // Assert
            var updatedCustomer = await context.Customer.FindAsync(customer.CustomerID);
  
            Assert.NotEqual(originalPic, updatedCustomer.ProfilePicture);
        }


        public IFormFile getProfilePic(int type)
        {
            if(type == 1)
            {
                string projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), @"../../../..");
               string imageFilePath = Path.Combine(projectRootPath, "MCBA", "wwwroot", "images", "1200x1600.png");


                byte[] profilePicContent;
                using (var stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        profilePicContent = memoryStream.ToArray();
                    }
                }

            https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.formfile?view=aspnetcore-7.0
                  // creating a an IFormFile to be used as a parameter inthe edit method
                var profilePic = new FormFile(new MemoryStream(profilePicContent), 0, profilePicContent.Length,
                                              "profilePic", imageFilePath)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                } as IFormFile;
                return profilePic;
            }
            else
            {

                byte[] profilePicContent = new byte[] { 1, 2, 3, 4 };

                string projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), @"../../../..");
                string imageFilePath = Path.Combine(projectRootPath, "MCBA", "wwwroot", "images", "NotApicture.txt");
               
            //Act

            // reference for the following 
            https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.formfile?view=aspnetcore-7.0
                  // creating a new FormFile as an IFormFile - the file type that the edit method may be expecting
                var profilePic = new FormFile(new MemoryStream(profilePicContent), 0, profilePicContent.Length,
                                              "profilePic", imageFilePath)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "Text/txt"
                } as IFormFile;
                return profilePic;
            }

        }


        public CustomerController GetController(MCBAContext context)
        {
            var controller = new CustomerController(context)
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
