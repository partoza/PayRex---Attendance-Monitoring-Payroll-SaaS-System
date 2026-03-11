using System;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.Enums;

namespace PayRexApplication.Services
{
 /// <summary>
 /// Simple helper to seed initial Admin and HR users for the system company
 /// </summary>
 public static class InitialUserSeeder
 {
 public static void EnsureSeed(AppDbContext db)
 {
 // Ensure system company exists (by company code)
 var systemCompany = db.Companies.FirstOrDefault(c => c.CompanyCode == "0000");
 if (systemCompany == null)
 {
	 systemCompany = new Company
	 {
		 CompanyCode = "0000",
		 CompanyName = "PayRex System",
		 PlanId = 3,
		 Status = CompanyStatus.Active,
		 IsActive = true,
		 CreatedAt = DateTime.UtcNow
	 };
	 db.Companies.Add(systemCompany);
	 db.SaveChanges();
 }

 // Add or update Admin user (company-level Admin). Ensure username/email is brewtracks@payrex.com
 var existingAdmin = db.Users.FirstOrDefault(u => u.Role == UserRole.Admin || u.Email == "admin@payrex.local");
 if (existingAdmin == null)
 {
	 var admin = new User
	 {
		 CompanyId = systemCompany.CompanyId,
		 FirstName = "Default",
		 LastName = "Admin",
		 Email = "brewtracks@payrex.com",
		 PasswordHash = BCrypt.Net.BCrypt.HashPassword("PayRex12345!"),
		 Role = UserRole.Admin,
		 Status = UserStatus.Active,
		 CreatedAt = DateTime.UtcNow,
		 MustChangePassword = true
	 };
	 db.Users.Add(admin);
	 db.SaveChanges();
 }
 else
 {
	 existingAdmin.Email = "brewtracks@payrex.com";
	 // keep existing password unless it's the default seeder one; do not override password here
	 existingAdmin.UpdatedAt = DateTime.UtcNow;
	 db.SaveChanges();
 }

 // Add HR user
 if (!db.Users.Any(u => u.Email == "hr@payrex.local"))
 {
	 var hr = new User
	 {
		 CompanyId = systemCompany.CompanyId,
		 FirstName = "Default",
		 LastName = "HR",
		 Email = "hr@payrex.local",
		 PasswordHash = BCrypt.Net.BCrypt.HashPassword("PayRex12345!"),
		 Role = UserRole.Hr,
		 Status = UserStatus.Active,
		 CreatedAt = DateTime.UtcNow,
		 MustChangePassword = true
	 };
	 db.Users.Add(hr);
	 db.SaveChanges();
 }

 // Ensure there is a SuperAdmin; if present, update password and disable TOTP. If not present, create one.
 var super = db.Users.FirstOrDefault(u => u.Role == UserRole.SuperAdmin);
 if (super == null)
 {
	 var sa = new User
	 {
		 CompanyId = systemCompany.CompanyId,
		 FirstName = "System",
		 LastName = "Super",
		 Email = "superadmin@payrex.local",
		 PasswordHash = BCrypt.Net.BCrypt.HashPassword("PayRex12345!"),
		 Role = UserRole.SuperAdmin,
		 Status = UserStatus.Active,
		 CreatedAt = DateTime.UtcNow,
		 MustChangePassword = true,
		 IsTwoFactorEnabled = false
	 };
	 db.Users.Add(sa);
	 db.SaveChanges();
 }
 else
 {
	 // Update only password and TOTP flag as requested
	 super.PasswordHash = BCrypt.Net.BCrypt.HashPassword("PayRex12345!");
	 super.IsTwoFactorEnabled = false;
	 super.UpdatedAt = DateTime.UtcNow;
	 db.SaveChanges();
 }
 }
 }
}
