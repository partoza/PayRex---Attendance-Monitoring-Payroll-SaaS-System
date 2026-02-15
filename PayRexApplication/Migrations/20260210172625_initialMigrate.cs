using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class initialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscriptionPlans",
                columns: table => new
                {
                    planId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    maxEmployees = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    billingCycle = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    planUserLimit = table.Column<int>(type: "int", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptionPlans", x => x.planId);
                });

            migrationBuilder.CreateTable(
                name: "systemSettings",
                columns: table => new
                {
                    settingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sssPercentage = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    pagIbigPercentage = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    philHealthPercentage = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    effectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systemSettings", x => x.settingId);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    companyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    planId = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.companyId);
                    table.ForeignKey(
                        name: "FK_companies_subscriptionPlans_planId",
                        column: x => x.planId,
                        principalTable: "subscriptionPlans",
                        principalColumn: "planId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "billingInvoices",
                columns: table => new
                {
                    invoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    invoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billingInvoices", x => x.invoiceId);
                    table.ForeignKey(
                        name: "FK_billingInvoices_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companySettings",
                columns: table => new
                {
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    payrollCycle = table.Column<int>(type: "int", nullable: false),
                    workHoursPerDay = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    overtimeRate = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    lateGraceMinutes = table.Column<int>(type: "int", nullable: false),
                    nightDifferentialRate = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    holidayRate = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companySettings", x => x.companyId);
                    table.ForeignKey(
                        name: "FK_companySettings_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    employeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    employeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    salaryRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    employmentType = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    phoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    dateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.employeeId);
                    table.ForeignKey(
                        name: "FK_employees_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payrollPeriods",
                columns: table => new
                {
                    payrollPeriodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    startDate = table.Column<DateOnly>(type: "date", nullable: false),
                    endDate = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    periodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrollPeriods", x => x.payrollPeriodId);
                    table.ForeignKey(
                        name: "FK_payrollPeriods_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    subscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    planId = table.Column<int>(type: "int", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.subscriptionId);
                    table.ForeignKey(
                        name: "FK_subscriptions_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_subscriptions_subscriptionPlans_planId",
                        column: x => x.planId,
                        principalTable: "subscriptionPlans",
                        principalColumn: "planId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    role = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    profileImageUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    isTwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    totpSecretKey = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    recoveryCodesHash = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    passwordResetTokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    passwordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastPasswordChangeAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    mustChangePassword = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userId);
                    table.ForeignKey(
                        name: "FK_users_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    paymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoiceId = table.Column<int>(type: "int", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    referenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    paidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.paymentId);
                    table.ForeignKey(
                        name: "FK_payments_billingInvoices_invoiceId",
                        column: x => x.invoiceId,
                        principalTable: "billingInvoices",
                        principalColumn: "invoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendanceRecords",
                columns: table => new
                {
                    attendanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    timeIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    timeOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    source = table.Column<int>(type: "int", nullable: false),
                    locked = table.Column<bool>(type: "bit", nullable: false),
                    totalHoursWorked = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    overtimeHours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    lateMinutes = table.Column<int>(type: "int", nullable: true),
                    undertimeMinutes = table.Column<int>(type: "int", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendanceRecords", x => x.attendanceId);
                    table.ForeignKey(
                        name: "FK_attendanceRecords_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId");
                    table.ForeignKey(
                        name: "FK_attendanceRecords_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attendanceScans",
                columns: table => new
                {
                    scanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    scanTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    scanType = table.Column<int>(type: "int", nullable: false),
                    deviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    result = table.Column<int>(type: "int", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendanceScans", x => x.scanId);
                    table.ForeignKey(
                        name: "FK_attendanceScans_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employeeBenefits",
                columns: table => new
                {
                    benefitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    isTaxable = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employeeBenefits", x => x.benefitId);
                    table.ForeignKey(
                        name: "FK_employeeBenefits_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employeeDeductions",
                columns: table => new
                {
                    deductionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    recurring = table.Column<bool>(type: "bit", nullable: false),
                    startDate = table.Column<DateOnly>(type: "date", nullable: true),
                    endDate = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employeeDeductions", x => x.deductionId);
                    table.ForeignKey(
                        name: "FK_employeeDeductions_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employeeQrCodes",
                columns: table => new
                {
                    qrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    qrValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    issuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    expiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employeeQrCodes", x => x.qrId);
                    table.ForeignKey(
                        name: "FK_employeeQrCodes_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "governmentContributions",
                columns: table => new
                {
                    contributionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    payrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    employeeShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    employerShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_governmentContributions", x => x.contributionId);
                    table.ForeignKey(
                        name: "FK_governmentContributions_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_governmentContributions_payrollPeriods_payrollPeriodId",
                        column: x => x.payrollPeriodId,
                        principalTable: "payrollPeriods",
                        principalColumn: "payrollPeriodId");
                });

            migrationBuilder.CreateTable(
                name: "payrollSummaries",
                columns: table => new
                {
                    payrollSummaryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    grossPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    totalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    netPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    basicPay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    overtimePay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    holidayPay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    allowances = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrollSummaries", x => x.payrollSummaryId);
                    table.ForeignKey(
                        name: "FK_payrollSummaries_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeId");
                    table.ForeignKey(
                        name: "FK_payrollSummaries_payrollPeriods_payrollPeriodId",
                        column: x => x.payrollPeriodId,
                        principalTable: "payrollPeriods",
                        principalColumn: "payrollPeriodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auditLogs",
                columns: table => new
                {
                    auditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    userId = table.Column<int>(type: "int", nullable: true),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entityAffected = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    target = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    targetId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    oldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    newValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ipAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    userAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditLogs", x => x.auditId);
                    table.ForeignKey(
                        name: "FK_auditLogs_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId");
                    table.ForeignKey(
                        name: "FK_auditLogs_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "payrollApprovals",
                columns: table => new
                {
                    approvalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    approvedBy = table.Column<int>(type: "int", nullable: false),
                    approvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payrollApprovals", x => x.approvalId);
                    table.ForeignKey(
                        name: "FK_payrollApprovals_payrollPeriods_payrollPeriodId",
                        column: x => x.payrollPeriodId,
                        principalTable: "payrollPeriods",
                        principalColumn: "payrollPeriodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payrollApprovals_users_approvedBy",
                        column: x => x.approvedBy,
                        principalTable: "users",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "userLoginAttempts",
                columns: table => new
                {
                    attemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: true),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ipAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    success = table.Column<bool>(type: "bit", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    userAgent = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    attemptCount = table.Column<int>(type: "int", nullable: false),
                    isLocked = table.Column<bool>(type: "bit", nullable: false),
                    lockUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userLoginAttempts", x => x.attemptId);
                    table.ForeignKey(
                        name: "FK_userLoginAttempts_users_userId",
                        column: x => x.userId,
                        principalTable: "users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payslips",
                columns: table => new
                {
                    payslipId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payrollSummaryId = table.Column<int>(type: "int", nullable: false),
                    pdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    generatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    released = table.Column<bool>(type: "bit", nullable: false),
                    releasedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payslips", x => x.payslipId);
                    table.ForeignKey(
                        name: "FK_payslips_payrollSummaries_payrollSummaryId",
                        column: x => x.payrollSummaryId,
                        principalTable: "payrollSummaries",
                        principalColumn: "payrollSummaryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "subscriptionPlans",
                columns: new[] { "planId", "billingCycle", "createdAt", "description", "maxEmployees", "name", "planUserLimit", "price", "status", "updatedAt" },
                values: new object[,]
                {
                    { 1, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "For small to medium Filipino businesses", 50, "Basic", 3, 2499.00m, 0, null },
                    { 2, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "For growing Philippine enterprises", 200, "Pro", 10, 4999.00m, 0, null },
                    { 3, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unlimited employees with dedicated support", 10000, "Enterprise", null, 9999.00m, 0, null }
                });

            migrationBuilder.InsertData(
                table: "systemSettings",
                columns: new[] { "settingId", "createdAt", "effectiveDate", "note", "pagIbigPercentage", "philHealthPercentage", "sssPercentage", "updatedAt" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Initial Philippine government contribution rates for 2025", 2.0m, 2.25m, 4.5m, null });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "companyId", "companyName", "createdAt", "isActive", "planId", "status", "updatedAt" },
                values: new object[,]
                {
                    { "0000", "PayRex System", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 0, null },
                    { "1001", "Demo Company", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, 0, null }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "userId", "companyId", "createdAt", "email", "firstName", "isTwoFactorEnabled", "lastName", "lastPasswordChangeAt", "passwordHash", "passwordResetTokenExpiry", "passwordResetTokenHash", "profileImageUrl", "recoveryCodesHash", "role", "status", "totpSecretKey", "updatedAt" },
                values: new object[] { 1, "0000", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "partozajohnrex@gmail.com", "John Rex", true, "Partoza", null, "$2a$11$8YbcvCZCc9Dg8fFQAX6.IencgY7Qr8I4JK7pPQ5X3SlTY3KgRAHe6", null, null, null, null, 0, 0, "JBSWY3DPEHPK3PXP", null });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "userId", "companyId", "createdAt", "email", "firstName", "lastName", "lastPasswordChangeAt", "passwordHash", "passwordResetTokenExpiry", "passwordResetTokenHash", "profileImageUrl", "recoveryCodesHash", "role", "status", "totpSecretKey", "updatedAt" },
                values: new object[,]
                {
                    { 2, "1001", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alice.admin@example.com", "Alice", "Admin", null, "$2a$11$TqOB31ARbW3mi2V4qgsZbOiNq3iGwfArimcWUfYFz/tknRWIgYQJe", null, null, null, null, 1, 0, null, null },
                    { 3, "1001", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hannah.hr@example.com", "Hannah", "HR", null, "$2a$11$KXrNfrM5504u3Y2ennjFS.oD3y6Vs5ozC8hMJQIrTAvrz4rYgVcvS", null, null, null, null, 2, 0, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendanceRecords_companyId",
                table: "attendanceRecords",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_attendanceRecords_employeeId",
                table: "attendanceRecords",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_attendanceRecords_employeeId_date",
                table: "attendanceRecords",
                columns: new[] { "employeeId", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendanceScans_employeeId",
                table: "attendanceScans",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_attendanceScans_scanTime",
                table: "attendanceScans",
                column: "scanTime");

            migrationBuilder.CreateIndex(
                name: "IX_auditLogs_action",
                table: "auditLogs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_auditLogs_companyId",
                table: "auditLogs",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_auditLogs_createdAt",
                table: "auditLogs",
                column: "createdAt");

            migrationBuilder.CreateIndex(
                name: "IX_auditLogs_userId",
                table: "auditLogs",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_billingInvoices_companyId",
                table: "billingInvoices",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_billingInvoices_invoiceNumber",
                table: "billingInvoices",
                column: "invoiceNumber",
                unique: true,
                filter: "[invoiceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_companies_companyName",
                table: "companies",
                column: "companyName");

            migrationBuilder.CreateIndex(
                name: "IX_companies_planId",
                table: "companies",
                column: "planId");

            migrationBuilder.CreateIndex(
                name: "IX_employeeBenefits_employeeId",
                table: "employeeBenefits",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_employeeDeductions_employeeId",
                table: "employeeDeductions",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_employeeQrCodes_employeeId",
                table: "employeeQrCodes",
                column: "employeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employeeQrCodes_qrValue",
                table: "employeeQrCodes",
                column: "qrValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_companyId",
                table: "employees",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_companyId_employeeCode",
                table: "employees",
                columns: new[] { "companyId", "employeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_governmentContributions_employeeId",
                table: "governmentContributions",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_governmentContributions_employeeId_payrollPeriodId_type",
                table: "governmentContributions",
                columns: new[] { "employeeId", "payrollPeriodId", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_governmentContributions_payrollPeriodId",
                table: "governmentContributions",
                column: "payrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_invoiceId",
                table: "payments",
                column: "invoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_referenceNo",
                table: "payments",
                column: "referenceNo");

            migrationBuilder.CreateIndex(
                name: "IX_payrollApprovals_approvedBy",
                table: "payrollApprovals",
                column: "approvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_payrollApprovals_payrollPeriodId",
                table: "payrollApprovals",
                column: "payrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_payrollPeriods_companyId",
                table: "payrollPeriods",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_payrollPeriods_companyId_startDate_endDate",
                table: "payrollPeriods",
                columns: new[] { "companyId", "startDate", "endDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payrollSummaries_employeeId",
                table: "payrollSummaries",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_payrollSummaries_payrollPeriodId",
                table: "payrollSummaries",
                column: "payrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_payrollSummaries_payrollPeriodId_employeeId",
                table: "payrollSummaries",
                columns: new[] { "payrollPeriodId", "employeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payslips_payrollSummaryId",
                table: "payslips",
                column: "payrollSummaryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptionPlans_name",
                table: "subscriptionPlans",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_companyId",
                table: "subscriptions",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_planId",
                table: "subscriptions",
                column: "planId");

            migrationBuilder.CreateIndex(
                name: "IX_systemSettings_effectiveDate",
                table: "systemSettings",
                column: "effectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_userLoginAttempts_email",
                table: "userLoginAttempts",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_userLoginAttempts_userId",
                table: "userLoginAttempts",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_users_companyId",
                table: "users",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendanceRecords");

            migrationBuilder.DropTable(
                name: "attendanceScans");

            migrationBuilder.DropTable(
                name: "auditLogs");

            migrationBuilder.DropTable(
                name: "companySettings");

            migrationBuilder.DropTable(
                name: "employeeBenefits");

            migrationBuilder.DropTable(
                name: "employeeDeductions");

            migrationBuilder.DropTable(
                name: "employeeQrCodes");

            migrationBuilder.DropTable(
                name: "governmentContributions");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "payrollApprovals");

            migrationBuilder.DropTable(
                name: "payslips");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "systemSettings");

            migrationBuilder.DropTable(
                name: "userLoginAttempts");

            migrationBuilder.DropTable(
                name: "billingInvoices");

            migrationBuilder.DropTable(
                name: "payrollSummaries");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "payrollPeriods");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "subscriptionPlans");
        }
    }
}
