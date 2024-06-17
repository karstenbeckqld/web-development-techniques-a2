using Microsoft.AspNetCore.Mvc;

namespace WebAPIPortal.Controllers;

public class PortalLoginController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    private HttpClient Client => _clientFactory.CreateClient("api");

    public PortalLoginController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;
    
   [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Index(string username, string password)
    {

        if (username == "admin" && password == "admin")
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }
}