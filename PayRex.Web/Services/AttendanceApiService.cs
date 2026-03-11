using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Services
{
    public class AttendanceApiService : IAttendanceApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AttendanceApiService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AttendanceApiService(HttpClient httpClient, ILogger<AttendanceApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<AttendanceRecordDto>> GetAttendanceRecordsAsync(string token, DateTime? from = null, DateTime? to = null, string? search = null, string? status = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var queryParams = new List<string>();
                if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");

                var url = "api/Attendance" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<AttendanceRecordDto>>(content, JsonOptions) ?? new List<AttendanceRecordDto>();
                }
                return new List<AttendanceRecordDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance records");
                return new List<AttendanceRecordDto>();
            }
        }

        public async Task<AttendanceStatsDto> GetTodayStatsAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/Attendance/stats");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AttendanceStatsDto>(content, JsonOptions) ?? new AttendanceStatsDto();
                }
                return new AttendanceStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching attendance stats");
                return new AttendanceStatsDto();
            }
        }

        public async Task<(bool Success, string Message, dynamic? Data)> ProcessQrScanAsync(string token, string qrValue, string? deviceId = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var body = new { QrValue = qrValue, DeviceId = deviceId ?? "web" };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/Attendance/scan", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Scan processed";
                    return (true, message ?? "Success", null);
                }
                else
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var message = doc.RootElement.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Failed to process scan";
                    return (false, message ?? "Error", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing QR scan");
                return (false, "An error occurred while connecting to the server.", null);
            }
        }

        public async Task<(bool Found, string? Error, EmployeePreviewDto? Data)> PreviewEmployeeAsync(string token, string qrValue)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"api/Attendance/preview?qrValue={Uri.EscapeDataString(qrValue)}";
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<EmployeePreviewDto>(content, JsonOptions);
                    return (true, null, data);
                }

                string errorMsg = "Employee not found for this QR code.";
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("message", out var m))
                        errorMsg = m.GetString() ?? errorMsg;
                }
                catch { }

                return (false, errorMsg, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing employee");
                return (false, "Could not connect to the server. Please try again.", null);
            }
        }

        public async Task<AttendanceArchiveResponse> GetArchivedRecordsAsync(string token, DateTime? from = null, DateTime? to = null, string? search = null, int page = 1, int pageSize = 20)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var queryParams = new List<string>();
                if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");
                if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"pageSize={pageSize}");

                var url = "api/Attendance/archives" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<AttendanceArchiveResponse>(content, JsonOptions) ?? new AttendanceArchiveResponse();
                }
                return new AttendanceArchiveResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching archived attendance records");
                return new AttendanceArchiveResponse();
            }
        }

        public async Task<MyAttendanceResponse> GetMyAttendanceAsync(string token, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var queryParams = new List<string>();
                if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
                if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");

                var url = "api/Attendance/my" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<MyAttendanceResponse>(content, JsonOptions) ?? new MyAttendanceResponse();
                }
                return new MyAttendanceResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching my attendance records");
                return new MyAttendanceResponse();
            }
        }

        public async Task<CompanyAttendanceConfigDto?> GetCompanyConfigAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("api/Attendance/company-config");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<CompanyAttendanceConfigDto>(content, JsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company attendance config");
                return null;
            }
        }
    }
}
