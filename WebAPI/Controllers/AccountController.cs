using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.DataManager;
using MCBA.Models;

namespace WebApi.Controllers;

// See here for more information:
// https://learn.microsoft.com/en-au/aspnet/core/web-api/?view=aspnetcore-7.0

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountManager _repo;

    public AccountController(AccountManager repo)
    {
        _repo = repo;
    }

    // GET: api/customer
    [HttpGet]
    public IEnumerable<Account> Get()
    {
        return _repo.GetAll();
    }

    // GET api/customer/1
    [HttpGet("{id}")]
    public Account Get(int id)
    {
        return _repo.Get(id);
    }

    // POST api/customer
    [HttpPost]
    public void Post([FromBody] Account account)
    {
        _repo.Add(account);
    }
    
    // PUT api/customer
    [HttpPut]
    public void Put([FromBody] Account account)
    {
        _repo.Update(account.AccountNumber, account);
    }

    // DELETE api/customer/1
    [HttpDelete("{id}")]
    public long Delete(int id)
    {
        return _repo.Delete(id);
    }


    [HttpGet("/api/accounts/customer/{id}")]
    public IEnumerable<Account> GetAllAccounts([FromRoute]int id)
    {
        return _repo.GetAllAccounts(id);
    
    }
}
