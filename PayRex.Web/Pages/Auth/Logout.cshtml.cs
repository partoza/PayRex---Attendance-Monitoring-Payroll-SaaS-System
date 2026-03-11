using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayRex.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
     {
   _logger = logger;
      }

    public async Task<IActionResult> OnGetAsync()
    {
        var cookieOptions = new CookieOptions { Path = "/" };
        var host = Request.Host.Host ?? string.Empty;
        if (!host.Contains("localhost") && host.Contains("runasp.net"))
        {
          cookieOptions.Domain = ".runasp.net";
          cookieOptions.Secure = true;
        }
        Response.Cookies.Delete("PayRex.AuthToken", cookieOptions);
      _logger.LogInformation("User logged out");
      return RedirectToPage("/Auth/Login");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var cookieOptions = new CookieOptions { Path = "/" };
        var host = Request.Host.Host ?? string.Empty;
        if (!host.Contains("localhost") && host.Contains("runasp.net"))
        {
          cookieOptions.Domain = ".runasp.net";
          cookieOptions.Secure = true;
        }
        Response.Cookies.Delete("PayRex.AuthToken", cookieOptions);
      _logger.LogInformation("User logged out");
      return RedirectToPage("/Auth/Login");
    }
    }
}
