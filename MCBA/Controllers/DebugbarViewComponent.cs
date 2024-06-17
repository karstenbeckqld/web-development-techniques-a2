using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

public class DebugbarViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }

}