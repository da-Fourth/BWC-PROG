using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BWC.DataConnection; // Update with the actual namespace for your DbContext
using BWC.Models;

namespace BWC.Controllers
{
    public class StudentAdminController : Controller
    {
        private readonly SqlServerDbContext _context;

        public StudentAdminController(SqlServerDbContext context)
        {
            _context = context;
        }

        // GET: /StudentAdmin
        public async Task<IActionResult> Index()
        {
            try
            {
                var students = await _context.Users
                    .Where(u => u.Role == 0) // Fetch users with role = 0 (Student)
                    .ToListAsync();

                return View(students);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while fetching the students.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: /StudentAdmin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /StudentAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,Email,Username,PasswordHash,Program")] User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                user.Role = 0; // Set role to 0 for Student
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while creating the student.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the student.");
                return View(user);
            }
        }

        // GET: /StudentAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Role != 0) // Ensure user exists and has role = 0
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while fetching the student for editing.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: /StudentAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,Email,Username,PasswordHash,Program")] User user)
        {
            if (id != user.Id || user.Role != 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    // Log the exception (optional)
                    // _logger.LogError(ex, "A concurrency error occurred while updating the student.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while updating the student.");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the student.");
                return View(user);
            }
        }

        // GET: /StudentAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id && u.Role == 0);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while fetching the student for deletion.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: /StudentAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null && user.Role == 0)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while deleting the student.");
                return StatusCode(500, "Internal server error");
            }
        }

        private bool UserExists(int id)
        {
            try
            {
                return _context.Users.Any(e => e.Id == id && e.Role == 0);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                // _logger.LogError(ex, "An error occurred while checking if the user exists.");
                return false;
            }
        }
    }
}
