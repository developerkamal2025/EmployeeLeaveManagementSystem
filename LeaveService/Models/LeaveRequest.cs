using System.ComponentModel.DataAnnotations;

namespace LeaveService.Models
{
    public class Leave
    {
        public int Id { get; set; }
        public string EmployeeEmail { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Pending";
    }
    public class LeaveRequest
    {
        [EmailAddress]
        public string EmployeeEmail { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
