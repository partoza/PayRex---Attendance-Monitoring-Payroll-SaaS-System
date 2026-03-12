using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToFinanceEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "companyId",
                table: "financeEntries",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 1,
                column: "moduleName",
                value: "Employee Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 2,
                column: "moduleName",
                value: "Attendance Monitoring");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 3,
                column: "moduleName",
                value: "Leave Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 8,
                column: "moduleName",
                value: "Payslip");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 9,
                column: "moduleName",
                value: "Billing");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 10,
                column: "moduleName",
                value: "Company Settings");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 11,
                column: "moduleName",
                value: "Archives");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 12,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName", "roleName" },
                values: new object[] { true, true, true, "Audit Logs", "Admin" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 14,
                column: "moduleName",
                value: "Attendance Monitoring");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 15,
                columns: new[] { "canAdd", "canUpdate", "moduleName" },
                values: new object[] { true, true, "Leave Management" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 16,
                column: "moduleName",
                value: "Salary Computation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 17,
                column: "moduleName",
                value: "Tax & Contributions");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 18,
                column: "moduleName",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 19,
                column: "moduleName",
                value: "Compensation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 20,
                column: "moduleName",
                value: "Payslip");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 21,
                column: "moduleName",
                value: "Billing");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 22,
                column: "moduleName",
                value: "Company Settings");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 23,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Archives", "HR" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 24,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Audit Logs", "HR" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 25,
                column: "moduleName",
                value: "Employee Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 26,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName" },
                values: new object[] { false, false, false, "Attendance Monitoring" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 27,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName" },
                values: new object[] { false, false, false, "Leave Management" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 28,
                column: "moduleName",
                value: "Salary Computation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 29,
                column: "moduleName",
                value: "Tax & Contributions");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 30,
                columns: new[] { "canInactivate", "moduleName" },
                values: new object[] { true, "Finance" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 31,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName" },
                values: new object[] { true, true, true, "Compensation" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 32,
                columns: new[] { "canAdd", "canUpdate", "moduleName" },
                values: new object[] { true, true, "Payslip" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 33,
                column: "moduleName",
                value: "Billing");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 34,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Company Settings", "Accountant" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 35,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Archives", "Accountant" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 36,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Audit Logs", "Accountant" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 37,
                column: "moduleName",
                value: "Employee Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 38,
                column: "moduleName",
                value: "Attendance Monitoring");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 39,
                column: "moduleName",
                value: "Leave Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 40,
                column: "moduleName",
                value: "Salary Computation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 41,
                column: "moduleName",
                value: "Tax & Contributions");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 42,
                column: "moduleName",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 43,
                column: "moduleName",
                value: "Compensation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 44,
                column: "moduleName",
                value: "Payslip");

            migrationBuilder.InsertData(
                table: "rolePermissions",
                columns: new[] { "permissionId", "canAdd", "canInactivate", "canUpdate", "createdAt", "moduleName", "roleName", "updatedAt" },
                values: new object[,]
                {
                    { 45, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Billing", "Employee", null },
                    { 46, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Company Settings", "Employee", null },
                    { 47, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Archives", "Employee", null },
                    { 48, false, false, false, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Audit Logs", "Employee", null },
                    { 49, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Attendance", "Employee", null },
                    { 50, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Leave Request", "Employee", null },
                    { 51, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Payslips", "Employee", null },
                    { 52, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Contributions", "Employee", null },
                    { 53, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My QR Code", "Employee", null },
                    { 54, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Attendance", "HR", null },
                    { 55, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Leave Request", "HR", null },
                    { 56, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Payslips", "HR", null },
                    { 57, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Contributions", "HR", null },
                    { 58, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My QR Code", "HR", null },
                    { 59, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Attendance", "Accountant", null },
                    { 60, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Leave Request", "Accountant", null },
                    { 61, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Payslips", "Accountant", null },
                    { 62, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My Contributions", "Accountant", null },
                    { 63, true, false, true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My QR Code", "Accountant", null }
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$SaOp5BENZcleUSrpg/EGCO/LWIfWrt7L33Fuf4bYEiME65lVKBz1S");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$Uju9rbj9tjxvsf9XRAhmQe5R63nmKqSbq5P6j3hAJId3tURIPhxuy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$QPNVrdZTcmfSCMMTAztZM.yXR1aBCg51SsFoq.S/DhOtm/Y5EC7Yy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$ZdgAqSS0mpGd/MifhkJO0uZf4FfHr93I2crDmqsvkscrQGF5R8iFi");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$0dTKPdKHMXjqmcsa/bMA..z20IbvrxiiA4/JleFIa8TZCTsMLaF..");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$AXpMtGTiYZgBSuylAtm8iOfcWijBOh4cQOYNAgJ5bv7h2wzkLp/dK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$IcWLaTqbmRGbewR9SX8/BuzQkbV9Ed/N/f3EoQqvjH3jXvNhNXnAS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$0OTKRWLiuSEU5QQR6/oKTOZPBZG3P8nEJTbekKNppDvcDM6n8VwFm");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$fgebdg8dlGDutJavcdZ7gOknnD4KOpXi9wdzrQ0Z3mhihW9U.lNKa");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$abEvqLSzIT4G4XCzTslgFOiS.aBezLO.scEztp1dSu9OCrLFMj0o.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$atizO/bYmxIaTG5.K3cavuJqT9WuhNo6KDcxV.Ir3RqKeH1HfL1cK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$G0WnQxhm58XXQWbskMcfX.gck7FG1/GmV7YA8JtDtVVkoixOsi5lG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 63);

            migrationBuilder.DropColumn(
                name: "companyId",
                table: "financeEntries");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 1,
                column: "moduleName",
                value: "User Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 2,
                column: "moduleName",
                value: "Employee Management");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 3,
                column: "moduleName",
                value: "Attendance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 8,
                column: "moduleName",
                value: "Payslips");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 9,
                column: "moduleName",
                value: "Company Settings");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 10,
                column: "moduleName",
                value: "Archives");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 11,
                column: "moduleName",
                value: "Audit Logs");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 12,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName", "roleName" },
                values: new object[] { false, false, false, "User Management", "HR" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 14,
                column: "moduleName",
                value: "Attendance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 15,
                columns: new[] { "canAdd", "canUpdate", "moduleName" },
                values: new object[] { false, false, "Salary Computation" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 16,
                column: "moduleName",
                value: "Tax & Contributions");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 17,
                column: "moduleName",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 18,
                column: "moduleName",
                value: "Compensation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 19,
                column: "moduleName",
                value: "Payslips");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 20,
                column: "moduleName",
                value: "Company Settings");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 21,
                column: "moduleName",
                value: "Archives");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 22,
                column: "moduleName",
                value: "Audit Logs");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 23,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "User Management", "Accountant" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 24,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Employee Management", "Accountant" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 25,
                column: "moduleName",
                value: "Attendance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 26,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName" },
                values: new object[] { true, true, true, "Salary Computation" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 27,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName" },
                values: new object[] { true, true, true, "Tax & Contributions" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 28,
                column: "moduleName",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 29,
                column: "moduleName",
                value: "Compensation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 30,
                columns: new[] { "canInactivate", "moduleName" },
                values: new object[] { false, "Payslips" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 31,
                columns: new[] { "canAdd", "canInactivate", "canUpdate", "moduleName" },
                values: new object[] { false, false, false, "Company Settings" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 32,
                columns: new[] { "canAdd", "canUpdate", "moduleName" },
                values: new object[] { false, false, "Archives" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 33,
                column: "moduleName",
                value: "Audit Logs");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 34,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "User Management", "Employee" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 35,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Employee Management", "Employee" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 36,
                columns: new[] { "moduleName", "roleName" },
                values: new object[] { "Attendance", "Employee" });

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 37,
                column: "moduleName",
                value: "Salary Computation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 38,
                column: "moduleName",
                value: "Tax & Contributions");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 39,
                column: "moduleName",
                value: "Finance");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 40,
                column: "moduleName",
                value: "Compensation");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 41,
                column: "moduleName",
                value: "Payslips");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 42,
                column: "moduleName",
                value: "Company Settings");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 43,
                column: "moduleName",
                value: "Archives");

            migrationBuilder.UpdateData(
                table: "rolePermissions",
                keyColumn: "permissionId",
                keyValue: 44,
                column: "moduleName",
                value: "Audit Logs");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$SbR/.MXp/U8Bf0IT12iOIOG2AA3ir16Q879F0UZv.czHT5fgrfqZ2");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$aXMog5vxrjQslGkMBmn7.ue0OKyXki0HOUZZQ6i.2dyHOwwMbKq9a");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$OAvdEhd7V9E4DSx0VkVM1OojZUG5TuwNjoBeA3LZQRNzaN5pUykLW");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$Os1YNGu3ueOPeqxaQkMOgeQ64mCbubV9n4u4qF5CFgjFG4dc.KU4S");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$372rSuLhMvfQ7XvA0Ha6fOExJwhT3wgIDXM1ofOtci3eUi/jy9ZYO");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$oCRnhPvTTsTQcWhKAWIFue8yJkZbLDHvmHcRPwKsc9SLg.7bMExKO");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$aRza3ufOmzM6AkPJYVRzfuiyq/tENdHfRWKJ3Rj6neS/dVQwOOfgK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$WSfOCgtyCKzYrkYFtkRCie/AqxrtikgzOednnvE.Tzfam3h4nMz9e");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$pS6hYNv/Wj4nWiV7nfbKxuxXGdwtJB4kq62uyQnbcfI1rvJtIIPn6");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$nCXsikTTGI9GPSUZFpFdFuTGauyEbEaRQzmVPykAX99Vmi2y.eVVW");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$84hT55/xE40MlxwG2wQfaevIHD4LZsTjBxafdiEtpgvHK1qWjgusC");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$vNRSOLNWl0zZTQH1HWbzV.f8f3DGYsRvY3qF4aImhCJFdxpQnqRzW");
        }
    }
}
