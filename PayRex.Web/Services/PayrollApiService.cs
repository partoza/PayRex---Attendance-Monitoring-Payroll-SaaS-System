using System.Net.Http.Headers;
using System.Text.Json;

namespace PayRex.Web.Services
{
    public interface IPayrollApiService
    {
        Task<List<PayrollPeriodDto>> GetPeriodsAsync(string token);
        Task<(bool success, string message)> CreatePeriodAsync(string token, object period);
        Task<(bool success, string message, int created)> AutoGeneratePeriodsAsync(string token);
        Task<(bool success, string message)> ComputeSalariesAsync(string token, int periodId);
        Task<List<PayrollSummaryDto>> GetSummariesAsync(string token, int periodId);
        Task<List<ContributionDto>> GetContributionsAsync(string token, int? periodId = null, bool selfOnly = false);
        Task<CompensationResponse> GetCompensationAsync(string token);
        Task<(bool success, string message)> AddDeductionAsync(string token, object deduction);
        Task<(bool success, string message)> AddBenefitAsync(string token, object benefit);
        Task<List<PayslipDto>> GetPayslipsAsync(string token, bool selfOnly = false);
        Task<(bool success, string message)> GeneratePayslipsAsync(string token, int periodId);
        Task<List<LeaveRequestDto>> GetLeaveRequestsAsync(string token, string? status = null, bool selfOnly = false);
        Task<List<LeaveRequestDto>> GetArchivedLeaveRequestsAsync(string token, string? status = null);
        Task<(bool success, string message)> CreateLeaveRequestAsync(string token, object request);
        Task<(bool success, string message)> ReviewLeaveRequestAsync(string token, int id, object review);
        Task<(bool success, string message)> ArchiveLeaveRequestAsync(string token, int id);
        Task<(bool success, string message)> RestoreLeaveRequestAsync(string token, int id);
        Task<LeaveBalanceDto> GetLeaveBalanceAsync(string token);
        Task<List<HolidayDto>> GetHolidaysAsync(string token, int year);
        Task<(bool success, string message)> ApprovePeriodAsync(string token, int periodId);
        Task<(bool success, string message)> RejectPeriodAsync(string token, int periodId, string remarks);
        Task<(bool success, string message)> ReleasePeriodAsync(string token, int periodId);
        Task<(bool success, string message)> ArchivePayslipsByPeriodAsync(string token, string periodName);
        Task<(bool success, string message)> RestorePayslipsByPeriodAsync(string token, string periodName);
        Task<List<ArchivedPayslipDto>> GetArchivedPayslipsAsync(string token);
    }

