using PayRexApplication.Data;
using PayRexApplication.Models;

namespace PayRexApplication.Services
{
    /// <summary>
    /// Service for creating audit log entries.
    /// </summary>
  public interface IActivityLoggerService
    {
 Task LogAsync(int? userId, string? companyId, string action, string? entityType = null,
       string? entityId = null, string? oldValue = null, string? newValue = null,
   string? ipAddress = null, string? userAgent = null,
        string? role = null, string? entityAffected = null);
    }

    public class ActivityLoggerService : IActivityLoggerService
    {
        private readonly AppDbContext _db;
 private readonly ILogger<ActivityLoggerService> _logger;

   public ActivityLoggerService(AppDbContext db, ILogger<ActivityLoggerService> logger)
    {
         _db = db;
      _logger = logger;
        }

 public async Task LogAsync(int? userId, string? companyId, string action, string? entityType = null,
            string? entityId = null, string? oldValue = null, string? newValue = null,
      string? ipAddress = null, string? userAgent = null,
         string? role = null, string? entityAffected = null)
        {
    try
    {
    var log = new AuditLog
{
  UserId = userId,
      CompanyId = companyId,
        Role = role,
     Action = action,
     EntityAffected = entityAffected,
        Target = entityType,
     TargetId = entityId,
OldValues = oldValue,
         NewValues = newValue,
      IpAddress = ipAddress?.Length > 50 ? ipAddress[..50] : ipAddress,
           UserAgent = userAgent?.Length > 500 ? userAgent[..500] : userAgent,
       CreatedAt = DateTime.UtcNow
       };

       _db.AuditLogs.Add(log);
 await _db.SaveChangesAsync();
            }
  catch (Exception ex)
 {
      _logger.LogError(ex, "Failed to write audit log: {Action} for user {UserId}", action, userId);
    }
  }
  }
}
