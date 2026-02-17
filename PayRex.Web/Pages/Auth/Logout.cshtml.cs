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

  public IActionResult OnGet()
        {
        // Remove the authentication cookie with matching path
       Response.Cookies.Delete("PayRex.AuthToken", new CookieOptions { Path = "/" });
            _logger.LogInformation("User logged out");
            return Page();
    }

  public IActionResult OnPost()
 {
                Response.Cookies.Delete("PayRex.AuthToken", new CookieOptions { Path = "/" });
            _logger.LogInformation("User logged out");
                // Perform a server-side redirect to Index so the browser requests
                // the homepage without the auth cookie (unauthenticated state).
                return RedirectToPage("/Index");
          }
    }
}
