using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using System.Text.Json;

namespace PayRexApplication.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<HealthController> _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public HealthController(IDbContextFactory<AppDbContext> dbFactory, ILogger<HealthController> logger, IConfiguration config, IWebHostEnvironment env)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _config = config;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var response = new HealthResponse();
            string? dbError = null;

            // Attempt to read the connection string for diagnostic purposes
            string? rawCs = null;
            try
            {
                try
                {
                    // Prefer the value directly from the appsettings.json file in the content root so the health endpoint reflects that file
                    rawCs = ReadConnectionStringFromAppSettingsFile();

                    // If not found in the file, fall back to configuration (which may include env/app overrides)
                    if (string.IsNullOrEmpty(rawCs))
                    {
                        rawCs = _config.GetConnectionString("DefaultConnection");
                    }

                    // If still missing, fall back to the actual DbConnection string by creating a context (do not fail controller construction)
                    if (string.IsNullOrEmpty(rawCs) && _dbFactory != null)
                    {
                        try
                        {
                            using var dbCtx = _dbFactory.CreateDbContext();
                            rawCs = dbCtx.Database.GetDbConnection()?.ConnectionString;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to create DbContext to read connection string.");
                        }
                    }
                }
                catch
                {
                    // fall back to configuration key if DbConnection isn't available
                    rawCs = rawCs ?? _config.GetConnectionString("DefaultConnection");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to read connection string for health endpoint.");
            }

            // Determine whether to expose the full connection string
            bool exposeFull = _config.GetValue<bool>("Health:ExposeConnectionString", false);
            string? secretConfig = _config.GetValue<string>("Health:Secret");
            bool hasValidSecret = false;
            if (exposeFull && !string.IsNullOrEmpty(secretConfig))
            {
                if (Request.Headers.TryGetValue("X-Health-Secret", out var headerVal))
                {
                    hasValidSecret = headerVal == secretConfig;
                }
            }

            if (exposeFull && hasValidSecret)
            {
                // Return the raw connection string (careful: only when explicitly enabled and authorized)
                response.ConnectionString = rawCs ?? "<missing>";
            }
            else
            {
                // Default: masked connection string
                response.ConnectionString = MaskConnectionString(rawCs);
            }

            bool dbConnected = false;

            if (_dbFactory != null)
            {
                try
                {
                    using var db = _dbFactory.CreateDbContext();
                    dbConnected = await db.Database.CanConnectAsync();

                    if (dbConnected)
                    {
                        try
                        {
                            var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
                            response.PendingMigrations = pending ?? Array.Empty<string>();
                            response.PendingMigrationsCount = response.PendingMigrations.Length;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to get pending migrations.");
                            response.PendingMigrationsCount = -1;
                            response.PendingMigrations = Array.Empty<string>();
                        }

                        try
                        {
                            response.HasUsers = await db.Users.AnyAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to query Users table.");
                            response.HasUsers = false;
                        }

                        try
                        {
                            response.HasUserLoginAttempts = await db.UserLoginAttempts.AnyAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to query UserLoginAttempts table.");
                            response.HasUserLoginAttempts = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DB connect check failed");
                    dbError = ex.Message;
                    dbConnected = false;
                }
            }
            else
            {
                _logger.LogWarning("No IDbContextFactory<AppDbContext> available in DI. DB checks skipped.");
            }

            if (!dbConnected)
            {
                // DB not reachable - set defaults
                response.DbConnected = false;
                response.PendingMigrationsCount = -1;
                response.PendingMigrations = Array.Empty<string>();
                response.HasUsers = false;
                response.HasUserLoginAttempts = false;
            }
            else
            {
                response.DbConnected = true;
            }

            response.DbError = dbError;

            return Ok(response);
        }

        private string? ReadConnectionStringFromAppSettingsFile()
        {
            try
            {
                var file = Path.Combine(_env.ContentRootPath, "appsettings.json");
                if (!System.IO.File.Exists(file)) return null;
                using var fs = System.IO.File.OpenRead(file);
                using var doc = JsonDocument.Parse(fs);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var csElem))
                {
                    if (csElem.TryGetProperty("DefaultConnection", out var val))
                    {
                        return val.GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to read appsettings.json directly.");
            }
            return null;
        }

        private static string MaskConnectionString(string? cs)
        {
            if (string.IsNullOrEmpty(cs)) return "<missing>";
            try
            {
                var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    var idx = parts[i].IndexOf('=');
                    if (idx <= 0) continue;
                    var key = parts[i].Substring(0, idx).Trim();
                    if (key.Equals("password", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("pwd", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("user id", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("uid", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[i] = key + "=*****";
                    }
                }
                return string.Join(';', parts) + (cs.EndsWith(";") ? ";" : string.Empty);
            }
            catch
            {
                return "<redacted>";
            }
        }

        private class HealthResponse
        {
            public bool DbConnected { get; set; }
            public string? DbError { get; set; }
            public int PendingMigrationsCount { get; set; }
            public string[] PendingMigrations { get; set; } = Array.Empty<string>();
            public bool HasUsers { get; set; }
            public bool HasUserLoginAttempts { get; set; }
            // Connection string (masked by default). Full value is returned only when enabled and authorized.
            public string? ConnectionString { get; set; }
        }
    }
}