    // DTOs
    public class PayrollPeriodDto
    {
        public int PayrollPeriodId { get; set; }
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public string? PeriodName { get; set; }
        public string Status { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal TotalNetPay { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int PayrollSummaryId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public decimal? BasicPay { get; set; }
        public decimal? OvertimePay { get; set; }
        public decimal? HolidayPay { get; set; }
        public decimal? Allowances { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
    }

    public class ContributionDto
    {
        public int ContributionId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Type { get; set; } = "";
        public decimal EmployeeShare { get; set; }
        public decimal EmployerShare { get; set; }
        public string? PeriodName { get; set; }
        public int PayrollPeriodId { get; set; }
    }

    public class CompensationResponse
    {
        public List<DeductionDto> Deductions { get; set; } = new();
        public List<BenefitDto> Benefits { get; set; } = new();
    }

    public class DeductionDto
    {
        public int DeductionId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Type { get; set; } = "";
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BenefitDto
    {
        public int BenefitId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Type { get; set; } = "";
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PayslipDto
    {
        public int PayslipId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string? PeriodName { get; set; }
        public decimal? BasicPay { get; set; }
        public decimal? OvertimePay { get; set; }
        public decimal? HolidayPay { get; set; }
        public decimal? Allowances { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public bool Released { get; set; }
        public bool IsArchived { get; set; }
        public DateTime GeneratedAt { get; set; }
        // Contribution breakdowns
        public decimal SssContribution { get; set; }
        public decimal PhilHealthContribution { get; set; }
        public decimal PagIbigContribution { get; set; }
        public decimal WithholdingTax { get; set; }
    }

    public class ArchivedPayslipDto
    {
        public int PayslipId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string? PeriodName { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    public class LeaveRequestDto
    {
        public int LeaveRequestId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string LeaveType { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public int TotalDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "";
        public string? ReviewRemarks { get; set; }
        public string? ReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LeaveBalanceDto
    {
        public int SickUsed { get; set; }
        public int VacationUsed { get; set; }
        public int SickTotal { get; set; }
        public int VacationTotal { get; set; }
        public int SickRemaining { get; set; }
        public int VacationRemaining { get; set; }
    }

    public class HolidayDto
    {
        public DateTime Date { get; set; }
        public string LocalName { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Fixed { get; set; }
    }

    // Service Implementation
    public class PayrollApiService : IPayrollApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PayrollApiService> _logger;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public PayrollApiService(HttpClient http, ILogger<PayrollApiService> logger)
        {
            _http = http;
            _logger = logger;
        }

        private void SetAuth(string token) =>
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        public async Task<List<PayrollPeriodDto>> GetPeriodsAsync(string token)
        {
            SetAuth(token);
            try { return await _http.GetFromJsonAsync<List<PayrollPeriodDto>>("api/payroll/periods", JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetPeriods failed"); return new(); }
        }

        public async Task<(bool, string)> CreatePeriodAsync(string token, object period)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync("api/payroll/periods", period);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<(bool, string, int)> AutoGeneratePeriodsAsync(string token)
        {
            SetAuth(token);
            var res = await _http.PostAsync("api/payroll/periods/auto-generate", null);
            var body = await res.Content.ReadAsStringAsync();
            int created = 0;
            try
            {
                var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("created", out var c)) created = c.GetInt32();
            }
            catch { }
            return (res.IsSuccessStatusCode, GetMessage(body), created);
        }

        public async Task<(bool, string)> ComputeSalariesAsync(string token, int periodId)
        {
            SetAuth(token);
            var res = await _http.PostAsync($"api/payroll/compute/{periodId}", null);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<List<PayrollSummaryDto>> GetSummariesAsync(string token, int periodId)
        {
            SetAuth(token);
            try { return await _http.GetFromJsonAsync<List<PayrollSummaryDto>>($"api/payroll/summaries/{periodId}", JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetSummaries failed"); return new(); }
        }

        public async Task<List<ContributionDto>> GetContributionsAsync(string token, int? periodId = null, bool selfOnly = false)
        {
            SetAuth(token);
            var url = "api/payroll/contributions";
            var qp = new List<string>();
            if (periodId.HasValue) qp.Add($"periodId={periodId}");
            if (selfOnly) qp.Add("selfOnly=true");
            if (qp.Any()) url += "?" + string.Join("&", qp);
            try { return await _http.GetFromJsonAsync<List<ContributionDto>>(url, JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetContributions failed"); return new(); }
        }

        public async Task<CompensationResponse> GetCompensationAsync(string token)
        {
            SetAuth(token);
            try { return await _http.GetFromJsonAsync<CompensationResponse>("api/payroll/compensation", JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetCompensation failed"); return new(); }
        }

        public async Task<(bool success, string message)> AddDeductionAsync(string token, object deduction)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync("api/payroll/deductions", deduction, JsonOpts);
            if (res.IsSuccessStatusCode) return (true, "Deduction added successfully.");
            
            try
            {
                var content = await res.Content.ReadAsStringAsync();
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(content, JsonOpts);
                if (dict != null && dict.ContainsKey("message"))
                    return (false, dict["message"].ToString() ?? "Validation failed.");
            }
            catch { }
            return (false, "Failed to add deduction.");
        }

        public async Task<(bool success, string message)> AddBenefitAsync(string token, object benefit)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync("api/payroll/benefits", benefit, JsonOpts);
            if (res.IsSuccessStatusCode) return (true, "Benefit added successfully.");
            
            try
            {
                var content = await res.Content.ReadAsStringAsync();
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(content, JsonOpts);
                if (dict != null && dict.ContainsKey("message"))
                    return (false, dict["message"].ToString() ?? "Validation failed.");
            }
            catch { }
            return (false, "Failed to add benefit.");
        }

        public async Task<List<PayslipDto>> GetPayslipsAsync(string token, bool selfOnly = false)
        {
            SetAuth(token);
            var url = selfOnly ? "api/payroll/payslips?selfOnly=true" : "api/payroll/payslips";
            try { return await _http.GetFromJsonAsync<List<PayslipDto>>(url, JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetPayslips failed"); return new(); }
        }

        public async Task<(bool, string)> GeneratePayslipsAsync(string token, int periodId)
        {
            SetAuth(token);
            var res = await _http.PostAsync($"api/payroll/payslips/generate/{periodId}", null);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<List<LeaveRequestDto>> GetLeaveRequestsAsync(string token, string? status = null, bool selfOnly = false)
        {
            SetAuth(token);
            var qp = new List<string>();
            if (!string.IsNullOrEmpty(status)) qp.Add($"status={status}");
            if (selfOnly) qp.Add("selfOnly=true");
            var url = qp.Any() ? "api/leaverequest?" + string.Join("&", qp) : "api/leaverequest";
            try { return await _http.GetFromJsonAsync<List<LeaveRequestDto>>(url, JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetLeaveRequests failed"); return new(); }
        }

        public async Task<(bool, string)> CreateLeaveRequestAsync(string token, object request)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync("api/leaverequest", request);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<(bool, string)> ReviewLeaveRequestAsync(string token, int id, object review)
        {
            SetAuth(token);
            var res = await _http.PutAsJsonAsync($"api/leaverequest/{id}/review", review);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<List<LeaveRequestDto>> GetArchivedLeaveRequestsAsync(string token, string? status = null)
        {
            SetAuth(token);
            var qp = new List<string>();
            if (!string.IsNullOrEmpty(status)) qp.Add($"status={status}");
            var url = qp.Any() ? "api/leaverequest/archived?" + string.Join("&", qp) : "api/leaverequest/archived";
            try { return await _http.GetFromJsonAsync<List<LeaveRequestDto>>(url, JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetArchivedLeaveRequests failed"); return new(); }
        }

        public async Task<(bool, string)> ArchiveLeaveRequestAsync(string token, int id)
        {
            SetAuth(token);
            var res = await _http.PutAsync($"api/leaverequest/{id}/archive", null);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<(bool, string)> RestoreLeaveRequestAsync(string token, int id)
        {
            SetAuth(token);
            var res = await _http.PutAsync($"api/leaverequest/{id}/unarchive", null);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<LeaveBalanceDto> GetLeaveBalanceAsync(string token)
        {
            SetAuth(token);
            try { return await _http.GetFromJsonAsync<LeaveBalanceDto>("api/leaverequest/balance", JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetLeaveBalance failed"); return new(); }
        }

        public async Task<List<HolidayDto>> GetHolidaysAsync(string token, int year)
        {
            SetAuth(token);
            try { return await _http.GetFromJsonAsync<List<HolidayDto>>($"api/attendance/holidays/{year}", JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetHolidays failed"); return new(); }
        }

        public async Task<(bool success, string message)> ApprovePeriodAsync(string token, int periodId)
        {
            SetAuth(token);
            var res = await _http.PostAsync($"api/payroll/periods/{periodId}/approve", null);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<(bool success, string message)> RejectPeriodAsync(string token, int periodId, string remarks)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync($"api/payroll/periods/{periodId}/reject", new { remarks });
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<(bool success, string message)> ReleasePeriodAsync(string token, int periodId)
        {
            SetAuth(token);
            var res = await _http.PostAsync($"api/payroll/periods/{periodId}/release", null);
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        private static string GetMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() ?? "OK" : "OK";
            }
            catch { return "OK"; }
        }

        public async Task<(bool, string)> ArchivePayslipsByPeriodAsync(string token, string periodName)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync("api/payroll/payslips/archive-period", new { periodName });
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<(bool, string)> RestorePayslipsByPeriodAsync(string token, string periodName)
        {
            SetAuth(token);
            var res = await _http.PostAsJsonAsync("api/payroll/payslips/restore-period", new { periodName });
            var body = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, GetMessage(body));
        }

        public async Task<List<ArchivedPayslipDto>> GetArchivedPayslipsAsync(string token)
        {
            SetAuth(token);
            try { return await _http.GetFromJsonAsync<List<ArchivedPayslipDto>>("api/payroll/payslips/archived", JsonOpts) ?? new(); }
            catch (Exception ex) { _logger.LogError(ex, "GetArchivedPayslips failed"); return new(); }
        }
    }
}
