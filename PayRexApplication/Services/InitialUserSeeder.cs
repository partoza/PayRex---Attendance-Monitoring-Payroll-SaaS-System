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

 // Add Admin user (company-level Admin)
 if (!db.Users.Any(u => u.Email == "admin@payrex.local"))
 {
 var admin = new User
 {
 CompanyId = systemCompany.CompanyId,
 FirstName = "Default",
 LastName = "Admin",
 Email = "admin@payrex.local",
 PasswordHash = BCrypt.Net.BCrypt.HashPassword("PayRex12345!"),
 Role = UserRole.Admin,
 Status = UserStatus.Active,
 CreatedAt = DateTime.UtcNow,
 MustChangePassword = true
 };
 db.Users.Add(admin);
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
 }
 }
}
