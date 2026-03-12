using PayRexApplication.Models;
using PayRexApplication.Enums;
using PayRexApplication.DTOs;

namespace PayRexApplication.Services
{
    /// <summary>
    /// Service for SuperAdmin operations.
    /// </summary>
    public interface ISuperAdminService
    {
        Task<DashboardKpisDto> GetDashboardKpisAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AdminNotificationDto>> GetNotificationsAsync();
        Task<bool> SetUserStatusAsync(int userId, UserStatus status, int actorUserId, string? ipAddress, string? userAgent);
        Task<bool> SetUserRoleAsync(int userId, string newRole, int adminId, string? ip, string? userAgent);
        Task<bool> ResetUserPasswordAsync(int userId, int adminId, string? ip, string? userAgent);
        Task<bool> SetCompanyStatusAsync(string companyId, bool isActive, int actorUserId, string? ipAddress, string? userAgent);
        Task<bool> SetPlanUserLimitAsync(int planId, int? limit, int actorUserId, string? ipAddress, string? userAgent);
        Task<SystemSettingDto?> GetCurrentSystemSettingsAsync();
        Task<bool> UpdateSystemSettingsAsync(UpdateSystemSettingDto dto, int actorUserId, string? ipAddress, string? userAgent);
        Task<List<PlanDto>> GetPlansAsync();
        Task<List<AdminUserDto>> GetUsersAsync();
        Task<List<AdminUserDto>> GetAllUsersAsync();
        Task<List<AdminCompanyDto>> GetCompaniesAsync();
        Task<List<AdminCompanyDto>> GetAllCompaniesAsync();
        Task<List<AdminBillingDto>> GetBillingAsync();
        Task<List<AdminBillingDto>> GetArchivedBillingAsync();
        Task<bool> ArchiveInvoiceAsync(int invoiceId);
        Task<List<RolePermissionDto>> GetPermissionsAsync();
        Task<bool> SavePermissionsAsync(List<RolePermissionDto> permissions);
        Task<AuditLogListDto> GetAuditLogsAsync(int page, int pageSize, string? search, string? action, DateTime? fromDate, DateTime? toDate, int? companyId);
        Task<AuditLogStatsDto> GetAuditLogStatsAsync(int? companyId);
        Task<List<FinanceEntryDto>> GetFinanceEntriesAsync(string? type, string? category, DateTime? fromDate = null, DateTime? toDate = null);
        Task<FinanceSummaryDto> GetFinanceSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> AddFinanceEntryAsync(string type, string description, decimal amount, string? category, string? reference, int? createdBy);
        Task<bool> UpdatePlanAsync(int planId, string name, decimal price, int maxEmployees, string? description);
        Task<List<SystemNotificationDto>> GetSystemNotificationsAsync();
        Task<bool> AddSystemNotificationAsync(string title, string message, string type, string? targetRoles, int? createdBy);
        Task<bool> ToggleSystemNotificationAsync(int notificationId, bool isActive);
        Task<bool> DeleteSystemNotificationAsync(int notificationId);
        Task<List<SystemNotificationDto>> GetActiveNotificationsForRoleAsync(string role, int? userId = null);
        Task<bool> MarkNotificationReadAsync(int userId, int notificationId);
        Task<bool> MarkAllNotificationsReadAsync(int userId, string role);
    }
}
