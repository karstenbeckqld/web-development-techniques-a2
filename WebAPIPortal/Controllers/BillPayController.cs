using System.Text;
using MCBA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebAPIPortal.Controllers;

public class BillPayController : Controller
{
    private readonly IHttpClientFactory _clientFactory;

    private HttpClient Client => _clientFactory.CreateClient("api");

    public BillPayController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;



    [HttpGet("BillPay/Index/{customerId}")]
    public async Task<IActionResult> Index([FromRoute]int customerId)
    {

        var accountResponse = Client.GetAsync("http://localhost:5100/api/Accounts/customer/" + customerId).Result;
        
        var accountContent = await accountResponse.Content.ReadAsStringAsync();
            
        List<Account> accounts = JsonConvert.DeserializeObject<List<Account>>(accountContent);
            
        foreach (var account in accounts)
        {
            var billsResponseMessage = Client.GetAsync("http://localhost:5100/api/billpay/account/" + account.AccountNumber)
                .Result;
            var billsContent = await billsResponseMessage.Content.ReadAsStringAsync();
                
            account.Bills = JsonConvert.DeserializeObject<List<BillPay>>(billsContent);
        }

        return View(accounts);
    }
    
    
    [HttpPost("/BillPay/{billId}/lock")]
    public async Task<IActionResult> ToggleAccountLock([FromRoute] int billId)
    {
        
        var json = await Client.GetAsync("http://localhost:5100/api/billpay/"+billId).Result.Content.ReadAsStringAsync();
        BillPay bill = JsonConvert.DeserializeObject<BillPay>(json);

        bill.LockedPayment = !bill.LockedPayment;

        var content = new StringContent(JsonConvert.SerializeObject(bill), Encoding.UTF8, "application/json");
        var response = Client.PutAsync("http://localhost:5100/api/billpay", content).Result;

        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("outcome",response.StatusCode);
        result.Add("lockedStatus",bill.LockedPayment);

        return Json(result);
    }
    

}