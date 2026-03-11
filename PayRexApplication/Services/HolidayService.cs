using System.Text.Json;

namespace PayRexApplication.Services
{
    /// <summary>
    /// Service to fetch Philippine public holidays from Nager.Date API.
    /// Caches results in memory for 24 hours.
    /// </summary>
    public class HolidayService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HolidayService> _logger;
        private readonly Dictionary<int, (DateTime FetchedAt, List<Models.PhilippineHoliday> Holidays)> _cache = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public HolidayService(HttpClient httpClient, ILogger<HolidayService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Get Philippine public holidays for a given year. Results are cached for 24 hours.
        /// </summary>
        public async Task<List<Models.PhilippineHoliday>> GetHolidaysAsync(int year)
        {
            // Check cache first
            if (_cache.TryGetValue(year, out var cached) && DateTime.UtcNow - cached.FetchedAt < CacheDuration)
            {
                return cached.Holidays;
            }

            try
            {
                var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/PH";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Nager.Date API returned {Status} for year {Year}", response.StatusCode, year);
                    return _cache.TryGetValue(year, out var stale) ? stale.Holidays : new List<Models.PhilippineHoliday>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var holidays = JsonSerializer.Deserialize<List<Models.PhilippineHoliday>>(json, JsonOptions) ?? new List<Models.PhilippineHoliday>();

                _cache[year] = (DateTime.UtcNow, holidays);
                _logger.LogInformation("Fetched {Count} Philippine holidays for year {Year}", holidays.Count, year);
                return holidays;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Philippine holidays for year {Year}", year);
                return _cache.TryGetValue(year, out var stale) ? stale.Holidays : new List<Models.PhilippineHoliday>();
            }
        }

        /// <summary>
        /// Check if a specific date is a Philippine holiday
        /// </summary>
        public async Task<Models.PhilippineHoliday?> GetHolidayForDateAsync(DateTime date)
        {
            var holidays = await GetHolidaysAsync(date.Year);
            return holidays.FirstOrDefault(h => h.Date.Date == date.Date);
        }

        /// <summary>
        /// Check if a specific date is a Philippine holiday (synchronous check from cache only)
        /// </summary>
        public bool IsHolidayCached(DateTime date)
        {
            if (_cache.TryGetValue(date.Year, out var cached))
            {
                return cached.Holidays.Any(h => h.Date.Date == date.Date);
            }
            return false;
        }
    }
}
