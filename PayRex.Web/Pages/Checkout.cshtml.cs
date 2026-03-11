using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PayRex.Web.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CheckoutModel> _logger;
        private readonly IConfiguration _config;

        [BindProperty(SupportsGet = true)]
        public int? PlanId { get; set; }

        public string ApiBaseUrl => _config["ApiBaseUrl"] 
            ?? _config.GetSection("ApiSettings:BaseUrls").Get<string[]?>()?[0] 
            ?? "https://localhost:7000";

        public PlanDetail? Plan { get; set; }

        public CheckoutModel(IHttpClientFactory httpClientFactory, ILogger<CheckoutModel> logger, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token))
                return RedirectToPage("/Auth/Login");

            if (!PlanId.HasValue)
                return RedirectToPage("/Index");

            try
            {
                var client = _httpClientFactory.CreateClient("PayRexApi");
                var response = await client.GetAsync("api/superadmin/plans");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var plans = JsonSerializer.Deserialize<List<PlanDetail>>(json, opts) ?? new();
                    Plan = plans.FirstOrDefault(p => p.PlanId == PlanId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading plan for checkout");
            }

            if (Plan == null)
                return RedirectToPage("/Index");

            return Page();
        }

        public class PlanDetail
        {
            public int PlanId { get; set; }
            public string Name { get; set; } = "";
            public int MaxEmployees { get; set; }
            public decimal Price { get; set; }
            public string? Description { get; set; }
            public int? PlanUserLimit { get; set; }
            public string BillingCycle { get; set; } = "monthly";
        }
    }
}
