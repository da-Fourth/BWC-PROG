using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BWC.Models
{
    public class Appointment
    {
        [Key]
        public int Appointment_Id { get; set; }

        public int? CounselorId { get; set; }

        [ForeignKey("CounselorId")]
        public User Counselor { get; set; }

        public int? StudentId { get; set; }

        [ForeignKey("StudentId")]
        public User Student { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string Reason { get; set; }

        public int? Status { get; set; } // Values: 1 (Pending), 2 (Approved), 3 (No-Show), 4 (Cancelled), 5 (Complete)

        //[StringLength(255)]
        //public string Program { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? AppointmentType { get; set; } // Values: 1 (Initial), 2 (Follow Up)
    }
}
