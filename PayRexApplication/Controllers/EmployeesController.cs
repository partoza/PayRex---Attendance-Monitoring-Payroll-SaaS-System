using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.DTOs;
using PayRexApplication.Enums;
using PayRexApplication.Models;
using PayRexApplication.Services;
using System.Security.Cryptography;
using System.Text;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin,HR")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ICloudinaryService _cloudinary;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            AppDbContext db,
            IEmailService emailService,
            ICloudinaryService cloudinary,
            ILogger<EmployeesController> logger)
        {
            _db = db;
            _emailService = emailService;
            _cloudinary = cloudinary;
            _logger = logger;
        }

        /// <summary>
        /// Get the next employee code for the current company
        /// </summary>
        [HttpGet("next-code")]
        public async Task<IActionResult> GetNextEmployeeCode()
        {
            var companyIdStr = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out var companyId)) return Forbid();

            // Load company to include companyName in response
            var company = await _db.Companies.FindAsync(companyId);
            var nextCode = await GenerateEmployeeCodeAsync(companyId);
            return Ok(new { employeeCode = nextCode, companyId, companyName = company?.CompanyName });
        }

        /// <summary>
        /// Create a new employee with auto-generated user account
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEmployeeDto dto, IFormFile? profilePhoto, IFormFile? signature)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var companyIdStr = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out var companyId)) return Forbid();

            // Check if email already exists in Users
            var normalizedEmail = dto.Email.Trim().ToLower();
            var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
                return Conflict(new { message = "An account with this email already exists." });

            // Get company info for password generation
            var company = await _db.Companies.FindAsync(companyId);
            if (company == null) return BadRequest(new { message = "Company not found." });

            // Generate employee code
            var employeeCode = await GenerateEmployeeCodeAsync(companyId);

            // Generate password: CompanyName + "_" + 8 random chars
            var password = GeneratePassword(company.CompanyName);

            // Determine the correct UserRole based on the selected EmployeeRole
            var userRole = UserRole.Employee;
            if (dto.RoleId.HasValue)
            {
                var selectedRole = await _db.EmployeeRoles.FindAsync(dto.RoleId.Value);
                if (selectedRole != null && selectedRole.CompanyId == companyId)
                {
                    userRole = selectedRole.RoleName?.Trim() switch
                    {
                        "HR" => UserRole.Hr,
                        "Accountant" => UserRole.Accountant,
                        _ => UserRole.Employee
                    };
                }
            }

            // Create User account first (need UserId for Cloudinary uploads)
            var user = new User
            {
                CompanyId = company.CompanyId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = userRole,
                Status = UserStatus.Active,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // Save to get UserId

            // Upload profile photo to Cloudinary
            string? profilePhotoUrl = null;
            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                try
                {
                    using var stream = profilePhoto.OpenReadStream();
                    profilePhotoUrl = await _cloudinary.UploadProfileImageAsync(stream, profilePhoto.FileName, user.UserId.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload profile photo");
                }
            }

            // Upload signature to Cloudinary
            string? signatureUrl = null;
            if (signature != null && signature.Length > 0)
            {
                try
                {
                    using var stream = signature.OpenReadStream();
                    signatureUrl = await _cloudinary.UploadSignatureAsync(stream, signature.FileName, $"{user.UserId}_sig");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload signature");
                }
            }

            // Update user with uploaded images (profile + signature) so user table mirrors employee
            try
            {
                var userToUpdate = await _db.Users.FindAsync(user.UserId);
                if (userToUpdate != null)
                {
                    if (!string.IsNullOrEmpty(profilePhotoUrl)) userToUpdate.ProfileImageUrl = profilePhotoUrl;
                    if (!string.IsNullOrEmpty(signatureUrl)) userToUpdate.SignatureUrl = signatureUrl;
                    userToUpdate.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user profile/signature URLs");
            }


            // Create Employee record
            var employee = new Employee
            {
                CompanyId = company.CompanyId,
                EmployeeCode = employeeCode,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                ContactNumber = dto.ContactNumber?.Trim(),
                CivilStatus = dto.CivilStatus,
                Birthdate = dto.Birthdate,
                StartDate = dto.StartDate ?? DateTime.UtcNow.Date,
                Status = dto.Status ?? EmployeeStatus.Active,
                TIN = dto.TIN,
                SSS = dto.SSS,
                PhilHealth = dto.PhilHealth,
                PagIbig = dto.PagIbig,
                // Profile and signature URLs are stored on the User entity instead
                RoleId = dto.RoleId,
                UserId = user.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            // Send welcome email with credentials
            try
            {
                await _emailService.SendWelcomeEmailAsync(
                    dto.Email.Trim(),
                    $"{dto.FirstName} {dto.LastName}",
                    company.CompanyName,
                    password,
                    company.LogoUrl
                );
                _logger.LogInformation("Welcome email sent to {Email} for employee {EmployeeCode}", dto.Email, employeeCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}. Employee created successfully but email was not sent.", dto.Email);
                // Don't fail the request if email fails - employee is already created
            }

            return CreatedAtAction(null, new
            {
                employeeNumber = employee.EmployeeNumber,
                employeeCode = employee.EmployeeCode,
                email = employee.Email,
                userId = user.UserId,
                message = "Employee created successfully. Login credentials have been sent to their email."
            });
        }

            /// <summary>
            /// Update an existing employee
            /// </summary>
            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromForm] CreateEmployeeDto dto, IFormFile? profilePhoto, IFormFile? signature)
            {
                var emp = await _db.Employees.FindAsync(id);
                if (emp == null) return NotFound(new { message = "Employee not found." });

                // Update basic fields
                emp.FirstName = dto.FirstName?.Trim() ?? emp.FirstName;
                emp.LastName = dto.LastName?.Trim() ?? emp.LastName;
                emp.Email = dto.Email?.Trim() ?? emp.Email;
                emp.ContactNumber = dto.ContactNumber?.Trim();
                emp.CivilStatus = dto.CivilStatus;
                emp.Birthdate = dto.Birthdate ?? emp.Birthdate;
                emp.StartDate = dto.StartDate ?? emp.StartDate;
                emp.RoleId = dto.RoleId ?? emp.RoleId;
                emp.TIN = dto.TIN;
                emp.SSS = dto.SSS;
                emp.PhilHealth = dto.PhilHealth;
                emp.PagIbig = dto.PagIbig;
                emp.UpdatedAt = DateTime.UtcNow;

                // Update linked user email/profile if exists
                if (emp.UserId.HasValue)
                {
                    var user = await _db.Users.FindAsync(emp.UserId.Value);
                    if (user != null)
                    {
                        if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email.Trim();

                        // Sync User.Role when EmployeeRole changes
                        if (dto.RoleId.HasValue)
                        {
                            var newRole = await _db.EmployeeRoles.FindAsync(dto.RoleId.Value);
                            if (newRole != null && newRole.CompanyId == emp.CompanyId)
                            {
                                user.Role = newRole.RoleName?.Trim() switch
                                {
                                    "HR" => UserRole.Hr,
                                    "Accountant" => UserRole.Accountant,
                                    _ => UserRole.Employee
                                };
                            }
                        }

                        user.UpdatedAt = DateTime.UtcNow;

                        // Handle profile photo upload
                        if (profilePhoto != null && profilePhoto.Length > 0)
                        {
                            try
                            {
                                using var stream = profilePhoto.OpenReadStream();
                                var url = await _cloudinary.UploadProfileImageAsync(stream, profilePhoto.FileName, user.UserId.ToString());
                                user.ProfileImageUrl = url;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to upload profile photo during update");
                            }
                        }

                        // Handle signature upload
                        if (signature != null && signature.Length > 0)
                        {
                            try
                            {
                                using var stream = signature.OpenReadStream();
                                var url = await _cloudinary.UploadSignatureAsync(stream, signature.FileName, $"{user.UserId}_sig");
                                user.SignatureUrl = url;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to upload signature during update");
                            }
                        }
                    }
                }

                // Handle status change: if transitioning from Inactive -> Active, reset password and email credentials
                var oldStatus = emp.Status;
                if (dto.Status.HasValue && dto.Status.Value != emp.Status)
                {
                    emp.Status = dto.Status.Value;

                    if (oldStatus == EmployeeStatus.Inactive && emp.Status == EmployeeStatus.Active && emp.UserId.HasValue)
                    {
                        try
                        {
                            var company = await _db.Companies.FindAsync(emp.CompanyId);
                            var password = GeneratePassword(company?.CompanyName ?? "Company");
                            var userToUpdate = await _db.Users.FindAsync(emp.UserId.Value);
                            if (userToUpdate != null)
                            {
                                userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                                userToUpdate.MustChangePassword = true;
                                userToUpdate.Status = UserStatus.Active;
                                userToUpdate.UpdatedAt = DateTime.UtcNow;
                                await _db.SaveChangesAsync();

                                // Send welcome/reset email with the new credentials
                                try
                                {
                                    await _emailService.SendWelcomeEmailAsync(
                                        userToUpdate.Email,
                                        $"{emp.FirstName} {emp.LastName}",
                                        company?.CompanyName ?? string.Empty,
                                        password,
                                        company?.LogoUrl
                                    );
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Failed to send re-activation email to {Email}", userToUpdate.Email);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while handling status transition for employee {EmployeeNumber}", emp.EmployeeNumber);
                        }
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(new { message = "Employee updated" });
            }

        /// <summary>
        /// Generate employee code: CompanyId-XXXX (sequential)
        /// </summary>
        private async Task<string> GenerateEmployeeCodeAsync(int companyId)
        {
            var company = await _db.Companies.FindAsync(companyId);
            var companyCode = company?.CompanyCode ?? companyId.ToString();

            var lastEmployee = await _db.Employees
                .Where(e => e.CompanyId == companyId)
                .OrderByDescending(e => e.EmployeeCode)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (lastEmployee != null)
            {
                // Parse the sequence number from the last employee code (format: CODE-0001)
                var parts = lastEmployee.EmployeeCode.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out var lastSeq))
                {
                    nextSequence = lastSeq + 1;
                }
            }

            return $"{companyCode}-{nextSequence:D4}";
        }

        /// <summary>
        /// Generate password: CompanyName_8RandomChars (letters, numbers, special chars)
        /// Format: CompanyName_haT?aq76
        /// </summary>
        private static string GeneratePassword(string companyName)
        {
            // Clean company name: replace spaces with underscores
            var cleanName = companyName.Replace(" ", "_");

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%&?";
            const string allChars = lowercase + uppercase + digits + special;

            var random = new byte[8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);

            var passwordChars = new char[8];

            // Ensure at least one of each type
            passwordChars[0] = lowercase[random[0] % lowercase.Length];
            passwordChars[1] = uppercase[random[1] % uppercase.Length];
            passwordChars[2] = digits[random[2] % digits.Length];
            passwordChars[3] = special[random[3] % special.Length];

            // Fill the rest randomly
            for (int i = 4; i < 8; i++)
            {
                passwordChars[i] = allChars[random[i] % allChars.Length];
            }

            // Shuffle the chars
            var shuffledRandom = new byte[8];
            rng.GetBytes(shuffledRandom);
            for (int i = passwordChars.Length - 1; i > 0; i--)
            {
                int j = shuffledRandom[i] % (i + 1);
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }

            return $"{cleanName}_{new string(passwordChars)}";
        }
    }
}
