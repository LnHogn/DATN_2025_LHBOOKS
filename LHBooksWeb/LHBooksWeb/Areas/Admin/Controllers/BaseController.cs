using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LHBooksWeb.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["ActiveController"] = ControllerContext.RouteData.Values["controller"]?.ToString();
            base.OnActionExecuting(context);
        }
    }
}
