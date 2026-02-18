namespace PayRexApplication.Data
{
    using Microsoft.EntityFrameworkCore;
    using PayRexApplication.Enums;
    using PayRexApplication.Models;
    using System;

    /// <summary>
    /// Defines the <see cref="AppDbContext" />
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class.
        /// </summary>
        /// <param name="options">The options<see cref="DbContextOptions{AppDbContext}"/></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Platform / SaaS Level

        /// <summary>
        /// Gets or sets the SubscriptionPlans
        /// </summary>
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

        /// <summary>
        /// Gets or sets the Companies
        /// </summary>
        public DbSet<Company> Companies { get; set; }

        /// <summary>
        /// Gets or sets the Subscriptions
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Gets or sets the BillingInvoices
        /// </summary>
        public DbSet<BillingInvoice> BillingInvoices { get; set; }

        /// <summary>
        /// Gets or sets the Payments
        /// </summary>
        public DbSet<Payment> Payments { get; set; }

        /// <summary>
        /// Gets or sets the EmployeeRoles
        /// </summary>
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }

        // All Users (SuperAdmin, Admin, HR)

        /// <summary>
        /// Gets or sets the Users
        /// </summary>
        public DbSet<User> Users { get; set; }

        // Login Attempts (for account lockout)

        /// <summary>
        /// Gets or sets the UserLoginAttempts
        /// </summary>
        public DbSet<UserLoginAttempt> UserLoginAttempts { get; set; }

        // Company Settings

        /// <summary>
        /// Gets or sets the CompanySettings
        /// </summary>
        public DbSet<CompanySetting> CompanySettings { get; set; }

        // Employees

        /// <summary>
        /// Gets or sets the Employees
        /// </summary>
        public DbSet<Employee> Employees { get; set; }

        /// <summary>
        /// Gets or sets the EmployeeQrCodes
        /// </summary>
        public DbSet<EmployeeQrCode> EmployeeQrCodes { get; set; }

        // Attendance

        /// <summary>
        /// Gets or sets the AttendanceRecords
        /// </summary>
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        /// <summary>
        /// Gets or sets the AttendanceScans
        /// </summary>
        public DbSet<AttendanceScan> AttendanceScans { get; set; }

        // Payroll

        /// <summary>
        /// Gets or sets the PayrollPeriods
        /// </summary>
        public DbSet<PayrollPeriod> PayrollPeriods { get; set; }

        /// <summary>
        /// Gets or sets the PayrollSummaries
        /// </summary>
        public DbSet<PayrollSummary> PayrollSummaries { get; set; }

        /// <summary>
        /// Gets or sets the PayrollApprovals
        /// </summary>
        public DbSet<PayrollApproval> PayrollApprovals { get; set; }

        // Government Contributions

        /// <summary>
        /// Gets or sets the GovernmentContributions
        /// </summary>
        public DbSet<GovernmentContribution> GovernmentContributions { get; set; }

        // Deductions & Benefits

        /// <summary>
        /// Gets or sets the EmployeeDeductions
        /// </summary>
        public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }

        /// <summary>
        /// Gets or sets the EmployeeBenefits
        /// </summary>
        public DbSet<EmployeeBenefit> EmployeeBenefits { get; set; }

        // Payslips

        /// <summary>
        /// Gets or sets the Payslips
        /// </summary>
        public DbSet<Payslip> Payslips { get; set; }

        // Audit Logs

        /// <summary>
        /// Gets or sets the AuditLogs
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Gets or sets the SystemSettings
        /// </summary>
        public DbSet<SystemSetting> SystemSettings { get; set; }

        /// <summary>
        /// The OnModelCreating
        /// </summary>
        /// <param name="modelBuilder">The modelBuilder<see cref="ModelBuilder"/></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== UserLoginAttempt Configuration =====
            modelBuilder.Entity<UserLoginAttempt>(entity =>
            {
                entity.HasKey(e => e.AttemptId);
                entity.Property(e => e.AttemptId).UseIdentityColumn();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ===== SubscriptionPlan Configuration =====
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(e => e.PlanId);
                entity.Property(e => e.PlanId).UseIdentityColumn();
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.BillingCycle).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
            });

            // ===== Company Configuration =====
            modelBuilder.Entity<Company>(entity =>
            {
              entity.HasKey(e => e.CompanyId);
              entity.Property(e => e.CompanyId).UseIdentityColumn();
              entity.Property(e => e.CompanyCode).HasMaxLength(4);
              entity.HasIndex(e => e.CompanyCode).IsUnique();
              entity.HasIndex(e => e.CompanyName);
              entity.Property(e => e.Status).HasConversion<int>();
              entity.Property(e => e.IsActive).HasDefaultValue(true);

              entity.Property(e => e.Address).HasMaxLength(1000);
              entity.Property(e => e.ContactEmail).HasMaxLength(256);
              entity.Property(e => e.ContactPhone).HasMaxLength(50);
              entity.Property(e => e.Tin).HasMaxLength(50);
              entity.Property(e => e.LogoUrl).HasMaxLength(512);

              entity.HasOne(e => e.SubscriptionPlan)
                .WithMany(p => p.Companies)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Subscription Configuration =====
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.SubscriptionId);
                entity.Property(e => e.SubscriptionId).UseIdentityColumn();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.PlanId);
                entity.Property(e => e.Status).HasConversion<int>();

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Subscriptions)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SubscriptionPlan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(e => e.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== BillingInvoice Configuration =====
            modelBuilder.Entity<BillingInvoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);
                entity.Property(e => e.InvoiceId).UseIdentityColumn();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.BillingInvoices)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Payment Configuration =====
            modelBuilder.Entity<Payment>(entity =>
    {
        entity.HasKey(e => e.PaymentId);
        entity.Property(e => e.PaymentId).UseIdentityColumn();
        entity.HasIndex(e => e.InvoiceId);
        entity.HasIndex(e => e.ReferenceNo);
        entity.Property(e => e.Status).HasConversion<int>();

        entity.HasOne(e => e.BillingInvoice)
         .WithMany(i => i.Payments)
              .HasForeignKey(e => e.InvoiceId)
              .OnDelete(DeleteBehavior.Cascade);
    });

            // ===== User Configuration =====
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).UseIdentityColumn();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();

                // 2FA and Profile fields
                entity.Property(e => e.IsTwoFactorEnabled).HasDefaultValue(false);
                entity.Property(e => e.TotpSecretKey).HasMaxLength(2048);
                entity.Property(e => e.RecoveryCodesHash).HasMaxLength(2048);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(512);
                entity.Property(e => e.LastPasswordChangeAt);
                entity.Property(e => e.MustChangePassword).HasDefaultValue(false);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Users)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== CompanySetting Configuration =====
            modelBuilder.Entity<CompanySetting>(entity =>
                       {
                           entity.HasKey(e => e.CompanyId);
                           entity.Property(e => e.PayrollCycle).HasConversion<int>();

                           entity.HasOne(e => e.Company)
               .WithOne(c => c.CompanySetting)
              .HasForeignKey<CompanySetting>(e => e.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);
                       });

            // ===== Employee Configuration =====
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeNumber);
                entity.Property(e => e.EmployeeNumber).UseIdentityColumn();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => new { e.CompanyId, e.EmployeeCode }).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();

                // Government ID fields
                entity.Property(e => e.TIN).HasMaxLength(15);
                entity.Property(e => e.SSS).HasMaxLength(12);
                entity.Property(e => e.PhilHealth).HasMaxLength(14);
                entity.Property(e => e.PagIbig).HasMaxLength(14);

                // Profile/signature now stored on User; no Employee columns for these

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Employees)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Role relationship (optional)
                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Employees)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);

                // User relationship (optional)
                // Use NoAction to avoid SQL Server "multiple cascade paths" errors
                entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.NoAction);
            });

            // ===== EmployeeRole Configuration =====
            modelBuilder.Entity<EmployeeRole>(entity =>
 {
     entity.HasKey(e => e.RoleId);
     entity.Property(e => e.RoleId).UseIdentityColumn();
     entity.HasIndex(e => e.CompanyId);
     entity.Property(e => e.RoleName).HasMaxLength(100);
     entity.Property(e => e.RateType).HasMaxLength(50);
     entity.Property(e => e.Description).HasMaxLength(500);

     entity.HasOne(e => e.Company)
     .WithMany(c => c.EmployeeRoles)
     .HasForeignKey(e => e.CompanyId)
     .OnDelete(DeleteBehavior.Restrict);
 });

            // ===== EmployeeQrCode Configuration =====
            modelBuilder.Entity<EmployeeQrCode>(entity =>
           {
               entity.HasKey(e => e.QrId);
               entity.Property(e => e.QrId).UseIdentityColumn();
               entity.HasIndex(e => e.EmployeeId).IsUnique();
               entity.HasIndex(e => e.QrValue).IsUnique();

               entity.HasOne(e => e.Employee)
   .WithOne(emp => emp.EmployeeQrCode)
  .HasForeignKey<EmployeeQrCode>(e => e.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
           });

            // ===== AttendanceRecord Configuration =====
            modelBuilder.Entity<AttendanceRecord>(entity =>
           {
               entity.HasKey(e => e.AttendanceId);
               entity.Property(e => e.AttendanceId).UseIdentityColumn();
               entity.HasIndex(e => e.EmployeeId);
               entity.HasIndex(e => e.CompanyId);
               entity.HasIndex(e => new { e.EmployeeId, e.Date }).IsUnique();
               entity.Property(e => e.Source).HasConversion<int>();

               entity.HasOne(e => e.Employee)
         .WithMany(emp => emp.AttendanceRecords)
               .HasForeignKey(e => e.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);

               entity.HasOne(e => e.Company)
     .WithMany(c => c.AttendanceRecords)
      .HasForeignKey(e => e.CompanyId)
.OnDelete(DeleteBehavior.NoAction);
           });

            // ===== AttendanceScan Configuration =====
            modelBuilder.Entity<AttendanceScan>(entity =>
     {
         entity.HasKey(e => e.ScanId);
         entity.Property(e => e.ScanId).UseIdentityColumn();
         entity.HasIndex(e => e.EmployeeId);
         entity.HasIndex(e => e.ScanTime);
         entity.Property(e => e.ScanType).HasConversion<int>();
         entity.Property(e => e.Result).HasConversion<int>();

         entity.HasOne(e => e.Employee)
               .WithMany(emp => emp.AttendanceScans)
            .HasForeignKey(e => e.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);
     });

            // ===== PayrollPeriod Configuration =====
            modelBuilder.Entity<PayrollPeriod>(entity =>
 {
     entity.HasKey(e => e.PayrollPeriodId);
     entity.Property(e => e.PayrollPeriodId).UseIdentityColumn();
     entity.HasIndex(e => e.CompanyId);
     entity.HasIndex(e => new { e.CompanyId, e.StartDate, e.EndDate }).IsUnique();
     entity.Property(e => e.Status).HasConversion<int>();

     entity.HasOne(e => e.Company)
.WithMany(c => c.PayrollPeriods)
  .HasForeignKey(e => e.CompanyId)
.OnDelete(DeleteBehavior.Cascade);
 });

            // ===== PayrollSummary Configuration =====
            modelBuilder.Entity<PayrollSummary>(entity =>
  {
      entity.HasKey(e => e.PayrollSummaryId);
      entity.Property(e => e.PayrollSummaryId).UseIdentityColumn();
      entity.HasIndex(e => e.PayrollPeriodId);
      entity.HasIndex(e => e.EmployeeId);
      entity.HasIndex(e => new { e.PayrollPeriodId, e.EmployeeId }).IsUnique();

      entity.HasOne(e => e.PayrollPeriod)
.WithMany(p => p.PayrollSummaries)
        .HasForeignKey(e => e.PayrollPeriodId)
.OnDelete(DeleteBehavior.Cascade);

      entity.HasOne(e => e.Employee)
       .WithMany(emp => emp.PayrollSummaries)
         .HasForeignKey(e => e.EmployeeId)
    .OnDelete(DeleteBehavior.NoAction);
  });

            // ===== PayrollApproval Configuration =====
            modelBuilder.Entity<PayrollApproval>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);
                entity.Property(e => e.ApprovalId).UseIdentityColumn();
                entity.HasIndex(e => e.PayrollPeriodId);
                entity.HasIndex(e => e.ApprovedBy);
                entity.Property(e => e.Status).HasConversion<int>();

                entity.HasOne(e => e.PayrollPeriod)
           .WithMany(p => p.PayrollApprovals)
         .HasForeignKey(e => e.PayrollPeriodId)
       .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ApprovedByUser)
                         .WithMany(u => u.PayrollApprovals)
                       .HasForeignKey(e => e.ApprovedBy)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ===== GovernmentContribution Configuration =====
            modelBuilder.Entity<GovernmentContribution>(entity =>
 {
     entity.HasKey(e => e.ContributionId);
     entity.Property(e => e.ContributionId).UseIdentityColumn();
     entity.HasIndex(e => e.EmployeeId);
     entity.HasIndex(e => e.PayrollPeriodId);
     entity.HasIndex(e => new { e.EmployeeId, e.PayrollPeriodId, e.Type }).IsUnique();
     entity.Property(e => e.Type).HasConversion<int>();

     entity.HasOne(e => e.Employee)
            .WithMany(emp => emp.GovernmentContributions)
       .HasForeignKey(e => e.EmployeeId)
     .OnDelete(DeleteBehavior.Cascade);

     entity.HasOne(e => e.PayrollPeriod)
.WithMany(p => p.GovernmentContributions)
.HasForeignKey(e => e.PayrollPeriodId)
.OnDelete(DeleteBehavior.NoAction);
 });

            // ===== EmployeeDeduction Configuration =====
            modelBuilder.Entity<EmployeeDeduction>(entity =>
                 {
                     entity.HasKey(e => e.DeductionId);
                     entity.Property(e => e.DeductionId).UseIdentityColumn();
                     entity.HasIndex(e => e.EmployeeId);

                     entity.HasOne(e => e.Employee)
          .WithMany(emp => emp.EmployeeDeductions)
             .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
                 });

            // ===== EmployeeBenefit Configuration =====
            modelBuilder.Entity<EmployeeBenefit>(entity =>
                 {
                     entity.HasKey(e => e.BenefitId);
                     entity.Property(e => e.BenefitId).UseIdentityColumn();
                     entity.HasIndex(e => e.EmployeeId);

                     entity.HasOne(e => e.Employee)
            .WithMany(emp => emp.EmployeeBenefits)
                  .HasForeignKey(e => e.EmployeeId)
                 .OnDelete(DeleteBehavior.Cascade);
                 });

            // ===== Payslip Configuration =====
            modelBuilder.Entity<Payslip>(entity =>
                {
                    entity.HasKey(e => e.PayslipId);
                    entity.Property(e => e.PayslipId).UseIdentityColumn();
                    entity.HasIndex(e => e.PayrollSummaryId).IsUnique();

                    entity.HasOne(e => e.PayrollSummary)
          .WithOne(ps => ps.Payslip)
             .HasForeignKey<Payslip>(e => e.PayrollSummaryId)
       .OnDelete(DeleteBehavior.Cascade);
                });

            // ===== AuditLog Configuration =====
            modelBuilder.Entity<AuditLog>(entity =>
 {
          entity.HasKey(e => e.AuditId);
entity.Property(e => e.AuditId).UseIdentityColumn();
          entity.HasIndex(e => e.CompanyId);
      entity.HasIndex(e => e.UserId);
    entity.HasIndex(e => e.CreatedAt);
  entity.HasIndex(e => e.Action);
  entity.Property(e => e.Role).HasMaxLength(50);
  entity.Property(e => e.EntityAffected).HasMaxLength(200);

 entity.HasOne(e => e.Company)
    .WithMany(c => c.AuditLogs)
     .HasForeignKey(e => e.CompanyId)
  .OnDelete(DeleteBehavior.NoAction);

       entity.HasOne(e => e.User)
     .WithMany(u => u.AuditLogs)
      .HasForeignKey(e => e.UserId)
         .OnDelete(DeleteBehavior.NoAction);
    });

       // ===== SystemSetting Configuration =====
