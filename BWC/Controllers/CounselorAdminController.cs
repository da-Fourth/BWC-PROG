using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BWC.DataConnection; // Update with the actual namespace for your DbContext
using BWC.Models;

namespace BWC.Controllers
{
    public class CounselorAdminController : Controller
    {
        private readonly SqlServerDbContext _context;

        public CounselorAdminController(SqlServerDbContext context)
        {
            _context = context;
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
                // Log the exception or handle it accordingly
                // Example: _logger.LogError(ex, "An error occurred while fetching counselors.");
                return View("Error"); // Return an error view if needed
            }
        }

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
                // Log the exception or handle it accordingly
                // Example: _logger.LogError(ex, "An error occurred while creating a counselor.");
                return View("Error"); // Return an error view if needed
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
                // Log the exception or handle it accordingly
                // Example: _logger.LogError(ex, "An error occurred while fetching the counselor.");
                return View("Error"); // Return an error view if needed
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
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                // Example: _logger.LogError(ex, "An error occurred while updating the counselor.");
                return View("Error"); // Return an error view if needed
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
                // Log the exception or handle it accordingly
                // Example: _logger.LogError(ex, "An error occurred while fetching the counselor for deletion.");
                return View("Error"); // Return an error view if needed
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
                // Log the exception or handle it accordingly
                // Example: _logger.LogError(ex, "An error occurred while deleting the counselor.");
                return View("Error"); // Return an error view if needed
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id && e.Role == 1);
        }
    }
}
