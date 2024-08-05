namespace BWC.Models
{
    public class AppointmentDto
    {
        public int CounselorId { get; set; }
        public int StudentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Reason { get; set; }
        public int Status { get; set; }
        public int AppointmentType { get; set; }
    }


}
