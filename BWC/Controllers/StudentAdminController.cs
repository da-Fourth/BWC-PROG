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
            var students = await _context.Users
                .Where(u => u.Role == 0) // Fetch users with role = 0 (Student)
                .ToListAsync();

            return View(students);
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
            if (ModelState.IsValid)
            {
                user.Role = 0; // Set role to 0 for Student
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: /StudentAdmin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != 0) // Ensure user exists and has role = 0
            {
                return NotFound();
            }
            return View(user);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: /StudentAdmin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == 0);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: /StudentAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null && user.Role == 0)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id && e.Role == 0);
        }
    }
}
