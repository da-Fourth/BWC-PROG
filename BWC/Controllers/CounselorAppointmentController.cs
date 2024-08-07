using BWC.Models;
using BWC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BWC.DataConnection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging; // Add using directive for logging
using Microsoft.AspNetCore.Authorization;

namespace BWC.Controllers
{
    [Authorize(Policy = "CounselorPolicy")]
    public class CounselorAppointmentController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<CounselorAppointmentController> _logger; // Add logger

        public CounselorAppointmentController(SqlServerDbContext context, IUserService userService, ILogger<CounselorAppointmentController> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                _logger.LogWarning("Invalid userId format: {UserId}", userId);
                return RedirectToAction("Index");
            }

            try
            {
                var appointments = await _context.Appointments
                    .Where(a => a.CounselorId == userIdInt)
                    .Include(a => a.Student)
                    .Include(a => a.Counselor)
                    .ToListAsync();

                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching appointments.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = _userService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Login");
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                _logger.LogWarning("Invalid userId format: {UserId}", userId);
                return RedirectToAction("Index");
            }

            try
            {
                var counselor = await _context.Users
                    .Where(s => s.Id == userIdInt && s.Role == 1)
                    .FirstOrDefaultAsync();

                if (counselor == null)
                {
                    _logger.LogWarning("Counselor with ID {UserIdInt} not found.", userIdInt);
                    return RedirectToAction("Index");
                }

                ViewBag.CounselorName = $"{counselor.FirstName} {counselor.LastName}";

                var students = await _context.Users
                    .Where(u => u.Role == 0)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while preparing the create appointment view.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentDto model)
        {
            if (ModelState.IsValid)
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating an appointment.");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the appointment. Please try again.");
                }
            }

            try
            {
                var counselor = await _context.Users
                    .Where(s => s.Id == model.CounselorId && s.Role == 1)
                    .FirstOrDefaultAsync();
                ViewBag.CounselorName = $"{counselor.FirstName} {counselor.LastName}";

                var students = await _context.Users
                    .Where(u => u.Role == 0)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.FirstName} {c.LastName}"
                    })
                    .ToListAsync();

                ViewBag.Students = new SelectList(students, "Value", "Text");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while repopulating the create appointment view.");
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                var userId = _userService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Index", "Login");
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    _logger.LogWarning("Invalid userId format: {UserId}", userId);
                    return RedirectToAction("Index");
                }

                var counselor = await _context.Users
                    .Where(s => s.Id == userIdInt && s.Role == 1)
                    .FirstOrDefaultAsync();

                if (counselor == null)
                {
                    _logger.LogWarning("Counselor with ID {UserIdInt} not found.", userIdInt);
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
                    .Where(u => u.Role == 0)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.FirstName} {c.LastName}"
                    })
                    .ToListAsync();

                ViewBag.Students = new SelectList(students, "Value", "Text");

                return View(appointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while preparing the edit appointment view.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
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
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

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

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Concurrency error while updating appointment with ID {Id}.", id);
                        return StatusCode(500, "Internal server error. Please try again later.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the appointment.");
                    return StatusCode(500, "Internal server error. Please try again later.");
                }
            }

            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while repopulating the edit appointment view.");
            }

            return View(appointmentDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int appointment_id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting appointment with ID {AppointmentId}.", appointment_id);
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Appointment_Id == id);
        }
    }
}
