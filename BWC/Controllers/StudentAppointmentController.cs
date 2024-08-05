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
    public class StudentAppointmentController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly IUserService _userService;

        public StudentAppointmentController(SqlServerDbContext context, IUserService userService)
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
                .Where(a => a.StudentId == userIdInt)
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

            var student = await _context.Users
                .Where(s => s.Id == userIdInt && s.Role == 0)
                .FirstOrDefaultAsync();

            if (student == null)
            {
                // Handle the case where the student is not found
                return RedirectToAction("Index");
            }

            ViewBag.StudentName = $"{student.FirstName} {student.LastName}";

            var counselors = await _context.Users
                .Where(u => u.Role == 1) // Assuming Role 1 is for Counselors
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                })
                .ToListAsync();

            ViewBag.Counselors = new SelectList(counselors, "Value", "Text");

            var model = new AppointmentDto
            {
                StudentId = userIdInt,
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

            // Repopulate the ViewBag.StudentName and ViewBag.Counselors in case of an error
            var student = await _context.Users
                .Where(s => s.Id == model.StudentId && s.Role == 0)
                .FirstOrDefaultAsync();
            ViewBag.StudentName = $"{student.FirstName} {student.LastName}";

            var counselors = await _context.Users
                .Where(u => u.Role == 1) // Assuming Role 1 is for Counselors
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                })
                .ToListAsync();

            ViewBag.Counselors = new SelectList(counselors, "Value", "Text");

            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var appointmentDto = new AppointmentDto
            {
                CounselorId = Convert.ToInt16(appointment.CounselorId),
                StudentId = Convert.ToInt16(appointment.StudentId),
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Status = Convert.ToInt16(appointment.Status),
                AppointmentType = Convert.ToInt16(appointment.AppointmentType)
            };

            var counselors = _context.Users
                .Where(u => u.Role == 1)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                }).ToList();

            var students = _context.Users
                .Where(u => u.Role == 0)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                }).ToList();

            ViewBag.Counselors = counselors;
            ViewBag.Students = students;
            ViewBag.AppointmentId = appointment.Appointment_Id; // Pass the appointment_id to the view

            return View(appointmentDto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("CounselorId,StudentId,AppointmentDate,Reason,Status,AppointmentType")] AppointmentDto appointmentDto)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var appointment = _context.Appointments.Find(id);
                if (appointment == null)
                {
                    return NotFound();
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
                    _context.SaveChanges();
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

            var counselors = _context.Users
                .Where(u => u.Role == 1)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                }).ToList();

            var students = _context.Users
                .Where(u => u.Role == 0)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FirstName + " " + u.LastName
                }).ToList();

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
