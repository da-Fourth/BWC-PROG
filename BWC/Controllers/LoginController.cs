using Microsoft.AspNetCore.Mvc;
using BWC.DataConnection;
using Microsoft.EntityFrameworkCore;
using BWC.Models;
using System.Threading.Tasks;
using BWC.Services;

namespace BWC.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly IUserService _userService;

        public LoginController(SqlServerDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var user = await _context.Users
                            .FirstOrDefaultAsync(u => (u.Email == model.Username || u.Username == model.Username) && u.PasswordHash == model.Password);

                        if (user != null)
                        {
                            // Save user information in the user service
                            _userService.UserId = user.Id.ToString();
                            _userService.Username = user.Username;

                            // Login successful, redirect to a secure page
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Login failed, show error message
                            ModelState.AddModelError(string.Empty, "Invalid Credentials.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception (optional)
                        // _logger.LogError(ex, "An error occurred during login.");

                        // Show a generic error message
                        ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
            }

            // If we got this far, something failed, redisplay form
            return View("Index", model);
        }
    }
}
