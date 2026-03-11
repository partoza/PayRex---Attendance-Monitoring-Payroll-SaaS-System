using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIsSetupComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isSetupComplete",
                table: "companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "companies",
                keyColumn: "companyId",
                keyValue: 1,
                column: "isSetupComplete",
                value: false);

            migrationBuilder.UpdateData(
                table: "companies",
                keyColumn: "companyId",
                keyValue: 2,
                column: "isSetupComplete",
                value: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$e6rRj3dd0vtofB1PV33reu3syvN9uACLF/VT4S17TtnJRf7srLMaC");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$s2CUc8MfRdcomLF5M.IAY.rLuOFyYkGH.RurhEkc3BDhsR5X2qlG.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$fED8kFnqmj2DOCU.wSV/gOeNp.RsqfuMXv3XVdOwQScygtKPG5zLq");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$WbtiIO0HLew8SHe71JpL6OC4gyESbDzXHmXMU7E14aL5e5PH91/uq");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$rXI080Fb9pEiRuiqlsWy8./dNF/cBEJMNEdRw4Kar.XZ19rAb4j2W");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$9/KNdfISTJGyWHMIo5wfxOrJvS.2yjnsBgSi7.SWeffh4aJEiu9BG");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$stD3cuk55CB1j54QAcEbPeWrdsu05YqJvtQJKRQj4A0rKjtpCKglC");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$6DZ.Ewl4rzAJmJjFt083S.Vzbz5KczfF2FvyNbVtdLVFFf1Aa3Hbu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$lqEpN0p/ZiIcr3KXj5ZSN.wE6KPHozDykL0fqxokSowRgCfJHDVDS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$xOv/BJ7GAwcLCtN.Y5aug.r8H4hsspdO0inKENueESRSyk9x34m7i");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$yCjZg5cz3h0ybzu1w/yrOeraih4s0IoOjHYR7TempEpLNJpmKZVAy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$fYZtKGiKLB3QLcFTKILlN.yOkqG/1A00O/d8uTycU3Fgh6vSg329e");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isSetupComplete",
                table: "companies");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$oOHXuXRiQXOUfDn8aFBHd.SyEFHP8zHdPkE9LFuSLBeHGvd1JZKKy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$cgknj8sK8L8ZFmhjQcaW8.FqIbbnMo8u.eGRDdH2IY5ER4G2tXjY2");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$AP9qGaQOgInUDOd5gwnh5uZ1EtMMImRBWeAHVYlh7Hv1bpwo3UivK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$xmX0u0WK8U3r4bPPqnc3VuWpBYMaE4N0qyNhXRfeUoQU4vUTOpXuK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$5xMWVKq.jx7cQ2dZV793J.WLTuXLsDHR4UPo/6CYdzNUkegCSvwSu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$ikPXHuzDrP4Lk7vEghhRfOOKZf3MYlBvuBBa8Wp5e.aXNBgZ8VewO");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$tj2iNJQUUQuAtDftSEYfIuNqHY6jJqn2X/LlVeSmXPMr2uVEDq53u");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$AoJkB5bMIxytlvTFf.fJ9um9dAX8qZ0RdwK9Qi17vsrt.s7u7mftW");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$0muRUrYbPAmJQThvlsdc7.OWGugeEsY3B9Y5gf6lDaBPqsw/UZr5m");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$VkoRwSlXrmlJoZPpOKn2lONpdvHo4uNg58wGyiAbx/M3z4sjks08G");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$vFrGMdbo3St.xlnMHmEXLeaEpx9SbW80SqrcITHdWaKV2pLxUza.6");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$ek/PZ9nUTHBobxeJ4u7Gaui7EXmP/kCvh22iFMqqf7X8q2YUphmBK");
        }
    }
}
