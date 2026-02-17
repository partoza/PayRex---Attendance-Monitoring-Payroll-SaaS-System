using PayRex.Web.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace PayRex.Web.Services
{
 public class AuthApiService : IAuthApiService
 {
 private readonly HttpClient _httpClient;
 private readonly ILogger<AuthApiService> _logger;
 private static readonly JsonSerializerOptions JsonOptions = new()
 {
 PropertyNameCaseInsensitive = true
 };

 public AuthApiService(HttpClient httpClient, ILogger<AuthApiService> logger)
 {
 _httpClient = httpClient;
 _logger = logger;
 }

 // Ensure token does not include the "Bearer " prefix
 private static string NormalizeToken(string? token)
 {
 if (string.IsNullOrWhiteSpace(token)) return string.Empty;
 var t = token.Trim();
 if (t.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
 t = t.Substring(7).Trim();
 return t;
 }

 public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
 {
 try
 {
 var json = JsonSerializer.Serialize(request, JsonOptions);
 var content = new StringContent(json, Encoding.UTF8, "application/json");

 var response = await _httpClient.PostAsync("api/auth/login", content);

 var responseContent = await response.Content.ReadAsStringAsync();

 if (response.IsSuccessStatusCode)
 {
 return JsonSerializer.Deserialize<LoginResponseDto>(responseContent, JsonOptions);
 }

 // Log non-success responses for diagnosis
 _logger.LogWarning("Login API returned non-success status {Status}. Body: {Body}", response.StatusCode, responseContent);

 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 string? message = null;
 bool isLockedOut = false;
 int lockoutRemaining =0;

 if (doc.ValueKind == JsonValueKind.Object)
 {
 if (doc.TryGetProperty("message", out var m))
 {
 message = m.GetString();
 }
 if (doc.TryGetProperty("remainingSeconds", out var rs) && rs.TryGetInt32(out var secs))
 {
 isLockedOut = true;
 lockoutRemaining = secs;
 }
 if (message == null && doc.TryGetProperty("lockedUntil", out var _))
 {
 isLockedOut = true;
 }
 }

 // If API didn't provide a message, include a short excerpt of the body for debugging
 var shortBody = string.IsNullOrWhiteSpace(responseContent) ? null : (responseContent.Length >300 ? responseContent[..300] + "..." : responseContent);

 return new LoginResponseDto
 {
 RequireTotp = false,
 Message = message ?? ($"Login failed ({response.StatusCode})" + (shortBody != null ? $": {shortBody}" : ".")),
 IsLockedOut = isLockedOut,
 LockoutRemainingSeconds = lockoutRemaining
 };
 }
 catch (Exception ex)
 {
 _logger.LogWarning(ex, "Failed to parse error response from login endpoint");
 var shortBody = string.IsNullOrWhiteSpace(responseContent) ? null : (responseContent.Length >300 ? responseContent[..300] + "..." : responseContent);
 return new LoginResponseDto { Message = $"Login failed ({response.StatusCode})" + (shortBody != null ? $": {shortBody}" : ".") };
 }
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling login API");
 return new LoginResponseDto
 {
 RequireTotp = false,
 Message = "Authentication service unavailable. " + ex.Message
 };
 }
 }

 public async Task<LoginResponseDto?> VerifyTotpAsync(TotpVerificationDto request)
 {
 try
 {
 var json = JsonSerializer.Serialize(request, JsonOptions);
 var content = new StringContent(json, Encoding.UTF8, "application/json");

 // API endpoint for login TOTP verification is under auth controller: api/auth/totp/verify
 var response = await _httpClient.PostAsync("api/auth/totp/verify", content);

 var responseContent = await response.Content.ReadAsStringAsync();

 if (response.IsSuccessStatusCode)
 {
 return JsonSerializer.Deserialize<LoginResponseDto>(responseContent, JsonOptions);
 }

 // Try parse error message
 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("message", out var m))
 {
 return new LoginResponseDto { Message = m.GetString() };
 }
 }
 catch (Exception ex)
 {
 _logger.LogDebug(ex, "Failed to parse TOTP verify error body");
 }

 if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
 {
 _logger.LogWarning("TOTP verify returned unauthorized/forbidden: {Status}", response.StatusCode);
 return new LoginResponseDto { Message = "Invalid TOTP code or unauthorized." };
 }

 _logger.LogWarning("TOTP verification failed with status code: {StatusCode} Body: {Body}", response.StatusCode, responseContent);
 return new LoginResponseDto { Message = $"TOTP verification failed ({response.StatusCode})." };
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling TOTP verification API");
 return new LoginResponseDto { Message = "TOTP verification service unavailable. " + ex.Message };
 }
 }

 public async Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request)
 {
 try
 {
 var json = JsonSerializer.Serialize(request, JsonOptions);
 var content = new StringContent(json, Encoding.UTF8, "application/json");

 var response = await _httpClient.PostAsync("api/auth/register", content);

 if (response.IsSuccessStatusCode)
 {
 var responseContent = await response.Content.ReadAsStringAsync();
 return JsonSerializer.Deserialize<RegisterResponseDto>(responseContent, JsonOptions);
 }

 if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
 {
 _logger.LogWarning("Registration failed: Email already exists");
 return null;
 }

 _logger.LogWarning("Registration failed with status code: {StatusCode}", response.StatusCode);
 return null;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling register API");
 return null;
 }
 }

 public async Task<UserInfoDto?> GetCurrentUserAsync(string token)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean))
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var response = await _httpClient.SendAsync(request);

 if (response.IsSuccessStatusCode)
 {
 var responseContent = await response.Content.ReadAsStringAsync();
 return JsonSerializer.Deserialize<UserInfoDto>(responseContent, JsonOptions);
 }

 _logger.LogWarning("GetCurrentUser failed with status code: {StatusCode}", response.StatusCode);
 return null;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling GetCurrentUser API");
 return null;
 }
 }

 // ========================
 // Profile Methods
 // ========================

 public async Task<UserProfileDto?> GetUserProfileAsync(string token)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Get, "api/profile");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean))
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var response = await _httpClient.SendAsync(request);

 if (response.IsSuccessStatusCode)
 {
 var responseContent = await response.Content.ReadAsStringAsync();
 return JsonSerializer.Deserialize<UserProfileDto>(responseContent, JsonOptions);
 }

 _logger.LogWarning("GetUserProfile failed with status code: {StatusCode}", response.StatusCode);
 return null;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling GetUserProfile API");
 return null;
 }
 }

 public async Task<(bool Success, string? Message)> UpdateProfileAsync(string token, UpdateProfileRequestDto dto)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Put, "api/profile");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean))
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);
 request.Content = new StringContent(JsonSerializer.Serialize(dto, JsonOptions), Encoding.UTF8, "application/json");

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 if (response.IsSuccessStatusCode)
 {
 return (true, "Profile updated successfully");
 }

 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (doc.TryGetProperty("message", out var m))
 {
 return (false, m.GetString());
 }
 }
 catch { }

 return (false, "Failed to update profile");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling UpdateProfile API");
 return (false, "An error occurred while updating profile");
 }
 }

 public async Task<(bool Success, string? Message, bool RequireRelogin)> ChangePasswordAsync(string token, ChangePasswordRequestDto dto)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Post, "api/profile/change-password");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean))
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var jsonPayload = JsonSerializer.Serialize(dto, JsonOptions);
 _logger.LogInformation("ChangePassword request payload: {Payload}", jsonPayload);

 request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 _logger.LogInformation("ChangePassword response: Status={Status}, Body={Body}", response.StatusCode, responseContent);

 if (response.IsSuccessStatusCode)
 {
 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 var requireRelogin = doc.TryGetProperty("requireRelogin", out var r) && r.GetBoolean();
 var message = doc.TryGetProperty("message", out var m) ? m.GetString() : "Password changed successfully";
 return (true, message, requireRelogin);
 }
 catch { }
 return (true, "Password changed successfully", true);
 }

 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (doc.TryGetProperty("message", out var m))
 {
 return (false, m.GetString(), false);
 }
 // Check for validation errors array
 if (doc.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
 {
 var errorList = new List<string>();
 foreach (var error in errors.EnumerateArray())
 {
 if (error.ValueKind == JsonValueKind.String)
 errorList.Add(error.GetString() ?? "");
 }
 if (errorList.Count >0)
 return (false, string.Join("; ", errorList), false);
 }
 }
 catch { }

 return (false, $"Failed to change password (HTTP {(int)response.StatusCode})", false);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling ChangePassword API");
 return (false, "An error occurred while changing password: " + ex.Message, false);
 }
 }

 // ========================
 // Profile Image Methods
 // ========================

 public async Task<ProfileImageResponseDto?> UploadProfileImageAsync(string token, Stream imageStream, string fileName, string contentType)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Post, "api/profile/image");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean))
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var content = new MultipartFormDataContent();
 var streamContent = new StreamContent(imageStream);
 streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
 content.Add(streamContent, "file", fileName);

 request.Content = content;

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 return JsonSerializer.Deserialize<ProfileImageResponseDto>(responseContent, JsonOptions);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling UploadProfileImage API");
 return new ProfileImageResponseDto { Success = false, Message = "An error occurred while uploading image" };
 }
 }

 public async Task<(bool Success, string? Message)> RemoveProfileImageAsync(string token)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Delete, "api/profile/image");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean))
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var response = await _httpClient.SendAsync(request);

 if (response.IsSuccessStatusCode)
 {
 return (true, "Profile image removed successfully");
 }

 return (false, "Failed to remove profile image");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling RemoveProfileImage API");
 return (false, "An error occurred while removing image");
 }
 }

 // ========================
 //2FA Methods
 // ========================

 public async Task<ClientTotpSetupResponseDto?> SetupTotpAsync(string token)
 {
 try
 {
 var clean = NormalizeToken(token);
 if (string.IsNullOrEmpty(clean))
 {
 _logger.LogWarning("SetupTotp: no token available");
 return new ClientTotpSetupResponseDto { Success = false, Message = "Not authenticated" };
 }

 using var request = new HttpRequestMessage(HttpMethod.Post, "api/profile/2fa/setup");
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 _logger.LogInformation("SetupTotp response: Status={Status}, Body={Body}", response.StatusCode, responseContent);

 if (!response.IsSuccessStatusCode)
 {
 // Parse error message from API
 var errorMsg = $"Setup failed ({response.StatusCode})";
 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("message", out var m))
 errorMsg = m.GetString() ?? errorMsg;
 }
 catch { }

 return new ClientTotpSetupResponseDto { Success = false, Message = errorMsg };
 }

 // Parse success response
 try
 {
 var result = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (result.ValueKind == JsonValueKind.Object)
 {
 return new ClientTotpSetupResponseDto
 {
 Success = true,
 SecretKey = result.TryGetProperty("secretKey", out var s) ? s.GetString() ?? "" : "",
 QrCodeUri = result.TryGetProperty("qrCodeUri", out var q) ? q.GetString() ?? "" : "",
 ManualEntryKey = result.TryGetProperty("manualEntryKey", out var mk) ? mk.GetString() ?? "" : ""
 };
 }
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to parse SetupTotp response");
 }

 return new ClientTotpSetupResponseDto { Success = false, Message = "Failed to parse setup response" };
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling SetupTotp API");
 return new ClientTotpSetupResponseDto { Success = false, Message = "Error calling setup API: " + ex.Message };
 }
 }

 public async Task<ClientTotpEnableResponseDto?> EnableTotpAsync(string token, EnableTotpRequestDto requestDto)
 {
 try
 {
 var clean = NormalizeToken(token);
 if (string.IsNullOrEmpty(clean))
 return new ClientTotpEnableResponseDto { Success = false, Message = "Not authenticated" };

 using var request = new HttpRequestMessage(HttpMethod.Post, "api/profile/2fa/enable");
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);
 request.Content = new StringContent(JsonSerializer.Serialize(requestDto, JsonOptions), Encoding.UTF8, "application/json");

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 _logger.LogInformation("EnableTotp response: Status={Status}, Body={Body}", response.StatusCode, responseContent);

 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (doc.ValueKind == JsonValueKind.Object)
 {
 var success = response.IsSuccessStatusCode && doc.TryGetProperty("success", out var sv) && sv.GetBoolean();
 var message = doc.TryGetProperty("message", out var m) ? m.GetString() : null;
 List<string>? codes = null;

 if (doc.TryGetProperty("recoveryCodes", out var rc) && rc.ValueKind == JsonValueKind.Array)
 {
 codes = new List<string>();
 foreach (var c in rc.EnumerateArray())
 codes.Add(c.GetString() ?? string.Empty);
 }

 return new ClientTotpEnableResponseDto
 {
 Success = success,
 Message = message ?? (success ? "2FA enabled" : $"Enable failed ({response.StatusCode})"),
 RecoveryCodes = codes
 };
 }
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Failed to parse EnableTotp response");
 }

 return new ClientTotpEnableResponseDto { Success = false, Message = $"Enable TOTP failed ({response.StatusCode})" };
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling EnableTotp API");
 return new ClientTotpEnableResponseDto { Success = false, Message = ex.Message };
 }
 }

 public async Task<(bool Success, string? Message)> DisableTotpAsync(string token)
 {
 try
 {
 var clean = NormalizeToken(token);
 if (string.IsNullOrEmpty(clean))
 return (false, "Not authenticated");

 using var request = new HttpRequestMessage(HttpMethod.Post, "api/profile/2fa/disable");
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 try
 {
 var doc = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
 if (doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("message", out var m))
 return (response.IsSuccessStatusCode, m.GetString());
 }
 catch { }

 return response.IsSuccessStatusCode
 ? (true, "Two-factor authentication disabled successfully")
 : (false, $"Disable TOTP failed ({response.StatusCode})");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling DisableTotp API");
 return (false, ex.Message);
 }
 }

 public async Task<TwoFactorStatusDto?> GetTwoFactorStatusAsync(string token)
 {
 try
 {
 var clean = NormalizeToken(token);
 if (string.IsNullOrEmpty(clean)) return null;

 using var request = new HttpRequestMessage(HttpMethod.Get, "api/profile/2fa/status");
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();

 if (!response.IsSuccessStatusCode)
 {
 _logger.LogWarning("GetTwoFactorStatus failed: {Status}", response.StatusCode);
 return null;
 }

 return JsonSerializer.Deserialize<TwoFactorStatusDto>(responseContent, JsonOptions);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling GetTwoFactorStatus API");
 return null;
 }
 }

 public async Task<CompanyProfileDto?> GetCompanyProfileAsync(string token)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/company");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);

 var sw = Stopwatch.StartNew();
 var response = await _httpClient.SendAsync(request);
 var responseContent = await response.Content.ReadAsStringAsync();
 sw.Stop();

 // Log elapsed time and status to help diagnose slow API calls
 _logger.LogInformation("GetCompanyProfileAsync: Status={Status}, ElapsedMs={ElapsedMs}, Url={Url}", response.StatusCode, sw.ElapsedMilliseconds, _httpClient?.BaseAddress?.ToString() + "api/auth/company");

 if (response.IsSuccessStatusCode)
 {
 return JsonSerializer.Deserialize<CompanyProfileDto>(responseContent, JsonOptions);
 }
 _logger.LogWarning("GetCompanyProfile failed with status code: {Status}", response.StatusCode);
 return null;
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling GetCompanyProfile API");
 return null;
 }
 }

 public async Task<(bool Success, string? Message)> UpdateCompanyProfileAsync(string token, UpdateCompanyRequestDto dto)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Put, "api/auth/company");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);
 request.Content = new StringContent(JsonSerializer.Serialize(dto, JsonOptions), Encoding.UTF8, "application/json");

 var response = await _httpClient.SendAsync(request);
 var content = await response.Content.ReadAsStringAsync();
 if (response.IsSuccessStatusCode) return (true, "Company updated");
 try { var doc = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions); if (doc.TryGetProperty("message", out var m)) return (false, m.GetString()); } catch { }
 return (false, "Failed to update company");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling UpdateCompanyProfile API");
 return (false, ex.Message);
 }
 }

 public async Task<List<EmployeeRoleDto>?> GetEmployeeRolesAsync(string token)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Get, "api/EmployeeRoles");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);
 var response = await _httpClient.SendAsync(request);
 if (!response.IsSuccessStatusCode) return null;
 var content = await response.Content.ReadAsStringAsync();
 return JsonSerializer.Deserialize<List<EmployeeRoleDto>>(content, JsonOptions);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling GetEmployeeRoles API");
 return null;
 }
 }

 public async Task<(bool Success, string? Message)> SyncEmployeeRolesAsync(string token, List<EmployeeRoleDto> roles)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Post, "api/EmployeeRoles/sync");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);
 request.Content = new StringContent(JsonSerializer.Serialize(roles, JsonOptions), Encoding.UTF8, "application/json");
 var response = await _httpClient.SendAsync(request);
 var content = await response.Content.ReadAsStringAsync();
 if (response.IsSuccessStatusCode) return (true, null);
 try { var doc = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions); if (doc.TryGetProperty("message", out var m)) return (false, m.GetString()); } catch { }
 return (false, "Failed to sync roles");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling SyncEmployeeRoles API");
 return (false, ex.Message);
 }
 }

 public async Task<(bool Success, string? Message)> DeleteEmployeeRoleAsync(string token, int id)
 {
 try
 {
 using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/EmployeeRoles/{id}");
 var clean = NormalizeToken(token);
 if (!string.IsNullOrEmpty(clean)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", clean);
 var response = await _httpClient.SendAsync(request);
 var content = await response.Content.ReadAsStringAsync();
 if (response.IsSuccessStatusCode) return (true, null);
 try { var doc = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions); if (doc.TryGetProperty("message", out var m)) return (false, m.GetString()); } catch { }
 return (false, "Failed to delete role");
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error calling DeleteEmployeeRole API");
 return (false, ex.Message);
 }
 }
 } // <- This closing brace was missing
}
