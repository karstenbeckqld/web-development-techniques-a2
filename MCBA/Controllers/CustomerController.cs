using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCBA.Data;
using MCBA.Models;
using MCBA.Utils;
using JetBrains.Annotations;
using SimpleHashing.Net;
using ImageMagick;
using System.Diagnostics;

namespace MCBA.Controllers;

// The CustomerController is responsible for all customer related operations, like changing the password and changing a
// customer's details, including the profile image.

[AuthoriseCustomer]
public class CustomerController : Controller
{
    private readonly MCBAContext _context;

    private CustomerViewModel _customerViewModel;

    private static readonly ISimpleHash _sSimpleHash = new SimpleHash();

    private const string _defaultImagePath = "/images/test.jpg";

    private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

    [AuthoriseCustomer]
    public CustomerController(MCBAContext context)
    {
        _context = context;
    }

    // The Index() method is the landing page method for a customer after logging in and displays a customer's accounts. 
    public async Task<IActionResult> Index()
    {
        var customer = await _context.Customer.Include(x => x.Accounts)
            .FirstOrDefaultAsync(x => x.CustomerID == CustomerID);

        return View(customer);
    }

    [HttpGet]
    public async Task<IActionResult> Details()
    {
        var customer = await _context.Customer.Include(x => x.login)
            .FirstOrDefaultAsync(x => x.CustomerID == CustomerID);

        _customerViewModel = SetCustomerViewModelProperties(customer, _customerViewModel);


        return View(_customerViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Details([CanBeNull] string PasswordHash, [CanBeNull] string NewPassword,
        [CanBeNull] string NewPasswordConfirm)
    {
        var customerID = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

        var loginData = _context.Login.FirstOrDefaultAsync(x => x.CustomerID == customerID).Result;

        if (loginData is null) return RedirectToAction("Index", "Customer");

        var customer = await _context.Customer.FirstOrDefaultAsync(x => x.CustomerID == customerID);

        if (customer is null) return RedirectToAction("Index", "Customer");

        _customerViewModel = SetCustomerViewModelProperties(customer, _customerViewModel);

        if (PasswordHash is null ||
            string.IsNullOrEmpty(PasswordHash) ||
            !_sSimpleHash.Verify(PasswordHash, loginData.PasswordHash))
        {
            ModelState.AddModelError("CurrentPasswordError", "Current password is incorrect");
            return View(_customerViewModel);
        }

        if (NewPassword is null ||
            string.IsNullOrEmpty(NewPassword) ||
            NewPasswordConfirm is null ||
            string.IsNullOrEmpty(NewPasswordConfirm) ||
            !NewPassword.Equals(NewPasswordConfirm))
        {
            ModelState.AddModelError("PasswordMatchError", "New password doesn't match.");
            return View(_customerViewModel);
        }

        if (ModelState.IsValid)
        {
            ISimpleHash hash = new SimpleHash();
            var changedPassword = hash.Compute(NewPassword);
            loginData.ChangePassword(changedPassword);
            await _context.SaveChangesAsync();

        }

        return RedirectToAction("Details", "Customer");
    }

    private CustomerViewModel SetCustomerViewModelProperties(Customer customer, CustomerViewModel customerViewModel)
    {
        customerViewModel = new CustomerViewModel
        {
            CustomerID = CustomerID,
            Name = customer.Name,
            TFN = customer.TFN,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostCode = customer.PostCode,
            Mobile = customer.Mobile
        };

        return customerViewModel;
    }

    // POST: Customers/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Customer == null)
        {
            return NotFound();
        }

        var customer = await _context.Customer.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

   

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Customer customer,
        IFormFile? ProfilePicture,
        string value)
    {

        if (id != customer.CustomerID)
        {
            return NotFound();
        }

        if (ProfilePicture is not null && ProfilePicture.Length > 0)
        {
            if (ProfilePicture.ContentType.Contains("image"))
            { 
                customer.ProfilePicture = await pic(ProfilePicture);
            }
        }

        if (value == "delete")
        {
            // Image conversion method adapted from:
            // https://stackoverflow.com/questions/64600837/how-to-convert-a-file-path-to-iformfile-in-asp-net-core

      
            string currentDirectory = Directory.GetCurrentDirectory();

            // setting path to default image
            // setting path to default image
            string imageFilePath = Path.Combine(currentDirectory, "wwwroot", "images", "placeholder.jpg");

            // this if statement is doing a check which will change the directory - this is because the testing environment does not run in
            // the same location and paths for images need to be altered
            if (currentDirectory.Contains("Debug"))
            {
                string projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), @"../../../.."); 
                imageFilePath = Path.Combine(projectRootPath, "MCBA", "wwwroot", "images", "placeholder.jpg");
            }

      
            customer.ProfilePicture = await System.IO.File
                .ReadAllBytesAsync(imageFilePath);
       
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit));
            }
            catch (DbUpdateConcurrencyException)
            {
          
                if (!CustomerExists(CustomerID))
                {
                    return NotFound();
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Edit));
            }
            catch (DbUpdateConcurrencyException)
            {
            
                if (!CustomerExists(CustomerID))
                {
                    return NotFound();
                }
            }
        }

        return View(customer);
    }

    public async Task<IActionResult> DisplayImage()
    {
        // Method adapted from:
        // https://stackoverflow.com/questions/20165376/asp-net-mvc-image-from-byte-array

        var customer = await _context.Customer.FindAsync(CustomerID);

        var imageData = customer.ProfilePicture;
        
        return File(imageData, "image/png");
    }

    private bool CustomerExists(int id)
    {
        return (_context.Customer?.Any(e => e.CustomerID == id)).GetValueOrDefault();
    }

    // The pic method is used when uploading an image to resize it accordingly
    // It uses MagickImage by default and will maintain the aspect ratio to the closest possible values according to the
    // specified dimensions (400, 400)
    public async Task<byte[]> pic(IFormFile ProfilePicture)
    {
        // FullPath is the new file's path.

        using var stream = new MemoryStream();
        ProfilePicture.CopyTo(stream);
        var tmpImg = stream.ToArray();

        using var collection = new MagickImageCollection(tmpImg);
        using var image = new MagickImage(tmpImg);

        image.Resize(400, 400);
        return image.ToByteArray();
    }

    [HttpPost]
    public async Task<IActionResult> Update(IFormFile ProfilePicture)
    {
        Debug.WriteLine("Here");

        var customer = _context.Customer.Find(CustomerID);  

        if (ProfilePicture is not null && ProfilePicture.Length > 0)
        {
            if (ProfilePicture.ContentType.Contains("image"))
            {
                customer.ProfilePicture = await pic(ProfilePicture);
                _context.SaveChanges();
            }
        }

        return View("Edit",customer);
    }
   

}