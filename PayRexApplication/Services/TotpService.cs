using OtpNet;
using System;
using System.Text;

namespace PayRexApplication.Services
{
    /// <summary>
    /// Service for managing TOTP (Time-based One-Time Password) authentication
    /// Compatible with Google Authenticator (RFC6238)
    /// </summary>
    public interface ITotpService
    {
        /// <summary>
        /// Generate a new Base32-encoded secret key
        /// </summary>
        string GenerateSecretKey();

        /// <summary>
        /// Generate QR code URI for Google Authenticator
        /// </summary>
        string GenerateQrCodeUri(string email, string secretKey, string issuer = "PayRex");

        /// <summary>
        /// Verify a TOTP code against a secret key
        /// </summary>
        bool VerifyTotpCode(string secretKey, string totpCode);

        /// <summary>
        /// Format secret key for manual entry (with spaces every4 characters)
        /// </summary>
        string FormatSecretKeyForManualEntry(string secretKey);
    }

    /// <summary>
    /// Implementation of TOTP service using Otp.NET library
    /// </summary>
    public class TotpService : ITotpService
    {
        private const int SecretKeyLength =32; //32 bytes =256 bits (recommended for security)
        private const int TimeStepSeconds =30; // Standard TOTP time window
        private const int WindowTolerance =1; // Allow1 time window before/after for clock drift

        public string GenerateSecretKey()
        {
            // Generate cryptographically secure random bytes
            var key = KeyGeneration.GenerateRandomKey(SecretKeyLength);
 
            // Convert to Base32 (compatible with Google Authenticator)
            return Base32Encoding.ToString(key);
        }

        public string GenerateQrCodeUri(string email, string secretKey, string issuer = "PayRex")
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentException("Secret key cannot be empty", nameof(secretKey));

            // Format: otpauth://totp/Issuer:email?secret=SECRET&issuer=Issuer
            var label = $"{issuer}:{email}";
            var encodedLabel = Uri.EscapeDataString(label);
            var encodedIssuer = Uri.EscapeDataString(issuer);

            return $"otpauth://totp/{encodedLabel}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period={TimeStepSeconds}";
        }

        public bool VerifyTotpCode(string secretKey, string totpCode)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                return false;

            if (string.IsNullOrWhiteSpace(totpCode) || totpCode.Length !=6 || !long.TryParse(totpCode, out _))
                return false;

            try
            {
                // Normalize Base32 secret: remove spaces inserted for manual entry and use uppercase
                secretKey = secretKey.Replace(" ", "").ToUpperInvariant();

                // Decode Base32 secret key
                var keyBytes = Base32Encoding.ToBytes(secretKey);
 
                // Create TOTP instance
                var totp = new Totp(keyBytes, step: TimeStepSeconds, mode: OtpHashMode.Sha1, totpSize:6);
 
                // Verify with time window tolerance for clock drift
                var verificationWindow = new VerificationWindow(
                    previous: WindowTolerance,
                    future: WindowTolerance
                );

                return totp.VerifyTotp(totpCode, out _, verificationWindow);
            }
            catch
            {
                return false;
            }
        }

        public string FormatSecretKeyForManualEntry(string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
                return string.Empty;

            // Insert space every4 characters for easier manual entry
            var sb = new StringBuilder();
            for (int i =0; i < secretKey.Length; i++)
            {
                if (i >0 && i %4 ==0)
                    sb.Append(' ');
                sb.Append(secretKey[i]);
            }

            return sb.ToString();
        }
    }
}
