using Microsoft.AspNetCore.Mvc;

namespace BWC.Controllers
{
    public class CounselorAppointment : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
