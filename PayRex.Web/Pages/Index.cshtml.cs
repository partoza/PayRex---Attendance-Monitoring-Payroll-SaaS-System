using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace PayRex.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public List<PlanViewModel> Plans { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PayRexApi");
                var response = await client.GetAsync("api/superadmin/plans");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Plans = JsonSerializer.Deserialize<List<PlanViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load plans for landing page, using defaults");
            }

            // Fallback if API is unavailable
            if (Plans.Count == 0)
            {
                Plans = new List<PlanViewModel>
                {
                    new() { PlanId = 1, Name = "Basic", MaxEmployees = 50, Price = 2499m, Description = "For small to medium Filipino businesses", PlanUserLimit = 3 },
                    new() { PlanId = 2, Name = "Pro", MaxEmployees = 200, Price = 4999m, Description = "For growing Philippine enterprises", PlanUserLimit = 10 },
                };
            }
        }

        public class PlanViewModel
        {
            public int PlanId { get; set; }
            public string Name { get; set; } = "";
            public int MaxEmployees { get; set; }
            public decimal Price { get; set; }
            public string? Description { get; set; }
            public int? PlanUserLimit { get; set; }
        }
    }
}