modelBuilder.Entity<SystemSetting>(entity =>
       {
         entity.HasKey(e => e.SettingId);
         entity.Property(e => e.SettingId).UseIdentityColumn();
         entity.HasIndex(e => e.EffectiveDate);
            });

      // ===== SEED DATA =====
    SeedData(modelBuilder);
        }

        /// <summary>
        /// The SeedData
        /// </summary>
        /// <param name="modelBuilder">The modelBuilder<see cref="ModelBuilder"/></param>
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Pre-generated TOTP secret key for testing (Base32 encoded)
            // This allows testing TOTP without needing to set it up first
            // In Google Authenticator, add manually with key: JBSWY3DPEHPK3PXP (formatted: JBSW Y3DP EHPK 3PXP)
            const string TestTotpSecretKey = "JBSWY3DPEHPK3PXP";

            // Seed default subscription plans
            var plans = new[]
         {
             new SubscriptionPlan
           {
 PlanId = 1,
      Name = "Basic",
    MaxEmployees = 50,
     Price = 2499.00m,
    BillingCycle = BillingCycle.Monthly,
      Status = PlanStatus.Active,
     Description = "For small to medium Filipino businesses",
PlanUserLimit = 3,
    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
           },
    new SubscriptionPlan
  {
    PlanId = 2,
    Name = "Pro",
       MaxEmployees = 200,
   Price = 4999.00m,
    BillingCycle = BillingCycle.Monthly,
        Status = PlanStatus.Active,
      Description = "For growing Philippine enterprises",
  PlanUserLimit = 10,
      CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
   },
    new SubscriptionPlan
     {
      PlanId = 3,
            Name = "Enterprise",
   MaxEmployees = 10000,
       Price = 9999.00m,
          BillingCycle = BillingCycle.Monthly,
     Status = PlanStatus.Active,
      Description = "Unlimited employees with dedicated support",
        PlanUserLimit = null, // unlimited
      CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    }
 };

        modelBuilder.Entity<SubscriptionPlan>().HasData(plans);

     // Seed System Company for SuperAdmin (CompanyId = "0000")
   var systemCompany = new Company
   {
  CompanyId = 1,
  CompanyCode = "0000",
    CompanyName = "PayRex System",
  PlanId = 3, // Enterprise plan
   Status = CompanyStatus.Active,
          IsActive = true,
    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

            modelBuilder.Entity<Company>().HasData(systemCompany);

  // Seed SuperAdmin User: John Rex Partoza
            var superAdmin = new User
     {
           UserId = 1,
        CompanyId = 1,
  FirstName = "John Rex",
      LastName = "Partoza",
     Email = "partozajohnrex@gmail.com",
       PasswordHash = BCrypt.Net.BCrypt.HashPassword("09284584655Rex!"),
     Role = UserRole.SuperAdmin,
 Status = UserStatus.Active,
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
       IsTwoFactorEnabled = true,
     TotpSecretKey = TestTotpSecretKey,
       MustChangePassword = false
          };

   modelBuilder.Entity<User>().HasData(superAdmin);

     // ========================================
 // Seed demo company and users for Admin and HR
 // ========================================
 var demoCompany = new Company
 {
 CompanyId = 2,
 CompanyCode = "1001",
 CompanyName = "Demo Company",
 PlanId =1,
 Status = CompanyStatus.Active,
 IsActive = true,
 CreatedAt = new DateTime(2025,2,1,0,0,0, DateTimeKind.Utc)
 };

 modelBuilder.Entity<Company>().HasData(demoCompany);

 // Shared password for seeded users
 const string seededPassword = "PayRex12345!";

 var adminUser = new User
 {
 UserId =2,
 CompanyId = demoCompany.CompanyId,
 FirstName = "Alice",
 LastName = "Admin",
 Email = "alice.admin@example.com",
 PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
 Role = UserRole.Admin,
 Status = UserStatus.Active,
 CreatedAt = new DateTime(2025,2,1,0,0,0, DateTimeKind.Utc),
 MustChangePassword = false
 };

 var hrUser = new User
 {
 UserId =3,
 CompanyId = demoCompany.CompanyId,
 FirstName = "Hannah",
 LastName = "HR",
 Email = "hannah.hr@example.com",
 PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
 Role = UserRole.Hr,
 Status = UserStatus.Active,
 CreatedAt = new DateTime(2025,2,1,0,0,0, DateTimeKind.Utc),
 MustChangePassword = false
 };

 modelBuilder.Entity<User>().HasData(adminUser, hrUser);

     // Seed initial SystemSetting with Philippine government contribution percentages
    var systemSetting = new SystemSetting
    {
  SettingId = 1,
         SssPercentage = 4.5m,       // Employee share SSS ~4.5%
       PagIbigPercentage = 2.0m,    // Employee share Pag-IBIG 2%
     PhilHealthPercentage = 2.25m, // Employee share PhilHealth 2.25% (of half)
                EffectiveDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
      Note = "Initial Philippine government contribution rates for 2025",
     CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

         modelBuilder.Entity<SystemSetting>().HasData(systemSetting);
        }
    }
}
