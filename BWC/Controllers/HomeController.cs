using BWC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using BWC.DataConnection;
using Microsoft.AspNetCore.Authorization;

namespace BWC.Controllers
{

    [Authorize(Policy = "AdminPolicy")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlServerDbContext _context;

        public HomeController(ILogger<HomeController> logger, SqlServerDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var totalAppointments = _context.Appointments.Count();
                var followUps = _context.Appointments.Count(a => a.AppointmentType == 1); // Follow-Up
                var completed = _context.Appointments.Count(a => a.Status == 4); // Complete

                var model = new AppointmentStatisticsViewModel
                {
                    TotalAppointments = totalAppointments,
                    FollowUps = followUps,
                    Completed = completed
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving appointment statistics.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
