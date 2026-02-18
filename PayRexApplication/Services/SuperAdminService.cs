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
    }

    public class AdminCompanyDto
    {
      public int CompanyId { get; set; }
   public string CompanyName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
      public string PlanName { get; set; } = string.Empty;
        public int UserCount { get; set; }
 public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminBillingDto
    {
        public int InvoiceId { get; set; }
 public string InvoiceNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
  public DateTime DueDate { get; set; }
      public DateTime CreatedAt { get; set; }
 }

    /// <summary>
    /// Service for SuperAdmin operations.
    /// </summary>
    public interface ISuperAdminService
    {
        Task<DashboardKpisDto> GetDashboardKpisAsync();
        Task<List<AdminNotificationDto>> GetNotificationsAsync();
        Task<bool> SetUserStatusAsync(int userId, UserStatus status, int actorUserId, string? ipAddress, string? userAgent);
        Task<bool> SetCompanyStatusAsync(string companyId, bool isActive, int actorUserId, string? ipAddress, string? userAgent);
        Task<bool> SetPlanUserLimitAsync(int planId, int? limit, int actorUserId, string? ipAddress, string? userAgent);
        Task<SystemSettingDto?> GetCurrentSystemSettingsAsync();
 Task<bool> UpdateSystemSettingsAsync(UpdateSystemSettingDto dto, int actorUserId, string? ipAddress, string? userAgent);
    Task<List<PlanDto>> GetPlansAsync();
  Task<List<AdminUserDto>> GetUsersAsync();
 Task<List<AdminCompanyDto>> GetCompaniesAsync();
      Task<List<AdminBillingDto>> GetBillingAsync();
 }

    public class SuperAdminService : ISuperAdminService
    {
        private readonly AppDbContext _db;
      private readonly IActivityLoggerService _audit;
        private readonly ILogger<SuperAdminService> _logger;

        public SuperAdminService(AppDbContext db, IActivityLoggerService audit, ILogger<SuperAdminService> logger)
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
      TotalPlans = await _db.SubscriptionPlans.CountAsync(p => p.Status == PlanStatus.Active),
     PendingInvoices = await _db.BillingInvoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid)
};
    }

    public async Task<List<AdminNotificationDto>> GetNotificationsAsync()
        {
            var notifications = new List<AdminNotificationDto>();

  var recentCompanies = await _db.Companies
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
        CreatedAt = u.CreatedAt
            })
        .ToListAsync();
   }

        public async Task<List<AdminCompanyDto>> GetCompaniesAsync()
  {
            return await _db.Companies
          .Where(c => c.CompanyCode != "0000")
         .Include(c => c.SubscriptionPlan)
     .OrderByDescending(c => c.CreatedAt)
      .Select(c => new AdminCompanyDto
    {
                CompanyId = c.CompanyId,
    CompanyName = c.CompanyName,
          Status = c.Status.ToString(),
         IsActive = c.IsActive,
   PlanName = c.SubscriptionPlan.Name,
        UserCount = c.Users.Count,
        EmployeeCount = c.Employees.Count,
         CreatedAt = c.CreatedAt
           })
    .ToListAsync();
        }

        public async Task<List<AdminBillingDto>> GetBillingAsync()
     {
        return await _db.BillingInvoices
     .Include(i => i.Company)
         .OrderByDescending(i => i.CreatedAt)
  .Take(100)
      .Select(i => new AdminBillingDto
      {
       InvoiceId = i.InvoiceId,
           InvoiceNumber = i.InvoiceNumber ?? "",
      CompanyName = i.Company.CompanyName,
         Amount = i.Amount,
          Status = i.Status.ToString(),
   DueDate = i.DueDate,
   CreatedAt = i.CreatedAt
      })
 .ToListAsync();
        }
    }
}
