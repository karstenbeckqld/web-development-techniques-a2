using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.DataManager;
using MCBA.Models;

namespace WebApi.Controllers;

// See here for more information:
// https://learn.microsoft.com/en-au/aspnet/core/web-api/?view=aspnetcore-7.0

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly LoginManager _repo;

    public LoginController(LoginManager repo)
    {
        _repo = repo;
    }

    // GET: api/customer
    [HttpGet]
    public IEnumerable<Login> Get()
    {
        return _repo.GetAll();
    }

    // GET api/customer/1
    [HttpGet("{id}")]
    public Login Get(int id)
    {
        return _repo.Get(id);
    }

    // POST api/customer
    [HttpPost]
    public void Post([FromBody] Login login)
    {
        _repo.Add(login);
    }
    
    // PUT api/customer
    [HttpPut]
    public void Put([FromBody] Login login)
    {
        _repo.Update(login.CustomerID, login);
    }

    // DELETE api/customer/1
    [HttpDelete("{id}")]
    public long Delete(int id)
    {
        return _repo.Delete(id);
    }
}
