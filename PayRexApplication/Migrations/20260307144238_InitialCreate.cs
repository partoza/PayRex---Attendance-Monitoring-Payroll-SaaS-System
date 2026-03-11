using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rolePermissions",
                columns: table => new
                {
                    permissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    moduleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    canAdd = table.Column<bool>(type: "bit", nullable: false),
                    canUpdate = table.Column<bool>(type: "bit", nullable: false),
                    canInactivate = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolePermissions", x => x.permissionId);
                });

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
                    companyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    companyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    planId = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    contactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    contactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    tin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    logoUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
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
                name: "companySettings",
                columns: table => new
                {
                    companyId = table.Column<int>(type: "int", nullable: false),
                    payrollCycle = table.Column<int>(type: "int", nullable: false),
                    workHoursPerDay = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    overtimeRate = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    lateGraceMinutes = table.Column<int>(type: "int", nullable: false),
                    holidayRate = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    scheduledTimeIn = table.Column<TimeSpan>(type: "time", nullable: true),
                    scheduledTimeOut = table.Column<TimeSpan>(type: "time", nullable: true),
                    vacationLeaveDays = table.Column<int>(type: "int", nullable: false),
                    vacationLeaveResetType = table.Column<int>(type: "int", nullable: false),
                    rolesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                name: "employeeRoles",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    roleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    basicRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    rateType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    isBuiltIn = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employeeRoles", x => x.roleId);
                    table.ForeignKey(
                        name: "FK_employeeRoles_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenseRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    payee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenseRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expenseRecords_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "incomeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incomeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_incomeRecords_companies_companyId",
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
                    companyId = table.Column<int>(type: "int", nullable: false),
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
                name: "users",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    role = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    profileImageUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    signatureUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                name: "auditLogs",
                columns: table => new
                {
                    auditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: true),
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
                name: "employees",
                columns: table => new
                {
                    employeeNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    employeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    contactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    civilStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    birthdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    salaryRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tin = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    sss = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    philHealth = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    pagIbig = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    roleId = table.Column<int>(type: "int", nullable: true),
                    userId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.employeeNumber);
                    table.ForeignKey(
                        name: "FK_employees_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employees_employeeRoles_roleId",
                        column: x => x.roleId,
                        principalTable: "employeeRoles",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employees_users_userId",
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
                name: "attendanceRecords",
                columns: table => new
                {
                    attendanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    timeIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    timeOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    source = table.Column<int>(type: "int", nullable: false),
                    locked = table.Column<bool>(type: "bit", nullable: false),
                    totalHoursWorked = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    overtimeHours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    lateMinutes = table.Column<int>(type: "int", nullable: true),
                    undertimeMinutes = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    isHoliday = table.Column<bool>(type: "bit", nullable: false),
                    holidayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                        principalColumn: "employeeNumber",
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
                        principalColumn: "employeeNumber",
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
                        principalColumn: "employeeNumber",
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
                        principalColumn: "employeeNumber",
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
                        principalColumn: "employeeNumber",
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
                        principalColumn: "employeeNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_governmentContributions_payrollPeriods_payrollPeriodId",
                        column: x => x.payrollPeriodId,
                        principalTable: "payrollPeriods",
                        principalColumn: "payrollPeriodId");
                });

            migrationBuilder.CreateTable(
                name: "leaveRequests",
                columns: table => new
                {
                    leaveRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employeeId = table.Column<int>(type: "int", nullable: false),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    leaveType = table.Column<int>(type: "int", nullable: false),
                    startDate = table.Column<DateOnly>(type: "date", nullable: false),
                    endDate = table.Column<DateOnly>(type: "date", nullable: false),
                    totalDays = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    reviewedBy = table.Column<int>(type: "int", nullable: true),
                    reviewRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    reviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaveRequests", x => x.leaveRequestId);
                    table.ForeignKey(
                        name: "FK_leaveRequests_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId");
                    table.ForeignKey(
                        name: "FK_leaveRequests_employees_employeeId",
                        column: x => x.employeeId,
                        principalTable: "employees",
                        principalColumn: "employeeNumber");
                    table.ForeignKey(
                        name: "FK_leaveRequests_users_reviewedBy",
                        column: x => x.reviewedBy,
                        principalTable: "users",
                        principalColumn: "userId");
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
                        principalColumn: "employeeNumber");
                    table.ForeignKey(
                        name: "FK_payrollSummaries_payrollPeriods_payrollPeriodId",
                        column: x => x.payrollPeriodId,
                        principalTable: "payrollPeriods",
                        principalColumn: "payrollPeriodId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "billingInvoices",
                columns: table => new
                {
                    invoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    invoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    subscriptionId = table.Column<int>(type: "int", nullable: true),
                    periodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    periodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    paidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billingInvoices", x => x.invoiceId);
                    table.ForeignKey(
                        name: "FK_billingInvoices_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId");
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
                    companyId = table.Column<int>(type: "int", nullable: true),
                    payMongoPaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    payMongoCheckoutSessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    paymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.ForeignKey(
                        name: "FK_payments_companies_companyId",
                        column: x => x.companyId,
                        principalTable: "companies",
                        principalColumn: "companyId");
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    subscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyId = table.Column<int>(type: "int", nullable: false),
                    planId = table.Column<int>(type: "int", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    autoRenew = table.Column<bool>(type: "bit", nullable: false),
                    gracePeriodDays = table.Column<int>(type: "int", nullable: false),
                    cancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastPaymentId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_subscriptions_payments_lastPaymentId",
                        column: x => x.lastPaymentId,
                        principalTable: "payments",
                        principalColumn: "paymentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_subscriptions_subscriptionPlans_planId",
                        column: x => x.planId,
                        principalTable: "subscriptionPlans",
                        principalColumn: "planId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "rolePermissions",
                columns: new[] { "permissionId", "canAdd", "canInactivate", "canUpdate", "createdAt", "moduleName", "roleName", "updatedAt" },
                values: new object[,]
                {
                    { 1, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "User Management", "Admin", null },
                    { 2, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Employee Management", "Admin", null },
                    { 3, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Attendance", "Admin", null },
                    { 4, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salary Computation", "Admin", null },
                    { 5, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tax & Contributions", "Admin", null },
                    { 6, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finance", "Admin", null },
                    { 7, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compensation", "Admin", null },
                    { 8, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Payslips", "Admin", null },
                    { 9, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Company Settings", "Admin", null },
                    { 10, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Archives", "Admin", null },
                    { 11, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Audit Logs", "Admin", null },
                    { 12, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "User Management", "HR", null },
                    { 13, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Employee Management", "HR", null },
                    { 14, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Attendance", "HR", null },
                    { 15, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salary Computation", "HR", null },
                    { 16, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tax & Contributions", "HR", null },
                    { 17, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finance", "HR", null },
                    { 18, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compensation", "HR", null },
                    { 19, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Payslips", "HR", null },
                    { 20, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Company Settings", "HR", null },
                    { 21, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Archives", "HR", null },
                    { 22, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Audit Logs", "HR", null },
                    { 23, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "User Management", "Accountant", null },
                    { 24, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Employee Management", "Accountant", null },
                    { 25, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Attendance", "Accountant", null },
                    { 26, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salary Computation", "Accountant", null },
                    { 27, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tax & Contributions", "Accountant", null },
                    { 28, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finance", "Accountant", null },
                    { 29, true, true, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compensation", "Accountant", null },
                    { 30, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Payslips", "Accountant", null },
                    { 31, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Company Settings", "Accountant", null },
                    { 32, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Archives", "Accountant", null },
                    { 33, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Audit Logs", "Accountant", null },
                    { 34, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "User Management", "Employee", null },
                    { 35, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Employee Management", "Employee", null },
                    { 36, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Attendance", "Employee", null },
                    { 37, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salary Computation", "Employee", null },
                    { 38, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tax & Contributions", "Employee", null },
                    { 39, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Finance", "Employee", null },
                    { 40, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compensation", "Employee", null },
                    { 41, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Payslips", "Employee", null },
                    { 42, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Company Settings", "Employee", null },
                    { 43, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Archives", "Employee", null },
                    { 44, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Audit Logs", "Employee", null }
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
                columns: new[] { "companyId", "address", "companyCode", "companyName", "contactEmail", "contactPhone", "createdAt", "isActive", "logoUrl", "planId", "status", "tin", "updatedAt" },
                values: new object[,]
                {
                    { 1, null, "0000", "PayRex System", null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, 3, 0, null, null },
                    { 2, null, "1001", "Demo Company", null, null, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, 1, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "companySettings",
                columns: new[] { "companyId", "createdAt", "holidayRate", "lateGraceMinutes", "overtimeRate", "payrollCycle", "rolesJson", "scheduledTimeIn", "scheduledTimeOut", "updatedAt", "vacationLeaveDays", "vacationLeaveResetType", "workHoursPerDay" },
                values: new object[] { 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2.00m, 15, 1.25m, 1, null, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 17, 0, 0, 0), null, 15, 1, 8.0m });

            migrationBuilder.InsertData(
                table: "employeeRoles",
                columns: new[] { "roleId", "basicRate", "companyId", "createdAt", "description", "isActive", "isBuiltIn", "rateType", "roleName", "updatedAt" },
                values: new object[,]
                {
                    { 1, null, 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Human Resource Manager - manages employees and attendance", true, true, null, "HR", null },
                    { 2, null, 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Accountant - manages salary, finance, and payslips", true, true, null, "Accountant", null },
                    { 3, 600.00m, 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Regular staff member", true, false, "Daily", "Staff", null }
                });

            migrationBuilder.InsertData(
                table: "subscriptions",
                columns: new[] { "subscriptionId", "autoRenew", "cancelledAt", "companyId", "createdAt", "endDate", "gracePeriodDays", "lastPaymentId", "planId", "startDate", "status", "updatedAt" },
                values: new object[] { 1, false, null, 2, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, null, 1, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "userId", "companyId", "createdAt", "email", "firstName", "isTwoFactorEnabled", "lastName", "lastPasswordChangeAt", "passwordHash", "passwordResetTokenExpiry", "passwordResetTokenHash", "profileImageUrl", "recoveryCodesHash", "role", "signatureUrl", "status", "totpSecretKey", "updatedAt" },
                values: new object[] { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "partozajohnrex@gmail.com", "John Rex", true, "Partoza", null, "$2a$11$oOHXuXRiQXOUfDn8aFBHd.SyEFHP8zHdPkE9LFuSLBeHGvd1JZKKy", null, null, null, null, 0, null, 0, "JBSWY3DPEHPK3PXP", null });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "userId", "companyId", "createdAt", "email", "firstName", "lastName", "lastPasswordChangeAt", "passwordHash", "passwordResetTokenExpiry", "passwordResetTokenHash", "profileImageUrl", "recoveryCodesHash", "role", "signatureUrl", "status", "totpSecretKey", "updatedAt" },
                values: new object[,]
                {
                    { 2, 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "dencysamson@gmail.com", "Dency", "Samson", null, "$2a$11$cgknj8sK8L8ZFmhjQcaW8.FqIbbnMo8u.eGRDdH2IY5ER4G2tXjY2", null, null, null, null, 1, null, 0, null, null },
                    { 3, 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mariasantos@gmail.com", "Maria", "Santos", null, "$2a$11$AP9qGaQOgInUDOd5gwnh5uZ1EtMMImRBWeAHVYlh7Hv1bpwo3UivK", null, null, null, null, 4, null, 0, null, null },
                    { 4, 2, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "anacruz@gmail.com", "Ana", "Cruz", null, "$2a$11$xmX0u0WK8U3r4bPPqnc3VuWpBYMaE4N0qyNhXRfeUoQU4vUTOpXuK", null, null, null, null, 2, null, 0, null, null },
                    { 5, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "juandelacruz@gmail.com", "Juan", "Dela Cruz", null, "$2a$11$5xMWVKq.jx7cQ2dZV793J.WLTuXLsDHR4UPo/6CYdzNUkegCSvwSu", null, null, null, null, 3, null, 0, null, null },
                    { 6, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "rosagarcia@gmail.com", "Rosa", "Garcia", null, "$2a$11$ikPXHuzDrP4Lk7vEghhRfOOKZf3MYlBvuBBa8Wp5e.aXNBgZ8VewO", null, null, null, null, 3, null, 0, null, null },
                    { 7, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "pedroreyes@gmail.com", "Pedro", "Reyes", null, "$2a$11$tj2iNJQUUQuAtDftSEYfIuNqHY6jJqn2X/LlVeSmXPMr2uVEDq53u", null, null, null, null, 3, null, 0, null, null },
                    { 8, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "lizamendoza@gmail.com", "Liza", "Mendoza", null, "$2a$11$AoJkB5bMIxytlvTFf.fJ9um9dAX8qZ0RdwK9Qi17vsrt.s7u7mftW", null, null, null, null, 3, null, 0, null, null },
                    { 9, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "markvillanueva@gmail.com", "Mark", "Villanueva", null, "$2a$11$0muRUrYbPAmJQThvlsdc7.OWGugeEsY3B9Y5gf6lDaBPqsw/UZr5m", null, null, null, null, 3, null, 0, null, null },
                    { 10, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "carmenlopez@gmail.com", "Carmen", "Lopez", null, "$2a$11$VkoRwSlXrmlJoZPpOKn2lONpdvHo4uNg58wGyiAbx/M3z4sjks08G", null, null, null, null, 3, null, 0, null, null },
                    { 11, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "angeloramos@gmail.com", "Angelo", "Ramos", null, "$2a$11$vFrGMdbo3St.xlnMHmEXLeaEpx9SbW80SqrcITHdWaKV2pLxUza.6", null, null, null, null, 3, null, 0, null, null },
                    { 12, 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "sofiatan@gmail.com", "Sofia", "Tan", null, "$2a$11$ek/PZ9nUTHBobxeJ4u7Gaui7EXmP/kCvh22iFMqqf7X8q2YUphmBK", null, null, null, null, 3, null, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "employees",
                columns: new[] { "employeeNumber", "birthdate", "civilStatus", "companyId", "contactNumber", "createdAt", "email", "employeeCode", "firstName", "lastName", "pagIbig", "philHealth", "roleId", "sss", "salaryRate", "startDate", "status", "tin", "updatedAt", "userId" },
                values: new object[,]
                {
                    { 1, new DateTime(1990, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Single", 2, "09171234501", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "anacruz@gmail.com", "1001-0001", "Ana", "Cruz", "1234-5678-9011", "12-345678901-1", 1, "33-1234561-1", 800.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-781", null, 4 },
                    { 2, new DateTime(1988, 8, 22, 0, 0, 0, 0, DateTimeKind.Utc), "Married", 2, "09171234502", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "mariasantos@gmail.com", "1001-0002", "Maria", "Santos", "1234-5678-9022", "12-345678902-2", 2, "33-1234562-2", 750.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-782", null, 3 },
                    { 3, new DateTime(1995, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Single", 2, "09171234503", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "juandelacruz@gmail.com", "1001-0003", "Juan", "Dela Cruz", "1234-5678-9033", "12-345678903-3", 3, "33-1234563-3", 600.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-783", null, 5 },
                    { 4, new DateTime(1992, 11, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Married", 2, "09171234504", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "rosagarcia@gmail.com", "1001-0004", "Rosa", "Garcia", "1234-5678-9044", "12-345678904-4", 3, "33-1234564-4", 600.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-784", null, 6 },
                    { 5, new DateTime(1997, 7, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Single", 2, "09171234505", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "pedroreyes@gmail.com", "1001-0005", "Pedro", "Reyes", "1234-5678-9055", "12-345678905-5", 3, "33-1234565-5", 600.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-785", null, 7 },
                    { 6, new DateTime(1994, 1, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Single", 2, "09171234506", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "lizamendoza@gmail.com", "1001-0006", "Liza", "Mendoza", "1234-5678-9066", "12-345678906-6", 3, "33-1234566-6", 620.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-786", null, 8 },
                    { 7, new DateTime(1991, 6, 12, 0, 0, 0, 0, DateTimeKind.Utc), "Married", 2, "09171234507", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "markvillanueva@gmail.com", "1001-0007", "Mark", "Villanueva", "1234-5678-9077", "12-345678907-7", 3, "33-1234567-7", 650.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-787", null, 9 },
                    { 8, new DateTime(1996, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Single", 2, "09171234508", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "carmenlopez@gmail.com", "1001-0008", "Carmen", "Lopez", "1234-5678-9088", "12-345678908-8", 3, "33-1234568-8", 600.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-788", null, 10 },
                    { 9, new DateTime(1993, 12, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Single", 2, "09171234509", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "angeloramos@gmail.com", "1001-0009", "Angelo", "Ramos", "1234-5678-9099", "12-345678909-9", 3, "33-1234569-9", 630.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-789", null, 11 },
                    { 10, new DateTime(1998, 4, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Married", 2, "09171234510", new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "sofiatan@gmail.com", "1001-0010", "Sofia", "Tan", "1234-5678-9100", "12-345678910-0", 3, "33-1234570-0", 610.00m, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 0, "123-456-790", null, 12 }
                });

            migrationBuilder.InsertData(
                table: "employeeQrCodes",
                columns: new[] { "qrId", "employeeId", "expiresAt", "isActive", "issuedAt", "qrValue" },
                values: new object[,]
                {
                    { 1, 1, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0001" },
                    { 2, 2, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0002" },
                    { 3, 3, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0003" },
                    { 4, 4, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0004" },
                    { 5, 5, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0005" },
                    { 6, 6, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0006" },
                    { 7, 7, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0007" },
                    { 8, 8, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0008" },
                    { 9, 9, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0009" },
                    { 10, 10, null, true, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "1001-0010" }
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
                name: "IX_billingInvoices_subscriptionId",
                table: "billingInvoices",
                column: "subscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_companies_companyCode",
                table: "companies",
                column: "companyCode",
                unique: true);

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
                name: "IX_employeeRoles_companyId",
                table: "employeeRoles",
                column: "companyId");

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
                name: "IX_employees_roleId",
                table: "employees",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_userId",
                table: "employees",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_expenseRecords_companyId",
                table: "expenseRecords",
                column: "companyId");

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
                name: "IX_incomeRecords_companyId",
                table: "incomeRecords",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_leaveRequests_companyId",
                table: "leaveRequests",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_leaveRequests_employeeId",
                table: "leaveRequests",
                column: "employeeId");

            migrationBuilder.CreateIndex(
                name: "IX_leaveRequests_reviewedBy",
                table: "leaveRequests",
                column: "reviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_payments_companyId",
                table: "payments",
                column: "companyId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_invoiceId",
                table: "payments",
                column: "invoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_payMongoCheckoutSessionId",
                table: "payments",
                column: "payMongoCheckoutSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_payMongoPaymentIntentId",
                table: "payments",
                column: "payMongoPaymentIntentId");

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
                name: "IX_subscriptions_lastPaymentId",
                table: "subscriptions",
                column: "lastPaymentId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_billingInvoices_subscriptions_subscriptionId",
                table: "billingInvoices",
                column: "subscriptionId",
                principalTable: "subscriptions",
                principalColumn: "subscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_billingInvoices_companies_companyId",
                table: "billingInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_companies_companyId",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_companies_companyId",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_billingInvoices_subscriptions_subscriptionId",
                table: "billingInvoices");

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
                name: "expenseRecords");

            migrationBuilder.DropTable(
                name: "governmentContributions");

            migrationBuilder.DropTable(
                name: "incomeRecords");

            migrationBuilder.DropTable(
                name: "leaveRequests");

            migrationBuilder.DropTable(
                name: "payrollApprovals");

            migrationBuilder.DropTable(
                name: "payslips");

            migrationBuilder.DropTable(
                name: "rolePermissions");

            migrationBuilder.DropTable(
                name: "systemSettings");

            migrationBuilder.DropTable(
                name: "userLoginAttempts");

            migrationBuilder.DropTable(
                name: "payrollSummaries");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "payrollPeriods");

            migrationBuilder.DropTable(
                name: "employeeRoles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "subscriptionPlans");

            migrationBuilder.DropTable(
                name: "billingInvoices");
        }
    }
}
