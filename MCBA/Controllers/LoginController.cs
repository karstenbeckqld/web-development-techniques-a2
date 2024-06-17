using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;

namespace MCBA.Controllers;

public class LoginController : Controller
{
    private static readonly ISimpleHash _sSimpleHash = new SimpleHash();

    private readonly MCBAContext _context;

    public LoginController(MCBAContext context) => _context = context;

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {

        if (ModelState.IsValid)
        {
            var login = await _context.Login.FindAsync(loginViewModel.LoginID);

            if (login is null || string.IsNullOrEmpty(loginViewModel.PasswordHash) ||
                !_sSimpleHash.Verify(loginViewModel.PasswordHash, login.PasswordHash))
            {
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View();
            }

            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.Name), login.Customer.Name);
        }
        return RedirectToAction("Index", "Customer");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        // Logout the customer
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
}