using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using PayRexApplication.Models;

namespace PayRexApplication.Services
{
    /// <summary>
    /// DTOs used by the SuperAdmin service and controller.
    /// </summary>
    public class DashboardKpisDto
    {
  public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
   public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalEmployees { get; set; }
      public decimal MonthlyRevenue { get; set; }
        public int TotalPlans { get; set; }
        public int PendingInvoices { get; set; }
    }

    public class AdminNotificationDto
    {
    public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
 }

    public class SystemSettingDto
    {
        public int SettingId { get; set; }
        public decimal SssPercentage { get; set; }
        public decimal PagIbigPercentage { get; set; }
        public decimal PhilHealthPercentage { get; set; }
public DateTime EffectiveDate { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateSystemSettingDto
    {
        public decimal SssPercentage { get; set; }
        public decimal PagIbigPercentage { get; set; }
        public decimal PhilHealthPercentage { get; set; }
 public DateTime EffectiveDate { get; set; }
        public string? Note { get; set; }
        // Password confirmation required to perform the update
 public string? Password { get; set; }
    }

    public class PlanDto
    {
        public int PlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxEmployees { get; set; }
        public decimal Price { get; set; }
        public string BillingCycle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
public int? PlanUserLimit { get; set; }
    }

    public class AdminUserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
   public string Status { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class AdminCompanyDto
    {
      public int CompanyId { get; set; }
      public string CompanyCode { get; set; } = string.Empty;
   public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
      public string PlanName { get; set; } = string.Empty;
        public int UserCount { get; set; }
 public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LogoUrl { get; set; }
    }

    public class AdminBillingDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RolePermissionDto
    {
        public int PermissionId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanInactivate { get; set; }
    }

    /// <summary>
    /// Service for SuperAdmin operations.
    /// </summary>
    public interface ISuperAdminService
    {
        Task<DashboardKpisDto> GetDashboardKpisAsync();
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
        Task<DTOs.AuditLogListDto> GetAuditLogsAsync(int page, int pageSize, string? search, string? action, DateTime? fromDate, DateTime? toDate, int? companyId);
        Task<DTOs.AuditLogStatsDto> GetAuditLogStatsAsync(int? companyId);
        Task<List<FinanceEntryDto>> GetFinanceEntriesAsync(string? type, string? category);
        Task<FinanceSummaryDto> GetFinanceSummaryAsync();
        Task<bool> AddFinanceEntryAsync(string type, string description, decimal amount, string? category, string? reference, int? createdBy);
        Task<bool> UpdatePlanAsync(int planId, string name, decimal price, int maxEmployees, string? description);
        Task<List<SystemNotificationDto>> GetSystemNotificationsAsync();
        Task<bool> AddSystemNotificationAsync(string title, string message, string type, string? targetRoles, int? createdBy);
        Task<bool> ToggleSystemNotificationAsync(int notificationId, bool isActive);
        Task<bool> DeleteSystemNotificationAsync(int notificationId);
        Task<List<SystemNotificationDto>> GetActiveNotificationsForRoleAsync(string role);
    }

    public class SuperAdminService : ISuperAdminService
    {
        private readonly AppDbContext _db;
      private readonly IActivityLoggerService _audit;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(AppDbContext db, IActivityLoggerService audit, ILogger<SuperAdminService> logger) // Modified
        {
  _db = db;
       _audit = audit;
       _logger = logger;
        }

      public async Task<DashboardKpisDto> GetDashboardKpisAsync()
        {
  var now = DateTime.UtcNow;
 var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return new DashboardKpisDto
       {
     TotalCompanies = await _db.Companies.CountAsync(c => c.CompanyCode != "0000"),
       ActiveCompanies = await _db.Companies.CountAsync(c => c.CompanyCode != "0000" && c.IsActive && c.Status == CompanyStatus.Active),
           TotalUsers = await _db.Users.CountAsync(u => u.Role != UserRole.SuperAdmin),
    ActiveUsers = await _db.Users.CountAsync(u => u.Role != UserRole.SuperAdmin && u.Status == UserStatus.Active),
                TotalEmployees = await _db.Employees.CountAsync(),
          MonthlyRevenue = await _db.Payments
      .Where(p => p.PaidAt >= startOfMonth && p.Status == PaymentStatus.Success && p.Amount.HasValue)
  .SumAsync(p => p.Amount!.Value),
      TotalPlans = await _db.SubscriptionPlans.CountAsync(p => p.Status == PlanStatus.Active && p.Name != "Enterprise"),
     PendingInvoices = await _db.BillingInvoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid)
};
    }

    public async Task<List<AdminNotificationDto>> GetNotificationsAsync()
        {
            var notifications = new List<AdminNotificationDto>();

  var recentCompanies = await _db.Companies
         .AsNoTracking()
         .Where(c => c.CompanyCode != "0000")
      .OrderByDescending(c => c.CreatedAt)
      .Take(5)
    .Select(c => new { c.CompanyName, c.CreatedAt })
         .ToListAsync();

            foreach (var c in recentCompanies)
    {
           notifications.Add(new AdminNotificationDto
      {
            Type = "NewCompany",
         Message = $"New company registered: {c.CompanyName}",
        Timestamp = c.CreatedAt
                });
    }

    var pendingCount = await _db.BillingInvoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid);
          if (pendingCount > 0)
     {
     notifications.Add(new AdminNotificationDto
                {
 Type = "PendingInvoices",
        Message = $"{pendingCount} pending invoices require attention",
    Timestamp = DateTime.UtcNow
 });
            }

       return notifications.OrderByDescending(n => n.Timestamp).ToList();
     }

        public async Task<bool> SetUserStatusAsync(int userId, UserStatus status, int actorUserId, string? ipAddress, string? userAgent)
 {
            var user = await _db.Users.FindAsync(userId);
         if (user == null || user.Role == UserRole.SuperAdmin) return false;

            var oldStatus = user.Status.ToString();
            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;
     await _db.SaveChangesAsync();

    await _audit.LogAsync(actorUserId, user.CompanyId, "SetUserStatus", "User", userId.ToString(),
    oldStatus, status.ToString(), ipAddress, userAgent);

       _logger.LogInformation("User {UserId} status changed to {Status} by {ActorId}", userId, status, actorUserId);
  return true;
        }

        public async Task<bool> SetUserRoleAsync(int userId, string newRole, int adminId, string? ip, string? userAgent)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.Role == UserRole.SuperAdmin) return false;
            
