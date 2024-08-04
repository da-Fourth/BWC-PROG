using Microsoft.AspNetCore.Mvc;
using BWC.DataConnection;
using BWC.Models;

namespace BWC.Controllers
{
    public class RegisterController : Controller
    {
        public readonly SqlServerDbContext _context;

        public RegisterController(SqlServerDbContext context)
        {
            _context = context;
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
                // Hash the password before saving (use a proper hashing method in production)
                model.PasswordHash = HashPassword(model.PasswordHash);

                _context.Users.Add(model);
                await _context.SaveChangesAsync();

                // Registration successful, redirect to a secure page
                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private string HashPassword(string password)
        {
            // Implement a proper password hashing method here
            return password; // Placeholder, replace with actual hashing
        }


    }
}
