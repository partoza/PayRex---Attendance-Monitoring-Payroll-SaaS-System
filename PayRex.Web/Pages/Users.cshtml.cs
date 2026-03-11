using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR")]
    public class UsersModel : PageModel
    {
        private readonly AppDbContext _db;
        public List<UserItem> Users { get; set; } = new();
        [TempData] public string? StatusMessage { get; set; }

        public UsersModel(AppDbContext db) => _db = db;

        private int GetCompanyId()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return 0;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "companyId")?.Value, out var cid) ? cid : 0;
            }
            catch { return 0; }
        }

        public async Task OnGetAsync()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return;

            Users = await _db.Users
                .AsNoTracking()
                .Where(u => u.CompanyId == companyId)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserItem
                {
                    UserId          = u.UserId,
                    Name            = (u.FirstName + " " + u.LastName).Trim(),
                    Email           = u.Email ?? "",
                    Role            = u.Role.ToString(),
                    Status          = u.Status == UserStatus.Active ? "Active" : "Inactive",
                    DateJoined      = u.CreatedAt,
                    ProfileImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int userId)
        {
            var companyId = GetCompanyId();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.CompanyId == companyId);
            if (user == null) { StatusMessage = "error:User not found."; return RedirectToPage(); }

            user.Status = user.Status == UserStatus.Active ? UserStatus.Suspended : UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var action = user.Status == UserStatus.Active ? "activated" : "suspended";
            StatusMessage = $"success:User {user.FirstName} {user.LastName} has been {action}.";
            return RedirectToPage();
        }

        public class UserItem
        {
            public int UserId { get; set; }
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime DateJoined { get; set; }
            public string? ProfileImageUrl { get; set; }
        }
    }
}
