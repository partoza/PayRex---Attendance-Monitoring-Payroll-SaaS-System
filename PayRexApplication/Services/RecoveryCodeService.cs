using System.Security.Cryptography;
using System.Text;

namespace PayRexApplication.Services
{
    /// <summary>
  /// Service for generating and managing 2FA recovery codes
    /// </summary>
    public interface IRecoveryCodeService
  {
      /// <summary>
        /// Generate a set of recovery codes
        /// </summary>
        /// <param name="count">Number of codes to generate (default: 10)</param>
        /// <returns>List of plain-text recovery codes</returns>
        List<string> GenerateRecoveryCodes(int count = 10);

        /// <summary>
        /// Hash recovery codes for secure storage
     /// </summary>
        /// <param name="codes">Plain-text recovery codes</param>
      /// <returns>Comma-separated hashed codes</returns>
        string HashRecoveryCodes(List<string> codes);

        /// <summary>
  /// Verify a recovery code against stored hashes
        /// </summary>
 /// <param name="code">Plain-text recovery code to verify</param>
        /// <param name="storedHashes">Comma-separated stored hashes</param>
        /// <returns>True if valid, false otherwise</returns>
        bool VerifyRecoveryCode(string code, string storedHashes);

        /// <summary>
        /// Remove a used recovery code from stored hashes
        /// </summary>
        /// <param name="code">The used recovery code</param>
        /// <param name="storedHashes">Current comma-separated hashes</param>
        /// <returns>Updated comma-separated hashes with the used code removed</returns>
   string RemoveUsedRecoveryCode(string code, string storedHashes);

        /// <summary>
 /// Get remaining recovery code count
     /// </summary>
        /// <param name="storedHashes">Comma-separated stored hashes</param>
        /// <returns>Number of remaining codes</returns>
        int GetRemainingCodeCount(string? storedHashes);
    }

    public class RecoveryCodeService : IRecoveryCodeService
    {
  private const int CodeLength = 8;
     private const string CodeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excluded confusing chars: I, O, 0, 1

        public List<string> GenerateRecoveryCodes(int count = 10)
        {
            var codes = new List<string>();

   using (var rng = RandomNumberGenerator.Create())
       {
   for (int i = 0; i < count; i++)
       {
          var code = GenerateSingleCode(rng);
   codes.Add(code);
  }
 }

            return codes;
        }

        private string GenerateSingleCode(RandomNumberGenerator rng)
        {
       var bytes = new byte[CodeLength];
          rng.GetBytes(bytes);

   var sb = new StringBuilder(CodeLength + 1);

       for (int i = 0; i < CodeLength; i++)
 {
       // Add hyphen in the middle for readability (XXXX-XXXX)
 if (i == 4)
     sb.Append('-');

                var index = bytes[i] % CodeCharacters.Length;
     sb.Append(CodeCharacters[index]);
        }

       return sb.ToString();
        }

        public string HashRecoveryCodes(List<string> codes)
        {
    var hashedCodes = codes.Select(code => HashCode(NormalizeCode(code)));
            return string.Join(",", hashedCodes);
        }

        private string HashCode(string code)
        {
  using (var sha256 = SHA256.Create())
            {
     var bytes = Encoding.UTF8.GetBytes(code);
    var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
            }
        }

        private string NormalizeCode(string code)
        {
      // Remove hyphens and convert to uppercase for consistent hashing
   return code.Replace("-", "").ToUpperInvariant();
      }

      public bool VerifyRecoveryCode(string code, string storedHashes)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(storedHashes))
             return false;

     var normalizedCode = NormalizeCode(code);
         var inputHash = HashCode(normalizedCode);
   var hashes = storedHashes.Split(',', StringSplitOptions.RemoveEmptyEntries);

            return hashes.Contains(inputHash);
        }

      public string RemoveUsedRecoveryCode(string code, string storedHashes)
 {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(storedHashes))
            return storedHashes ?? string.Empty;

   var normalizedCode = NormalizeCode(code);
            var inputHash = HashCode(normalizedCode);
 var hashes = storedHashes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

   hashes.Remove(inputHash);

         return string.Join(",", hashes);
        }

public int GetRemainingCodeCount(string? storedHashes)
        {
            if (string.IsNullOrEmpty(storedHashes))
      return 0;

  return storedHashes.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
