using System;
using System.Security.Cryptography;

namespace PayRexApplication.Services
{
    /// <summary>
    /// Service for generating and hashing password reset tokens
    /// </summary>
    public interface IPasswordResetTokenService
    {
        /// <summary>
        /// Generate a cryptographically secure random token
        /// </summary>
        string GenerateToken();

        /// <summary>
        /// Hash a token using SHA256 for secure database storage
      /// </summary>
        string HashToken(string token);

        /// <summary>
    /// Verify if a plain token matches the stored hash
        /// </summary>
     bool VerifyToken(string plainToken, string hashedToken);
  }

    /// <summary>
    /// Implementation of password reset token service
    /// </summary>
public class PasswordResetTokenService : IPasswordResetTokenService
    {
        private const int TokenByteLength = 32; // 32 bytes = 256 bits

        public string GenerateToken()
     {
            // Generate cryptographically secure random bytes
   var randomBytes = new byte[TokenByteLength];
 using (var rng = RandomNumberGenerator.Create())
            {
       rng.GetBytes(randomBytes);
      }

            // Convert to Base64 URL-safe string (no +, /, or =)
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
       .Replace("/", "_")
                .Replace("=", "");
        }

        public string HashToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

  // Hash using SHA256
            using var sha256 = SHA256.Create();
    var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
      var hashBytes = sha256.ComputeHash(tokenBytes);

          // Convert to hexadecimal string
         return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

   public bool VerifyToken(string plainToken, string hashedToken)
        {
            if (string.IsNullOrWhiteSpace(plainToken) || string.IsNullOrWhiteSpace(hashedToken))
         return false;

          try
            {
      var computedHash = HashToken(plainToken);
      
  // Constant-time comparison to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(
          System.Text.Encoding.UTF8.GetBytes(computedHash),
        System.Text.Encoding.UTF8.GetBytes(hashedToken)
     );
 }
            catch
          {
         return false;
       }
 }
    }
}
