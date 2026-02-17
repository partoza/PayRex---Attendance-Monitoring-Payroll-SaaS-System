using PayRex.Web.DTOs;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PayRex.Web.Services
{
    public interface IAuthApiService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<LoginResponseDto?> VerifyTotpAsync(TotpVerificationDto request);
        Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request);
        Task<UserInfoDto?> GetCurrentUserAsync(string token);

        // Profile methods
        Task<UserProfileDto?> GetUserProfileAsync(string token);
        Task<(bool Success, string? Message)> UpdateProfileAsync(string token, UpdateProfileRequestDto request);
        Task<(bool Success, string? Message, bool RequireRelogin)> ChangePasswordAsync(string token, ChangePasswordRequestDto request);

        // Company profile
        Task<CompanyProfileDto?> GetCompanyProfileAsync(string token);
        Task<(bool Success, string? Message)> UpdateCompanyProfileAsync(string token, UpdateCompanyRequestDto dto);

        // Profile image methods
        Task<ProfileImageResponseDto?> UploadProfileImageAsync(string token, Stream imageStream, string fileName, string contentType);
        Task<ProfileImageResponseDto?> UploadCompanyLogoAsync(string token, Stream imageStream, string fileName, string contentType);
        Task<(bool Success, string? Message)> RemoveProfileImageAsync(string token);

        // 2FA methods - use client DTO types
        Task<ClientTotpSetupResponseDto?> SetupTotpAsync(string token);
        Task<ClientTotpEnableResponseDto?> EnableTotpAsync(string token, EnableTotpRequestDto request);
        Task<(bool Success, string? Message)> DisableTotpAsync(string token);
        Task<TwoFactorStatusDto?> GetTwoFactorStatusAsync(string token);

        // Employee roles (company-level)
        Task<List<EmployeeRoleDto>?> GetEmployeeRolesAsync(string token);
        Task<(bool Success, string? Message)> SyncEmployeeRolesAsync(string token, List<EmployeeRoleDto> roles);
        Task<(bool Success, string? Message)> DeleteEmployeeRoleAsync(string token, int id);
    }
}
