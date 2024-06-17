using MCBA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MCBA.Utils;

public class AuthoriseCustomerAttribute: Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var customerId = context.HttpContext.Session.GetInt32(nameof(Customer.CustomerID));
        if (!customerId.HasValue)
        {
            context.Result = new RedirectToActionResult("Login", "Login", null);
        }
    }
}