using PayRex.Web.DTOs;

namespace PayRex.Web.Services
{
    public interface IAttendanceApiService
    {
        Task<List<AttendanceRecordDto>> GetAttendanceRecordsAsync(string token, DateTime? from = null, DateTime? to = null, string? search = null, string? status = null);
        Task<AttendanceStatsDto> GetTodayStatsAsync(string token);
        Task<(bool Success, string Message, dynamic? Data)> ProcessQrScanAsync(string token, string qrValue, string? deviceId = null);
        Task<AttendanceArchiveResponse> GetArchivedRecordsAsync(string token, DateTime? from = null, DateTime? to = null, string? search = null, int page = 1, int pageSize = 20);
        Task<(bool Found, string? Error, EmployeePreviewDto? Data)> PreviewEmployeeAsync(string token, string qrValue);
        Task<MyAttendanceResponse> GetMyAttendanceAsync(string token, DateTime? from = null, DateTime? to = null);
        Task<CompanyAttendanceConfigDto?> GetCompanyConfigAsync(string token);
    }

    public class CompanyAttendanceConfigDto
    {
        public int PayrollCycle { get; set; }
        public string? ScheduledTimeIn { get; set; }
        public string? ScheduledTimeOut { get; set; }
        public int LateGraceMinutes { get; set; }
        public bool IsConfigured { get; set; }
    }

    public class EmployeePreviewDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string PendingAction { get; set; } = string.Empty;
        public string? ExistingTimeIn { get; set; }
        public string? ExistingTimeOut { get; set; }
        public string CurrentTime { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }

    public class AttendanceRecordDto
    {
        public int AttendanceId { get; set; }
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string? TimeIn { get; set; }
        public string? TimeOut { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal OvertimeHours { get; set; }
        public int LateMinutes { get; set; }
        public int UndertimeMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
    }

    public class AttendanceStatsDto
    {
        public int TotalEmployees { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int Absent { get; set; }
        public int OnLeave { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
    }

    public class AttendanceArchiveResponse
    {
        public List<AttendanceRecordDto> Records { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class MyAttendanceResponse
    {
        public List<AttendanceRecordDto> Records { get; set; } = new();
        public MyAttendanceSummary Summary { get; set; } = new();
    }

    public class MyAttendanceSummary
    {
        public int TotalPresent { get; set; }
        public int TotalLate { get; set; }
        public int TotalAbsent { get; set; }
        public double TotalHours { get; set; }
        public double TotalOvertime { get; set; }
    }
}
