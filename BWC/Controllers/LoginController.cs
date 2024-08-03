using Microsoft.AspNetCore.Mvc;

namespace BWC.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