            if (!Enum.TryParse<UserRole>(newRole, true, out var parsedRole)) return false;

            user.Role = parsedRole;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, user.CompanyId, "SetUserRole", "User", userId.ToString(),
                null, newRole, ip, userAgent);

            _logger.LogInformation("User {UserId} role changed to {Role} by {AdminId}", userId, newRole, adminId);

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(int userId, int adminId, string? ip, string? userAgent)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.Role == UserRole.SuperAdmin) return false;

            var newPassword = $"PayRex_{DateTime.Now.Year}!";
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Flag user so they must change it on login
            user.MustChangePassword = true;

            await _db.SaveChangesAsync();

            await _audit.LogAsync(adminId, user.CompanyId, "ResetPassword", "User", userId.ToString(),
                null, "Reset", ip, userAgent);

            _logger.LogInformation("Password reset for {UserId} by {AdminId}", userId, adminId);

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetCompanyStatusAsync(string companyId, bool isActive, int actorUserId, string? ipAddress, string? userAgent)
        {
          // Support either numeric companyId or companyCode strings. Find company accordingly.
          Company? company = null;
          if (int.TryParse(companyId, out var cid))
          {
            company = await _db.Companies.Include(c => c.Users).FirstOrDefaultAsync(c => c.CompanyId == cid);
          }
          else
          {
            company = await _db.Companies.Include(c => c.Users).FirstOrDefaultAsync(c => c.CompanyCode == companyId);
          }

       if (company == null) return false;
       if (company.CompanyCode == "0000") return false; // Can't deactivate system company

            var oldIsActive = company.IsActive;
            company.IsActive = isActive;
 company.Status = isActive ? CompanyStatus.Active : CompanyStatus.Suspended;
     company.UpdatedAt = DateTime.UtcNow;

            // Cascade: set all users to Active/Suspended accordingly
            foreach (var user in company.Users)
   {
         if (user.Role != UserRole.SuperAdmin)
  {
      user.Status = isActive ? UserStatus.Active : UserStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;
    }
  }

     await _db.SaveChangesAsync();

    await _audit.LogAsync(actorUserId, company.CompanyId, "SetCompanyStatus", "Company", company.CompanyId.ToString(),
  oldIsActive.ToString(), isActive.ToString(), ipAddress, userAgent);

   _logger.LogInformation("Company {CompanyId} IsActive set to {IsActive} by {ActorId}. {Count} users cascaded.",
      companyId, isActive, actorUserId, company.Users.Count);
  return true;
        }

        public async Task<bool> SetPlanUserLimitAsync(int planId, int? limit, int actorUserId, string? ipAddress, string? userAgent)
        {
      var plan = await _db.SubscriptionPlans.FindAsync(planId);
       if (plan == null) return false;

            var oldLimit = plan.PlanUserLimit?.ToString() ?? "unlimited";
     plan.PlanUserLimit = limit;
        plan.UpdatedAt = DateTime.UtcNow;
          await _db.SaveChangesAsync();

   await _audit.LogAsync(actorUserId, null, "SetPlanUserLimit", "SubscriptionPlan", planId.ToString(),
   oldLimit, limit?.ToString() ?? "unlimited", ipAddress, userAgent);

            return true;
        }

      public async Task<SystemSettingDto?> GetCurrentSystemSettingsAsync()
  {
            var setting = await _db.SystemSettings.OrderByDescending(s => s.EffectiveDate).FirstOrDefaultAsync();
     if (setting == null) return null;

  return new SystemSettingDto
            {
    SettingId = setting.SettingId,
      SssPercentage = setting.SssPercentage,
  PagIbigPercentage = setting.PagIbigPercentage,
     PhilHealthPercentage = setting.PhilHealthPercentage,
   EffectiveDate = setting.EffectiveDate,
         Note = setting.Note
      };
        }

        public async Task<bool> UpdateSystemSettingsAsync(UpdateSystemSettingDto dto, int actorUserId, string? ipAddress, string? userAgent)
        {
 // Verify actor user's password before applying changes
 var actor = await _db.Users.FindAsync(actorUserId);
 if (actor == null)
 {
 _logger.LogWarning("Attempt to update system settings by non-existent user {ActorId}", actorUserId);
 return false;
 }

 if (string.IsNullOrEmpty(dto.Password))
 {
 _logger.LogWarning("Password not provided for system settings update by user {ActorId}", actorUserId);
 return false;
 }

 // Verify password using BCrypt (seeds use BCrypt)
 try
 {
 if (!BCrypt.Net.BCrypt.Verify(dto.Password, actor.PasswordHash))
 {
 _logger.LogWarning("Invalid password provided for system settings update by user {ActorId}", actorUserId);
 return false;
 }
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error verifying password for user {ActorId}", actorUserId);
 return false;
 }

 var existing = await _db.SystemSettings.OrderByDescending(s => s.EffectiveDate).FirstOrDefaultAsync();
 var oldValues = existing != null
 ? $"SSS={existing.SssPercentage},PagIbig={existing.PagIbigPercentage},PhilHealth={existing.PhilHealthPercentage}"
 : "none";

 // Create new setting record (append, don't overwrite, for history)
 var setting = new SystemSetting
 {
 SssPercentage = dto.SssPercentage,
 PagIbigPercentage = dto.PagIbigPercentage,
 PhilHealthPercentage = dto.PhilHealthPercentage,
 EffectiveDate = dto.EffectiveDate,
 Note = dto.Note,
 CreatedAt = DateTime.UtcNow
 };

 _db.SystemSettings.Add(setting);
 await _db.SaveChangesAsync();

 var newValues = $"SSS={dto.SssPercentage},PagIbig={dto.PagIbigPercentage},PhilHealth={dto.PhilHealthPercentage}";
 await _audit.LogAsync(actorUserId, null, "UpdateSystemSettings", "SystemSetting", setting.SettingId.ToString(),
 oldValues, newValues, ipAddress, userAgent);

 _logger.LogInformation("System settings updated by user {ActorId}: {NewValues}", actorUserId, newValues);

 return true;
        }

        public async Task<List<PlanDto>> GetPlansAsync()
        {
            return await _db.SubscriptionPlans
    .AsNoTracking()
    .Where(p => p.Name != "Enterprise")
    .OrderBy(p => p.PlanId)
                .Select(p => new PlanDto
    {
        PlanId = p.PlanId,
       Name = p.Name,
        MaxEmployees = p.MaxEmployees,
      Price = p.Price,
           BillingCycle = p.BillingCycle.ToString(),
           Status = p.Status.ToString(),
      Description = p.Description,
        PlanUserLimit = p.PlanUserLimit
     })
    .ToListAsync();
        }

 public async Task<List<AdminUserDto>> GetUsersAsync()
     {
            return await _db.Users
            .AsNoTracking()
            .Include(u => u.Company)
       .Where(u => u.Role != UserRole.SuperAdmin && u.Status == UserStatus.Active)
                .OrderByDescending(u => u.CreatedAt)
          .Select(u => new AdminUserDto
        {
       UserId = u.UserId,
  FirstName = u.FirstName,
 LastName = u.LastName,
           Email = u.Email,
        Role = u.Role.ToString(),
            Status = u.Status.ToString(),
CompanyId = u.CompanyId,
             CompanyName = u.Company.CompanyName,
        CreatedAt = u.CreatedAt,
                ProfileImageUrl = u.ProfileImageUrl
            })
        .ToListAsync();
   }

        public async Task<List<AdminCompanyDto>> GetCompaniesAsync()
  {
            return await _db.Companies
          .AsNoTracking()
          .Where(c => c.CompanyCode != "0000" && c.IsActive)
         .Include(c => c.SubscriptionPlan)
     .OrderByDescending(c => c.CreatedAt)
      .Select(c => new AdminCompanyDto
    {
                CompanyId = c.CompanyId,
                CompanyCode = c.CompanyCode,
    CompanyName = c.CompanyName,
          Status = c.Status.ToString(),
         IsActive = c.IsActive,
   PlanName = c.SubscriptionPlan.Name,
        UserCount = c.Users.Count,
        EmployeeCount = c.Employees.Count,
         CreatedAt = c.CreatedAt,
                LogoUrl = c.LogoUrl
           })
    .ToListAsync();
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Include(u => u.Company)
                .Where(u => u.Role != UserRole.SuperAdmin)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    Status = u.Status.ToString(),
                    CompanyId = u.CompanyId,
                    CompanyName = u.Company.CompanyName,
                    CreatedAt = u.CreatedAt,
                    ProfileImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();
        }

        public async Task<List<AdminCompanyDto>> GetAllCompaniesAsync()
        {
            return await _db.Companies
                .AsNoTracking()
                .Where(c => c.CompanyCode != "0000")
                .Include(c => c.SubscriptionPlan)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AdminCompanyDto
                {
                    CompanyId = c.CompanyId,
                    CompanyCode = c.CompanyCode,
                    CompanyName = c.CompanyName,
                    Status = c.Status.ToString(),
                    IsActive = c.IsActive,
                    PlanName = c.SubscriptionPlan.Name,
                    UserCount = c.Users.Count,
                    EmployeeCount = c.Employees.Count,
                    CreatedAt = c.CreatedAt,
                    LogoUrl = c.LogoUrl
                })
                .ToListAsync();
        }

        public async Task<List<AdminBillingDto>> GetBillingAsync()
        {
            return await _db.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Company)
                .Where(i => i.Status != InvoiceStatus.Archived)
                .OrderByDescending(i => i.CreatedAt)
                .Take(100)
                .Select(i => new AdminBillingDto
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber ?? "",
                    CompanyName = i.Company.CompanyName,
                    Amount = i.Amount,
                    VatAmount = i.VatAmount,
                    Status = i.Status.ToString(),
                    DueDate = i.DueDate,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<AdminBillingDto>> GetArchivedBillingAsync()
        {
            return await _db.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Company)
                .Where(i => i.Status == InvoiceStatus.Archived)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new AdminBillingDto
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber ?? "",
                    CompanyName = i.Company.CompanyName,
                    Amount = i.Amount,
                    VatAmount = i.VatAmount,
                    Status = i.Status.ToString(),
                    DueDate = i.DueDate,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ArchiveInvoiceAsync(int invoiceId)
        {
            var invoice = await _db.BillingInvoices.FindAsync(invoiceId);
            if (invoice == null) return false;
            invoice.Status = InvoiceStatus.Archived;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<RolePermissionDto>> GetPermissionsAsync()
        {
            return await _db.RolePermissions
                .AsNoTracking()
                .OrderBy(p => p.RoleName).ThenBy(p => p.ModuleName)
                .Select(p => new RolePermissionDto
                {
                    PermissionId = p.PermissionId,
                    RoleName = p.RoleName,
                    ModuleName = p.ModuleName,
                    CanAdd = p.CanAdd,
                    CanUpdate = p.CanUpdate,
                    CanInactivate = p.CanInactivate
                })
                .ToListAsync();
        }

        public async Task<bool> SavePermissionsAsync(List<RolePermissionDto> permissions)
        {
            foreach (var dto in permissions)
            {
                var existing = await _db.RolePermissions.FindAsync(dto.PermissionId);
                if (existing != null)
                {
                    existing.CanAdd = dto.CanAdd;
                    existing.CanUpdate = dto.CanUpdate;
                    existing.CanInactivate = dto.CanInactivate;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.RolePermissions.Add(new RolePermission
                    {
                        RoleName = dto.RoleName,
                        ModuleName = dto.ModuleName,
                        CanAdd = dto.CanAdd,
                        CanUpdate = dto.CanUpdate,
                        CanInactivate = dto.CanInactivate,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<DTOs.AuditLogListDto> GetAuditLogsAsync(int page, int pageSize, string? search, string? action, DateTime? fromDate, DateTime? toDate, int? companyId)
        {
            // Query AuditLogs
            var auditQuery = _db.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Company)
                .AsQueryable();

            if (companyId.HasValue)
                auditQuery = auditQuery.Where(a => a.CompanyId == companyId.Value);

            if (!string.IsNullOrEmpty(action))
                auditQuery = auditQuery.Where(a => a.Action.Contains(action));

            if (fromDate.HasValue)
                auditQuery = auditQuery.Where(a => a.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                auditQuery = auditQuery.Where(a => a.CreatedAt <= toDate.Value);

            // Query UserLoginAttempts (merge as audit items)
            var loginQuery = _db.UserLoginAttempts.AsNoTracking().Include(l => l.User).AsQueryable();

            // Scope login attempts to the same company when a companyId filter is active
            if (companyId.HasValue)
                loginQuery = loginQuery.Where(l => l.User != null && l.User.CompanyId == companyId.Value);

            if (fromDate.HasValue)
                loginQuery = loginQuery.Where(l => l.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                loginQuery = loginQuery.Where(l => l.Timestamp <= toDate.Value);

            // Map audit logs to DTOs
            var auditItems = await auditQuery
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new DTOs.AuditLogItemDto
                {
                    Id = a.AuditId,
                    Timestamp = a.CreatedAt,
                    Action = a.Action,
                    UserEmail = a.User != null ? a.User.Email : null,
                    UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : null,
                    IpAddress = a.IpAddress,
                    Details = (a.EntityAffected ?? a.Target ?? "") + (a.NewValues != null ? " → " + a.NewValues : ""),
                    Role = a.Role ?? (a.User != null ? a.User.Role.ToString() : null),
                    CompanyId = a.CompanyId,
                    CompanyName = a.Company != null ? a.Company.CompanyName : null
                })
                .ToListAsync();

            // Map login attempts
            var shouldIncludeLoginActions = string.IsNullOrEmpty(action) ||
                action == "Login" || action == "Failed Login";

            if (shouldIncludeLoginActions)
            {
                var loginItems = await loginQuery
                    .OrderByDescending(l => l.Timestamp)
                    .Select(l => new DTOs.AuditLogItemDto
                    {
                        Id = l.AttemptId + 1000000, // offset to avoid ID collision
                        Timestamp = l.Timestamp,
                        Action = l.Success ? "Login" : "Failed Login",
                        UserEmail = l.Email,
                        UserName = l.User != null ? l.User.FirstName + " " + l.User.LastName : l.Email,
                        IpAddress = l.IpAddress,
                        Details = l.Success ? "Successful login" : (l.Reason ?? "Invalid credentials"),
                        Role = l.User != null ? l.User.Role.ToString() : null,
                        CompanyId = l.User != null ? (int?)l.User.CompanyId : null,
                        CompanyName = null
                    })
                    .ToListAsync();

                // Filter login items by action type if specified
                if (!string.IsNullOrEmpty(action))
                {
                    loginItems = loginItems.Where(l => l.Action == action).ToList();
                }

                auditItems.AddRange(loginItems);
            }

            // Apply text search across all items
            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLowerInvariant();
                auditItems = auditItems.Where(a =>
                    (a.UserEmail != null && a.UserEmail.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (a.IpAddress != null && a.IpAddress.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Details != null && a.Details.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (a.UserName != null && a.UserName.Contains(s, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Sort and paginate
            var totalCount = auditItems.Count;
            var items = auditItems
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Stats
            var stats = new DTOs.AuditLogStatsDto
            {
                FailedLogins = auditItems.Count(a => a.Action == "Failed Login"),
                Registrations = auditItems.Count(a => a.Action.Contains("Register") || a.Action.Contains("Registration")),
                PasswordResets = auditItems.Count(a => a.Action.Contains("Password") || a.Action.Contains("ResetPassword")),
                SuccessfulLogins = auditItems.Count(a => a.Action == "Login")
            };

            return new DTOs.AuditLogListDto
            {
                Items = items,
                TotalCount = totalCount,
                Stats = stats
            };
        }

        public async Task<DTOs.AuditLogStatsDto> GetAuditLogStatsAsync(int? companyId)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var failedLogins = await _db.UserLoginAttempts
                .CountAsync(l => !l.Success && l.Timestamp >= thirtyDaysAgo);

            var successLogins = await _db.UserLoginAttempts
                .CountAsync(l => l.Success && l.Timestamp >= thirtyDaysAgo);

            var registrations = await _db.AuditLogs
                .CountAsync(a => a.Action.Contains("Register") && a.CreatedAt >= thirtyDaysAgo);

            var passwordResets = await _db.AuditLogs
                .CountAsync(a => (a.Action.Contains("Password") || a.Action.Contains("ResetPassword"))
                    && a.CreatedAt >= thirtyDaysAgo);

            return new DTOs.AuditLogStatsDto
            {
                FailedLogins = failedLogins,
                SuccessfulLogins = successLogins,
                Registrations = registrations,
                PasswordResets = passwordResets
            };
        }

        // ===== Finance Methods =====

        public async Task<List<FinanceEntryDto>> GetFinanceEntriesAsync(string? type, string? category)
        {
            var query = _db.FinanceEntries.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(f => f.Type == type);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(f => f.Category == category);

            return await query.OrderByDescending(f => f.CreatedAt)
                .Select(f => new FinanceEntryDto
                {
                    EntryId = f.EntryId,
                    Type = f.Type,
                    Description = f.Description,
                    Amount = f.Amount,
                    VatAmount = f.VatAmount,
                    Category = f.Category,
                    Reference = f.Reference,
                    IsAutoGenerated = f.IsAutoGenerated,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FinanceSummaryDto> GetFinanceSummaryAsync()
        {
            var entries = await _db.FinanceEntries.AsNoTracking().ToListAsync();
            var totalIncome = entries.Where(e => e.Type == "Income").Sum(e => e.Amount);
            var totalDeductions = entries.Where(e => e.Type == "Deduction").Sum(e => e.Amount);
            var totalVat = entries.Sum(e => e.VatAmount);

            return new FinanceSummaryDto
            {
                TotalIncome = totalIncome,
                TotalDeductions = totalDeductions,
                TotalVat = totalVat,
                NetProfit = totalIncome - totalDeductions,
                EntryCount = entries.Count
            };
        }

        public async Task<bool> AddFinanceEntryAsync(string type, string description, decimal amount, string? category, string? reference, int? createdBy)
        {
            var vatAmount = type == "Income" ? amount * 0.12m : 0m;
            _db.FinanceEntries.Add(new Models.FinanceEntry
            {
                Type = type,
                Description = description,
                Amount = amount,
                VatAmount = vatAmount,
                Category = category,
                Reference = reference,
                IsAutoGenerated = false,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePlanAsync(int planId, string name, decimal price, int maxEmployees, string? description)
        {
            var plan = await _db.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return false;
            plan.Name = name;
            plan.Price = price;
            plan.MaxEmployees = maxEmployees;
            plan.Description = description;
            plan.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<SystemNotificationDto>> GetSystemNotificationsAsync()
        {
            return await _db.SystemNotifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new SystemNotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    TargetRoles = n.TargetRoles,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();
        }

        public async Task<bool> AddSystemNotificationAsync(string title, string message, string type, string? targetRoles, int? createdBy)
        {
            _db.SystemNotifications.Add(new Models.SystemNotification
            {
                Title = title,
                Message = message,
                Type = type,
                TargetRoles = targetRoles,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSystemNotificationAsync(int notificationId, bool isActive)
        {
            var notif = await _db.SystemNotifications.FindAsync(notificationId);
            if (notif == null) return false;
            notif.IsActive = isActive;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSystemNotificationAsync(int notificationId)
        {
            var notif = await _db.SystemNotifications.FindAsync(notificationId);
            if (notif == null) return false;
            _db.SystemNotifications.Remove(notif);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<SystemNotificationDto>> GetActiveNotificationsForRoleAsync(string role)
        {
            return await _db.SystemNotifications
                .AsNoTracking()
                .Where(n => n.IsActive)
                .Where(n => n.TargetRoles == null || n.TargetRoles == "" || n.TargetRoles.Contains(role))
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new SystemNotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    TargetRoles = n.TargetRoles,
                    IsActive = n.IsActive,
                    CreatedAt = n.CreatedAt
                }).ToListAsync();
        }
    }

    // Finance DTOs
    public class FinanceEntryDto
    {
        public int EntryId { get; set; }
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public string? Category { get; set; }
        public string? Reference { get; set; }
        public bool IsAutoGenerated { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FinanceSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalVat { get; set; }
        public decimal NetProfit { get; set; }
        public int EntryCount { get; set; }
    }

    public class SystemNotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "info";
        public string? TargetRoles { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
