using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Employee")]
    public class AttendanceModel : PageModel
    {
        public List<AttendanceRecord> Records { get; set; } = new();
        public AttendanceStats Stats { get; set; } = new();
        public string CurrentDateRange { get; set; } = "";

        public void OnGet()
        {
            var now = DateTime.Now;
            CurrentDateRange = now.ToString("MMM 01, yyyy") + " - " + now.ToString("MMM dd, yyyy");

            Stats = new AttendanceStats
            {
                Present = 128,
                Late = 12,
                Absent = 5,
                OnLeave = 3
            };

            Records = new List<AttendanceRecord>
            {
                new AttendanceRecord { EmployeeName = "Juan Cruz", Date = now, TimeIn = now.Date.AddHours(7).AddMinutes(55), TimeOut = now.Date.AddHours(17).AddMinutes(5), WorkHours = 8.0, Status = "Present", Remarks = "Regular" },
                new AttendanceRecord { EmployeeName = "Maria Santos", Date = now, TimeIn = now.Date.AddHours(8).AddMinutes(15), TimeOut = now.Date.AddHours(17).AddMinutes(15), WorkHours = 8.0, Status = "Late", Remarks = "15m late" },
                new AttendanceRecord { EmployeeName = "Jose Reyes", Date = now, TimeIn = now.Date.AddHours(7).AddMinutes(45), TimeOut = now.Date.AddHours(16).AddMinutes(45), WorkHours = 8.0, Status = "Present", Remarks = "Regular" },
                new AttendanceRecord { EmployeeName = "Ana Dizon", Date = now, TimeIn = null, TimeOut = null, WorkHours = 0.0, Status = "On Leave", Remarks = "Sick Leave" },
                new AttendanceRecord { EmployeeName = "Pedro Garcia", Date = now, TimeIn = now.Date.AddHours(9).AddMinutes(30), TimeOut = now.Date.AddHours(18).AddMinutes(30), WorkHours = 8.0, Status = "Late", Remarks = "1h 30m late" },
                new AttendanceRecord { EmployeeName = "Luz Ramos", Date = now, TimeIn = now.Date.AddHours(8), TimeOut = now.Date.AddHours(17), WorkHours = 8.0, Status = "Present", Remarks = "Regular" },
                new AttendanceRecord { EmployeeName = "Rosa Torres", Date = now, TimeIn = now.Date.AddHours(7).AddMinutes(58), TimeOut = now.Date.AddHours(17).AddMinutes(2), WorkHours = 8.0, Status = "Present", Remarks = "Regular" },
                new AttendanceRecord { EmployeeName = "Luis Tan", Date = now, TimeIn = null, TimeOut = null, WorkHours = 0.0, Status = "Absent", Remarks = "No Notice" },
                new AttendanceRecord { EmployeeName = "Juan Cruz", Date = now.AddDays(-1), TimeIn = now.Date.AddDays(-1).AddHours(7).AddMinutes(50), TimeOut = now.Date.AddDays(-1).AddHours(17), WorkHours = 8.0, Status = "Present", Remarks = "Regular" },
                new AttendanceRecord { EmployeeName = "Maria Santos", Date = now.AddDays(-1), TimeIn = now.Date.AddDays(-1).AddHours(8).AddMinutes(5), TimeOut = now.Date.AddDays(-1).AddHours(17).AddMinutes(5), WorkHours = 8.0, Status = "Late", Remarks = "5m late" }
            };
        }

        public class AttendanceStats
        {
            public int Present { get; set; }
            public int Late { get; set; }
            public int Absent { get; set; }
            public int OnLeave { get; set; }
        }

        public class AttendanceRecord
        {
            public string EmployeeName { get; set; } = "";
            public DateTime Date { get; set; }
            public DateTime? TimeIn { get; set; }
            public DateTime? TimeOut { get; set; }
            public double WorkHours { get; set; }
            public string Status { get; set; } = "";
            public string Remarks { get; set; } = "";
        }
    }
}
