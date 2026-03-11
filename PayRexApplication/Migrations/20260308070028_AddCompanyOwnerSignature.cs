using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyOwnerSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ownerSignatureUrl",
                table: "companies",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "companies",
                keyColumn: "companyId",
                keyValue: 1,
                column: "ownerSignatureUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "companies",
                keyColumn: "companyId",
                keyValue: 2,
                column: "ownerSignatureUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$oZ5jqDa7cS3QtFwpA7fv3Oc7aEZ3wcw5CkH0nbNbMSjm7TIN3elSi");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$600JeAHxIYQQVdWmMhf5AuzOsprnuU3V3vWY6olUW18Q0eMv/b2Ra");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$xAXOPtHOrjlfz1OdB8i0junhkhZ5.dxssqkiAYZKK4ZsxKCvRPDYu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$HZbnk1uB0ENzOEtRrC5xPutxJfDQ6/AJaeAMEnM9dslZW9Vd5mYxu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$or94L3C8Fh5WzkgTJftQ3.H7RgxDeBIFi1VpX2F../OUwk8KIMPl2");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$plgEVq8uUNvEGTENdlhnE.tXm.dEQDVPS0rrL.Xrhq5wyjG37rZv.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$GxSQuNNr4z6SnvCme8WT2ORok/QeuKgROAss1fJgSfNVUf79FvAK2");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$ookKTCsnne.mQgf8dpQzr.mCfKUi4Q3O7BAJwsGZ19zc6IKtDcj7C");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$yFSh8dS5ettid6EQbywyIeRxgTPhRu45PqQL0WY6JhG36CaN4K.KS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$qZssRIHgfhxfNxVHVxbDuu7twM.67MZFuWTJ5MzYj3Xh9gwXbtXlS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$pYUJcL572KicblJ7PzZJ0Ow2iGQ0ljmxZ0XlvgihP547XuXcWF.Qa");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$/ZGwfb.RQiTntFZQOkfsUO5App9DXP4gCP9f6lTJPkairY.Y98pk.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ownerSignatureUrl",
                table: "companies");

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
    }
}
