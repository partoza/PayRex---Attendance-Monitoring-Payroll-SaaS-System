using System.Security.Claims;

namespace PayRexApplication.Helpers
{
    public static class ClaimsHelper
    {
        /// <summary>
        /// Gets the user ID from claims. All users are now in a single User table.
        /// Returns UserId and whether the user is a SuperAdmin.
        /// </summary>
        public static (int? UserId, bool IsSuperAdmin) GetUserIdFromClaims(ClaimsPrincipal user)
        {
       if (user == null) return (null, false);

          string? userClaim = user.FindFirst("userId")?.Value;
    string? uidClaim = user.FindFirst("uid")?.Value ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
      string? roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            int? userId = null;
        if (int.TryParse(userClaim, out var userIdFromClaim))
            {
           userId = userIdFromClaim;
       }
      else if (int.TryParse(uidClaim, out var uid))
         {
                userId = uid;
         }

            bool isSuperAdmin = roleClaim == "SuperAdmin";

            return (userId, isSuperAdmin);
    }
    }
}
