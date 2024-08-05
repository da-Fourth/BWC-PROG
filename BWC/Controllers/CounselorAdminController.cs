using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BWC.DataConnection; // Update with the actual namespace for your DbContext
using BWC.Models;
using Microsoft.Extensions.Logging; // Add using directive for logging

namespace BWC.Controllers
{
    public class CounselorAdminController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly ILogger<CounselorAdminController> _logger; // Add logger

        public CounselorAdminController(SqlServerDbContext context, ILogger<CounselorAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /CounselorAdmin
        public async Task<IActionResult> Index()
        {
            try
            {
                var counselors = await _context.Users
                    .Where(u => u.Role == 1) // Fetch users with role = 1 (Counselor)
                    .ToListAsync();

                return View(counselors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching counselors.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: /CounselorAdmin/Create
        public IActionResult Create()
        {
            var model = new User();
            return View(model);
        }

        // POST: /CounselorAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,Email,Username,PasswordHash,Program")] User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.Role = 1; // Set role to 1 for Counselor
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a counselor.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: /CounselorAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Role != 1) // Ensure user exists and has role = 1
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the counselor.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // POST: /CounselorAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,Email,Username,PasswordHash,Program")] User user)
        {
            if (id != user.Id || user.Role != 1)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency error while updating the counselor.");
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the counselor.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: /CounselorAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the counselor for deletion.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // POST: /CounselorAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null && user.Role == 1)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the counselor.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id && e.Role == 1);
        }
    }
}
