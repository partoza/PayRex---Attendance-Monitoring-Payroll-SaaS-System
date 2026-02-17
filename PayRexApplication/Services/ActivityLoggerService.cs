namespace PayRexApplication.Services
{
    using PayRexApplication.Data;
    using PayRexApplication.Models;

    /// <summary>
    /// Service for creating audit log entries
    /// </summary>
    public interface IActivityLoggerService
    {
        /// <summary>
        /// The LogAsync
        /// </summary>
        /// <param name="userId">The userId<see cref="int?"/></param>
        /// <param name="companyId">The companyId<see cref="string?"/></param>
        /// <param name="action">The action<see cref="string"/></param>
        /// <param name="entityType">The entityType<see cref="string?"/></param>
        /// <param name="entityId">The entityId<see cref="string?"/></param>
        /// <param name="oldValue">The oldValue<see cref="string?"/></param>
        /// <param name="newValue">The newValue<see cref="string?"/></param>
        /// <param name="ipAddress">The ipAddress<see cref="string?"/></param>
        /// <param name="userAgent">The userAgent<see cref="string?"/></param>
        /// <param name="role">The role<see cref="string?"/></param>
        /// <param name="entityAffected">The entityAffected<see cref="string?"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task LogAsync(int? userId, string? companyId, string action, string? entityType = null,
       string? entityId = null, string? oldValue = null, string? newValue = null,
   string? ipAddress = null, string? userAgent = null,
        string? role = null, string? entityAffected = null);
    }

    /// <summary>
    /// Defines the <see cref="ActivityLoggerService" />
    /// </summary>
    public class ActivityLoggerService : IActivityLoggerService
    {
        /// <summary>
        /// Defines the _db
        /// </summary>
        private readonly AppDbContext _db;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<ActivityLoggerService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLoggerService"/> class.
        /// </summary>
        /// <param name="db">The db<see cref="AppDbContext"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ActivityLoggerService}"/></param>
        public ActivityLoggerService(AppDbContext db, ILogger<ActivityLoggerService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// The LogAsync
        /// </summary>
        /// <param name="userId">The userId<see cref="int?"/></param>
        /// <param name="companyId">The companyId<see cref="string?"/></param>
        /// <param name="action">The action<see cref="string"/></param>
        /// <param name="entityType">The entityType<see cref="string?"/></param>
        /// <param name="entityId">The entityId<see cref="string?"/></param>
        /// <param name="oldValue">The oldValue<see cref="string?"/></param>
        /// <param name="newValue">The newValue<see cref="string?"/></param>
        /// <param name="ipAddress">The ipAddress<see cref="string?"/></param>
        /// <param name="userAgent">The userAgent<see cref="string?"/></param>
        /// <param name="role">The role<see cref="string?"/></param>
        /// <param name="entityAffected">The entityAffected<see cref="string?"/></param>
        /// <returns>The <see cref="Task"/></returns>
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
