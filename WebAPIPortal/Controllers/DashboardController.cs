using System.Text;
using MCBA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebAPIPortal.Controllers;

public class DashboardController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    private HttpClient Client => _clientFactory.CreateClient("api");

    public DashboardController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await Client.GetAsync("api/customer");

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest();
        }

        var customers = await Client.GetAsync("api/customer").Result.Content.ReadAsStringAsync();
        
        var customerList = JsonConvert.DeserializeObject<List<Customer>>(customers);

        return View(customerList);
    }

    [HttpGet("[controller]/edit/{customerId}")]
    public async Task<IActionResult> Edit([FromRoute] int customerId)
    {
        var json = await Client.GetAsync("api/Customer/" + customerId).Result.Content
            .ReadAsStringAsync();
        var loginJson = await Client.GetAsync("api/Login/" + customerId).Result.Content
            .ReadAsStringAsync();

        var customer = JsonConvert.DeserializeObject<Customer>(json);
        customer.login = JsonConvert.DeserializeObject<Login>(loginJson);

        return View(customer);
    }

    [HttpPost("[controller]/edit/{customerId}")]
    public async Task<IActionResult> Edit([FromRoute] int customerId, Customer customer)
    {
        var loginJson = await Client.GetAsync("api/Login/" + customerId).Result.Content
            .ReadAsStringAsync();
        var content = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
        var response = Client.PutAsync("api/Customer", content).Result;
        customer.login = JsonConvert.DeserializeObject<Login>(loginJson);
        
        ViewData["outcome"] = response.StatusCode;

        return View(customer);
    }

    [HttpGet("/Home/edit/{customerId}/Image")]
    public async Task<IActionResult> DisplayImage(int? customerId)
    {
        var json = await Client.GetAsync("api/Customer/" + customerId).Result.Content.ReadAsStringAsync();

        Customer customer = JsonConvert.DeserializeObject<Customer>(json);

        return File(customer.ProfilePicture, "image/png");
    }

    [HttpPost("/Home/edit/{customerId}/lock")]
    public async Task<IActionResult> ToggleAccountLock([FromRoute] int customerId)
    {
        var json = await Client.GetAsync("api/Login/" + customerId).Result.Content
            .ReadAsStringAsync();
        Login login = JsonConvert.DeserializeObject<Login>(json);

        login.LockedAccount = !login.LockedAccount;

        var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
        var response = Client.PutAsync("api/Login", content).Result;

        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("outcome", response.StatusCode);
        result.Add("lockedStatus", login.LockedAccount);

        return Json(result);
    }
}