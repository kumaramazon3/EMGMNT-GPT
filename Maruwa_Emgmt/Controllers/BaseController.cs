using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Maruwa_Emgmt.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // This method runs before every action in controllers inheriting from BaseController
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the session value exists
            var userName = HttpContext.Session.GetString("LoginUserName");
            if (string.IsNullOrEmpty(userName))
            {
                // If session is missing, redirect to login page
                context.Result = new RedirectToActionResult("Login", "LoginUser", null);
            }
            base.OnActionExecuting(context);
        }
    }
}
