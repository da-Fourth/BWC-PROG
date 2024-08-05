using BWC.Models;
using BWC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BWC.DataConnection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BWC.Controllers
{
    public class CounselorAppointmentController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly IUserService _userService;

        public CounselorAppointmentController(SqlServerDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                // User is not logged in, redirect to login page
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                // Handle the case where the userId is not a valid integer
                return RedirectToAction("Index");
            }

            var appointments = await _context.Appointments
                .Where(a => a.CounselorId == userIdInt)
                .Include(a => a.Student) // Ensure related Student data is loaded
                .Include(a => a.Counselor) // Ensure related Counselor data is loaded, even if null
                .ToListAsync();

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                // User is not logged in, redirect to login page
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                // Handle the case where the userId is not a valid integer
                return RedirectToAction("Index");
            }

            var counselor = await _context.Users
                .Where(s => s.Id == userIdInt && s.Role == 1)
                .FirstOrDefaultAsync();

            if (counselor == null)
            {
                // Handle the case where the counselor is not found
                return RedirectToAction("Index");
            }

            ViewBag.CounselorName = $"{counselor.FirstName} {counselor.LastName}";

            var students = await _context.Users
                .Where(u => u.Role == 0) // Assuming Role 0 is for Students
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                })
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "Value", "Text");

            var model = new AppointmentDto
            {
                CounselorId = userIdInt,
                Status = 0 // Default to Pending
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentDto model)
        {
            if (ModelState.IsValid)
            {
                var appointment = new Appointment
                {
                    StudentId = model.StudentId,
                    AppointmentDate = model.AppointmentDate,
                    Reason = model.Reason,
                    Status = model.Status,
                    AppointmentType = model.AppointmentType,
                    CounselorId = model.CounselorId
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // Repopulate the ViewBag.CounselorName and ViewBag.Students in case of an error
            var counselor = await _context.Users
                .Where(s => s.Id == model.CounselorId && s.Role == 1)
                .FirstOrDefaultAsync();
            ViewBag.CounselorName = $"{counselor.FirstName} {counselor.LastName}";

            var students = await _context.Users
                .Where(u => u.Role == 0) // Assuming Role 0 is for Students
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                })
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "Value", "Text");

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var userId = _userService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                // User is not logged in, redirect to login page
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                // Handle the case where the userId is not a valid integer
                return RedirectToAction("Index");
            }

            var counselor = await _context.Users
                .Where(s => s.Id == userIdInt && s.Role == 1)
                .FirstOrDefaultAsync();

            if (counselor == null)
            {
                // Handle the case where the counselor is not found
                return RedirectToAction("Index");
            }

            ViewBag.CounselorName = $"{counselor.FirstName} {counselor.LastName}";

            var appointmentDto = new AppointmentDto
            {
                CounselorId = Convert.ToInt16(appointment.CounselorId),
                StudentId = Convert.ToInt16(appointment.StudentId),
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Status = Convert.ToInt16(appointment.Status),
                AppointmentType = Convert.ToInt16(appointment.AppointmentType)
            };

            var students = await _context.Users
                .Where(u => u.Role == 0) // Assuming Role 0 is for Students
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                })
                .ToListAsync();

            ViewBag.Students = new SelectList(students, "Value", "Text");

            return View(appointmentDto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CounselorId,StudentId,AppointmentDate,Reason,Status,AppointmentType")] AppointmentDto appointmentDto)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                // Check if the CounselorId exists in the Users table
                var counselorExists = await _context.Users.AnyAsync(u => u.Id == appointmentDto.CounselorId && u.Role == 1);
                if (!counselorExists)
                {
                    ModelState.AddModelError("CounselorId", "The specified counselor does not exist.");
                    return View(appointmentDto);
                }

                appointment.CounselorId = appointmentDto.CounselorId;
                appointment.StudentId = appointmentDto.StudentId;
                appointment.AppointmentDate = appointmentDto.AppointmentDate;
                appointment.Reason = appointmentDto.Reason;
                appointment.Status = appointmentDto.Status;
                appointment.AppointmentType = appointmentDto.AppointmentType;

                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Appointment_Id))
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

            var counselors = await _context.Users
                .Where(u => u.Role == 1)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                }).ToListAsync();

            var students = await _context.Users
                .Where(u => u.Role == 0)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                }).ToListAsync();

            ViewBag.Counselors = counselors;
            ViewBag.Students = students;

            return View(appointmentDto);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int appointment_id)
        {
            var appointment = await _context.Appointments.FindAsync(appointment_id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Appointment_Id == id);
        }
    }
}
