using Microsoft.AspNetCore.Mvc;
using BWC.DataConnection;
using BWC.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using BCrypt.Net;

namespace BWC.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(SqlServerDbContext context, ILogger<RegisterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Hash the password before saving
                    model.PasswordHash = HashPassword(model.PasswordHash);

                    _context.Users.Add(model);
                    await _context.SaveChangesAsync();

                    // Registration successful, redirect to a secure page
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during registration.");
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private string HashPassword(string password)
        {
            // Use BCrypt for password hashing
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
