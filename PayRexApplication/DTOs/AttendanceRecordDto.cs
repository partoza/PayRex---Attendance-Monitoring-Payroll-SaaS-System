namespace PayRexApplication.DTOs
{
    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string TimeIn { get; set; }
        public string TimeOut { get; set; }
        public double TotalHoursWorked { get; set; }
        public double OvertimeHours { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public bool IsHoliday { get; set; }
        public string HolidayName { get; set; }
    }
}
