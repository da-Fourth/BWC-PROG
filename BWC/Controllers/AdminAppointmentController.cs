using BWC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BWC.DataConnection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BWC.Controllers
{
    public class AdminAppointmentController : Controller
    {
        private readonly SqlServerDbContext _context;

        public AdminAppointmentController(SqlServerDbContext context)
        {
            _context = context;
        }

        // GET: AdminAppointment
        public IActionResult Index()
        {
            try
            {
                var appointments = _context.Appointments
                    .Include(a => a.Counselor)
                    .Include(a => a.Student)
                    .ToList();

                if (appointments == null || !appointments.Any())
                {
                    appointments = new List<Appointment>();
                }

                return View(appointments);
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while fetching appointments.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: AdminAppointment/Details/5
        public IActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var appointment = _context.Appointments
                    .Include(a => a.Counselor)
                    .Include(a => a.Student)
                    .FirstOrDefault(m => m.Appointment_Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                return View(appointment);
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while fetching appointment details.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: AdminAppointment/Create
        public IActionResult Create()
        {
            try
            {
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

                return View(new AppointmentDto());
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while preparing the create appointment view.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // POST: AdminAppointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("CounselorId,StudentId,AppointmentDate,Reason,Status,AppointmentType")] AppointmentDto appointmentDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var appointment = new Appointment
                    {
                        CounselorId = appointmentDto.CounselorId,
                        StudentId = appointmentDto.StudentId,
                        AppointmentDate = appointmentDto.AppointmentDate,
                        Reason = appointmentDto.Reason,
                        Status = appointmentDto.Status,
                        AppointmentType = appointmentDto.AppointmentType
                    };

                    _context.Add(appointment);
                    _context.SaveChanges();
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
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while creating the appointment.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: AdminAppointment/Edit/5
        public IActionResult Edit(int? id)
        {
            try
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
                    CounselorId = appointment.CounselorId ?? 0,
                    StudentId = appointment.StudentId ?? 0,
                    AppointmentDate = appointment.AppointmentDate,
                    Reason = appointment.Reason,
                    Status = appointment.Status ?? 0,
                    AppointmentType = appointment.AppointmentType ?? 0
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

                return View(appointmentDto);
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while preparing the edit appointment view.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("CounselorId,StudentId,AppointmentDate,Reason,Status,AppointmentType")] AppointmentDto appointmentDto)
        {
            try
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
                            // Log the exception
                            // _logger.LogError(ex, "An error occurred while updating the appointment.");
                            return StatusCode(500, "Internal server error. Please try again later.");
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
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while editing the appointment.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // GET: AdminAppointment/Delete/5
        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var appointment = _context.Appointments
                    .Include(a => a.Counselor)
                    .Include(a => a.Student)
                    .FirstOrDefault(m => m.Appointment_Id == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                return View(appointment);
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while fetching the appointment for deletion.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        // POST: AdminAppointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var appointment = _context.Appointments.Find(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception
                // _logger.LogError(ex, "An error occurred while deleting the appointment.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Appointment_Id == id);
        }
    }
}
