using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PayRex.Web.Pages
{
    public class UnauthorizedModel : PageModel
    {
        public string CurrentUserRole { get; set; } = "Guest";

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                CurrentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            }
        }
    }
}
