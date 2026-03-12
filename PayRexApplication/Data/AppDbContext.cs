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
        /// Gets or sets the RolePermissions
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Gets or sets the LeaveRequests
        /// </summary>
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        /// <summary>
        /// Gets or sets the IncomeRecords
        /// </summary>
        public DbSet<IncomeRecord> IncomeRecords { get; set; }

        /// <summary>
        /// Gets or sets the ExpenseRecords
        /// </summary>
        public DbSet<ExpenseRecord> ExpenseRecords { get; set; }

        public DbSet<FinanceEntry> FinanceEntries { get; set; }

        public DbSet<SystemNotification> SystemNotifications { get; set; }

        public DbSet<NotificationRead> NotificationReads { get; set; }

        /// <summary>
        /// The OnModelCreating
        /// </summary>
        /// <param name="modelBuilder">The modelBuilder<see cref="ModelBuilder"/></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== NotificationRead Configuration =====
            modelBuilder.Entity<NotificationRead>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
                entity.HasIndex(e => new { e.UserId, e.NotificationId }).IsUnique();
            });

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

                entity.HasOne(e => e.LastPayment)
                    .WithMany()
                    .HasForeignKey(e => e.LastPaymentId)
                    .OnDelete(DeleteBehavior.SetNull);
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
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Subscription)
                    .WithMany()
                    .HasForeignKey(e => e.SubscriptionId)
                    .OnDelete(DeleteBehavior.NoAction);
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

                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.PayMongoPaymentIntentId);
                entity.HasIndex(e => e.PayMongoCheckoutSessionId);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.NoAction);
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

            // ===== LeaveRequest Configuration =====
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.LeaveRequestId);
                entity.Property(e => e.LeaveRequestId).UseIdentityColumn();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.EmployeeId);

                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Reviewer)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedBy)
                    .OnDelete(DeleteBehavior.NoAction);
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
 };

            modelBuilder.Entity<SubscriptionPlan>().HasData(plans);

            // Seed System Company for SuperAdmin (CompanyId = "0000")
            var systemCompany = new Company
            {
                CompanyId = 1,
                CompanyCode = "0000",
                CompanyName = "PayRex System",
                PlanId = 2, // Pro plan
                Status = CompanyStatus.Active,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<Company>().HasData(systemCompany);

            // Seed SuperAdmin User: John Rex Partoza (updated password)
            var superAdmin = new User
            {
                UserId = 1,
                CompanyId = 1,
                FirstName = "John Rex",
                LastName = "Partoza",
                Email = "partozajohnrex@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("PayRex12345!"),
                Role = UserRole.SuperAdmin,
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsTwoFactorEnabled = true,
                TotpSecretKey = TestTotpSecretKey,
                MustChangePassword = false
            };

            modelBuilder.Entity<User>().HasData(superAdmin);

            // ========================================
            // Seed demo company and users for Admin
            // ========================================
            var demoCompany = new Company
            {
                CompanyId = 2,
                CompanyCode = "1001",
                CompanyName = "Demo Company",
                PlanId = 1,
                Status = CompanyStatus.Active,
                IsActive = true,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<Company>().HasData(demoCompany);

            // Shared password for seeded users
            const string seededPassword = "PayRex12345!";

            var adminUser = new User
            {
                UserId = 2,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Dency",
                LastName = "Samson",
                Email = "dencysamson@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                MustChangePassword = false
            };

            // Note: HR user removed per request
            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed HR user for demo company
            var hrUser = new User
            {
                UserId = 4,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Ana",
                LastName = "Cruz",
                Email = "anacruz@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Hr,
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(hrUser);

            // Seed Accountant user for demo company
            var accountantUser = new User
            {
                UserId = 3,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Maria",
                LastName = "Santos",
                Email = "mariasantos@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Accountant,
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                MustChangePassword = false
            };

            modelBuilder.Entity<User>().HasData(accountantUser);

            // Seed built-in EmployeeRoles for demo company
            var builtInHrRole = new EmployeeRole
            {
                RoleId = 1,
                CompanyId = demoCompany.CompanyId,
                RoleName = "HR",
                Description = "Human Resource Manager - manages employees and attendance",
                IsBuiltIn = true,
                IsActive = true,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var builtInAccountantRole = new EmployeeRole
            {
                RoleId = 2,
                CompanyId = demoCompany.CompanyId,
                RoleName = "Accountant",
                Description = "Accountant - manages salary, finance, and payslips",
                IsBuiltIn = true,
                IsActive = true,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            modelBuilder.Entity<EmployeeRole>().HasData(builtInHrRole, builtInAccountantRole);

            // Seed a Staff role for demo company
            var staffRole = new EmployeeRole
            {
                RoleId = 3,
                CompanyId = demoCompany.CompanyId,
                RoleName = "Staff",
                BasicRate = 600.00m,
                RateType = "Daily",
                Description = "Regular staff member",
                IsBuiltIn = false,
                IsActive = true,
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            modelBuilder.Entity<EmployeeRole>().HasData(staffRole);

            // Seed Employees — 1 HR, 1 Accountant, 3 Staff for Dency Samson's company
            var seedEmpDate = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc);

            // HR Employee (linked to hrUser)
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 1,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0001",
                FirstName = "Ana",
                LastName = "Cruz",
                Email = "anacruz@gmail.com",
                ContactNumber = "09171234501",
                CivilStatus = "Single",
                Birthdate = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 800.00m,
                TIN = "123-456-781",
                SSS = "33-1234561-1",
                PhilHealth = "12-345678901-1",
                PagIbig = "1234-5678-9011",
                RoleId = builtInHrRole.RoleId,
                UserId = hrUser.UserId,
                CreatedAt = seedEmpDate
            });

            // Accountant Employee (linked to accountantUser)
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 2,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0002",
                FirstName = "Maria",
                LastName = "Santos",
                Email = "mariasantos@gmail.com",
                ContactNumber = "09171234502",
                CivilStatus = "Married",
                Birthdate = new DateTime(1988, 8, 22, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 750.00m,
                TIN = "123-456-782",
                SSS = "33-1234562-2",
                PhilHealth = "12-345678902-2",
                PagIbig = "1234-5678-9022",
                RoleId = builtInAccountantRole.RoleId,
                UserId = accountantUser.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 1 (Staff)
            var emp1User = new User
            {
                UserId = 5,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Juan",
                LastName = "Dela Cruz",
                Email = "juandelacruz@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp1User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 3,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0003",
                FirstName = "Juan",
                LastName = "Dela Cruz",
                Email = "juandelacruz@gmail.com",
                ContactNumber = "09171234503",
                CivilStatus = "Single",
                Birthdate = new DateTime(1995, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 600.00m,
                TIN = "123-456-783",
                SSS = "33-1234563-3",
                PhilHealth = "12-345678903-3",
                PagIbig = "1234-5678-9033",
                RoleId = staffRole.RoleId,
                UserId = emp1User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 2 (Staff)
            var emp2User = new User
            {
                UserId = 6,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Rosa",
                LastName = "Garcia",
                Email = "rosagarcia@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp2User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 4,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0004",
                FirstName = "Rosa",
                LastName = "Garcia",
                Email = "rosagarcia@gmail.com",
                ContactNumber = "09171234504",
                CivilStatus = "Married",
                Birthdate = new DateTime(1992, 11, 28, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 600.00m,
                TIN = "123-456-784",
                SSS = "33-1234564-4",
                PhilHealth = "12-345678904-4",
                PagIbig = "1234-5678-9044",
                RoleId = staffRole.RoleId,
                UserId = emp2User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 3 (Staff)
            var emp3User = new User
            {
                UserId = 7,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Pedro",
                LastName = "Reyes",
                Email = "pedroreyes@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp3User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 5,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0005",
                FirstName = "Pedro",
                LastName = "Reyes",
                Email = "pedroreyes@gmail.com",
                ContactNumber = "09171234505",
                CivilStatus = "Single",
                Birthdate = new DateTime(1997, 7, 4, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 600.00m,
                TIN = "123-456-785",
                SSS = "33-1234565-5",
                PhilHealth = "12-345678905-5",
                PagIbig = "1234-5678-9055",
                RoleId = staffRole.RoleId,
                UserId = emp3User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 4 (Staff) - Liza Mendoza
            var emp4User = new User
            {
                UserId = 8,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Liza",
                LastName = "Mendoza",
                Email = "lizamendoza@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp4User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 6,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0006",
                FirstName = "Liza",
                LastName = "Mendoza",
                Email = "lizamendoza@gmail.com",
                ContactNumber = "09171234506",
                CivilStatus = "Single",
                Birthdate = new DateTime(1994, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 620.00m,
                TIN = "123-456-786",
                SSS = "33-1234566-6",
                PhilHealth = "12-345678906-6",
                PagIbig = "1234-5678-9066",
                RoleId = staffRole.RoleId,
                UserId = emp4User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 5 (Staff) - Mark Villanueva
            var emp5User = new User
            {
                UserId = 9,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Mark",
                LastName = "Villanueva",
                Email = "markvillanueva@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp5User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 7,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0007",
                FirstName = "Mark",
                LastName = "Villanueva",
                Email = "markvillanueva@gmail.com",
                ContactNumber = "09171234507",
                CivilStatus = "Married",
                Birthdate = new DateTime(1991, 6, 12, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 650.00m,
                TIN = "123-456-787",
                SSS = "33-1234567-7",
                PhilHealth = "12-345678907-7",
                PagIbig = "1234-5678-9077",
                RoleId = staffRole.RoleId,
                UserId = emp5User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 6 (Staff) - Carmen Lopez
            var emp6User = new User
            {
                UserId = 10,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Carmen",
                LastName = "Lopez",
                Email = "carmenlopez@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp6User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 8,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0008",
                FirstName = "Carmen",
                LastName = "Lopez",
                Email = "carmenlopez@gmail.com",
                ContactNumber = "09171234508",
                CivilStatus = "Single",
                Birthdate = new DateTime(1996, 9, 5, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 600.00m,
                TIN = "123-456-788",
                SSS = "33-1234568-8",
                PhilHealth = "12-345678908-8",
                PagIbig = "1234-5678-9088",
                RoleId = staffRole.RoleId,
                UserId = emp6User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 7 (Staff) - Angelo Ramos
            var emp7User = new User
            {
                UserId = 11,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Angelo",
                LastName = "Ramos",
                Email = "angeloramos@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp7User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 9,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0009",
                FirstName = "Angelo",
                LastName = "Ramos",
                Email = "angeloramos@gmail.com",
                ContactNumber = "09171234509",
                CivilStatus = "Single",
                Birthdate = new DateTime(1993, 12, 18, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 630.00m,
                TIN = "123-456-789",
                SSS = "33-1234569-9",
                PhilHealth = "12-345678909-9",
                PagIbig = "1234-5678-9099",
                RoleId = staffRole.RoleId,
                UserId = emp7User.UserId,
                CreatedAt = seedEmpDate
            });

            // Employee 8 (Staff) - Sofia Tan
            var emp8User = new User
            {
                UserId = 12,
                CompanyId = demoCompany.CompanyId,
                FirstName = "Sofia",
                LastName = "Tan",
                Email = "sofiatan@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seededPassword),
                Role = UserRole.Employee,
                Status = UserStatus.Active,
                CreatedAt = seedEmpDate,
                MustChangePassword = false
            };
            modelBuilder.Entity<User>().HasData(emp8User);
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeNumber = 10,
                CompanyId = demoCompany.CompanyId,
                EmployeeCode = "1001-0010",
                FirstName = "Sofia",
                LastName = "Tan",
                Email = "sofiatan@gmail.com",
                ContactNumber = "09171234510",
                CivilStatus = "Married",
                Birthdate = new DateTime(1998, 4, 25, 0, 0, 0, DateTimeKind.Utc),
                StartDate = seedEmpDate,
                Status = EmployeeStatus.Active,
                SalaryRate = 610.00m,
                TIN = "123-456-790",
                SSS = "33-1234570-0",
                PhilHealth = "12-345678910-0",
                PagIbig = "1234-5678-9100",
                RoleId = staffRole.RoleId,
                UserId = emp8User.UserId,
                CreatedAt = seedEmpDate
            });

            // Seed QR codes for all employees
            modelBuilder.Entity<EmployeeQrCode>().HasData(
                new EmployeeQrCode { QrId = 1, EmployeeId = 1, QrValue = "1001-0001", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 2, EmployeeId = 2, QrValue = "1001-0002", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 3, EmployeeId = 3, QrValue = "1001-0003", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 4, EmployeeId = 4, QrValue = "1001-0004", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 5, EmployeeId = 5, QrValue = "1001-0005", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 6, EmployeeId = 6, QrValue = "1001-0006", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 7, EmployeeId = 7, QrValue = "1001-0007", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 8, EmployeeId = 8, QrValue = "1001-0008", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 9, EmployeeId = 9, QrValue = "1001-0009", IssuedAt = seedEmpDate, IsActive = true },
                new EmployeeQrCode { QrId = 10, EmployeeId = 10, QrValue = "1001-0010", IssuedAt = seedEmpDate, IsActive = true }
            );

            // Seed CompanySettings for demo company  
            modelBuilder.Entity<CompanySetting>().HasData(new CompanySetting
            {
                CompanyId = demoCompany.CompanyId,
                PayrollCycle = PayrollCycle.SemiMonthly,
                WorkHoursPerDay = 8.0m,
                OvertimeRate = 1.25m,
                LateGraceMinutes = 15,
                HolidayRate = 2.00m,
                ScheduledTimeIn = new TimeSpan(8, 0, 0),
                ScheduledTimeOut = new TimeSpan(17, 0, 0),
                VacationLeaveDays = 15,
                VacationLeaveResetType = 1, // Yearly
                CreatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // Seed 12-month subscription for Demo Company (Dency Samson)
            modelBuilder.Entity<Subscription>().HasData(new Subscription
            {
                SubscriptionId = 1,
                CompanyId = demoCompany.CompanyId,
                PlanId = 1, // Basic plan
                StartDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                Status = SubscriptionStatus.Active,
                AutoRenew = false,
                GracePeriodDays = 7,
                CreatedAt = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            });

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

            // Seed default Role Permissions
            // Module names MUST match MenuConfiguration titles exactly
            var managementModules = new[] { "Employee Management", "Attendance Monitoring", "Leave Management", "Salary Computation", "Tax & Contributions", "Finance", "Compensation", "Payslip", "Billing", "Company Settings", "Archives", "Audit Logs" };
            var employeeModules = new[] { "My Attendance", "Leave Request", "My Payslips", "My Contributions", "My QR Code" };
            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int permId = 1;

            // Admin: full access to all management modules
            foreach (var m in managementModules)
            {
                modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                {
                    PermissionId = permId++, RoleName = "Admin", ModuleName = m,
                    CanAdd = true, CanUpdate = true, CanInactivate = true, CreatedAt = seedDate
                });
            }

            // HR: Employee Management, Attendance, Leave Management
            var hrModules = new Dictionary<string, (bool add, bool upd, bool inact)>
            {
                ["Employee Management"] = (true, true, true),
                ["Attendance Monitoring"] = (true, true, false),
                ["Leave Management"] = (true, true, false)
            };
            foreach (var m in managementModules)
            {
                hrModules.TryGetValue(m, out var perms);
                modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                {
                    PermissionId = permId++, RoleName = "HR", ModuleName = m,
                    CanAdd = perms.add, CanUpdate = perms.upd, CanInactivate = perms.inact, CreatedAt = seedDate
                });
            }

            // Accountant: Salary, Tax, Finance, Compensation, Payslip
            var acctModules = new Dictionary<string, (bool add, bool upd, bool inact)>
            {
                ["Salary Computation"] = (true, true, true),
                ["Tax & Contributions"] = (true, true, true),
                ["Finance"] = (true, true, true),
                ["Compensation"] = (true, true, true),
                ["Payslip"] = (true, true, false)
            };
            foreach (var m in managementModules)
            {
                acctModules.TryGetValue(m, out var perms);
                modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                {
                    PermissionId = permId++, RoleName = "Accountant", ModuleName = m,
                    CanAdd = perms.add, CanUpdate = perms.upd, CanInactivate = perms.inact, CreatedAt = seedDate
                });
            }

            // Employee: no access to management modules
            foreach (var m in managementModules)
            {
                modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                {
                    PermissionId = permId++, RoleName = "Employee", ModuleName = m,
                    CanAdd = false, CanUpdate = false, CanInactivate = false, CreatedAt = seedDate
                });
            }

            // Employee-view modules: all roles with employee view get access
            var employeeViewRoles = new[] { "Employee", "HR", "Accountant" };
            foreach (var role in employeeViewRoles)
            {
                foreach (var m in employeeModules)
                {
                    modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                    {
                        PermissionId = permId++, RoleName = role, ModuleName = m,
                        CanAdd = true, CanUpdate = true, CanInactivate = false, CreatedAt = seedDate
                    });
                }
            }
        }
    }
}