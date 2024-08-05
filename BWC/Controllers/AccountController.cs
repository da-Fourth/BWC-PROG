using BWC.DataConnection;
using BWC.Models;
using Microsoft.AspNetCore.Mvc;

namespace BWC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SqlServerDbContext _context;

        public AccountController(SqlServerDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            try
            {
                return View("Register/Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            try
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
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Internal server error");
            }
        }

        private string HashPassword(string password)
        {
            try
            {
                // Implement a proper password hashing method here
                return password; // Placeholder, replace with actual hashing
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("Error hashing password", ex);
            }
        }
    }
}
