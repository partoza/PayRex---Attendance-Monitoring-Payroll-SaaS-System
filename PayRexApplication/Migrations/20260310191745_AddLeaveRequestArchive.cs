using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveRequestArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isArchived",
                table: "leaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$U0764bCOcE5PLat96cDvVefLE/4XTC.tJjYRq1mrqBtek0MI3ysLG");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$zD.85cLNvaaC6koqINpnW.Wb/fmz5iDVOrou0aUb/DeGQA9Osx9em");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$70uE6RG4C3bbvePB6yRNy.OZuQYZ/JfUNPn5Fns0U0K8AfwcL3elK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$lz.jhX3fCh9NFcV/NFEy1uqFhgoZijB2aWYCwjtX3VFBlBRJKBk6G");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$pYXCyKT/u1SbjIk/riBkSOXXIrVTvzQITRX8OmfD5cfrXqC3qHo9e");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$QfNP/ZEDIrhrBqf4aAkRh.EW.3Y1bgLxVxE9waOf1HwxfsKdz3IQe");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$CR1UIp9P5TtoBSF8r8WtxOGNPwuQJbDz3lpYcJutaIvw/kzUyKQsm");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$t908VdhGr38H0b7U7IS/OOpl2VkapRsOqbhyecnvPkmMj2/XU332a");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$fRsm3oTxHO8lgVpABK6fV.s0HM9uYgjM/A15JbKwHhRZjBu/zpqle");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$Cv7m.BZKYkwrikUl5DpCzupSz8vH1U.jd6j.xE8KYVEIeaVDNr9dm");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$cCYLS0duHprAKJkGgGT9DuJSLvbfSKiriNy7LM/HDmgb4MQtdYNVu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$WFFGIaZyqHKbb0.SFMOnleOJAZvqE7I3OQhnJV2T8DdOCpKSP2z3e");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isArchived",
                table: "leaveRequests");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$NpvuDW1Gk.Fqy37PDTaj4eykwNoyEmrRiH7mZk/aZLYZwEBpK//Bi");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$WxewtUZolfYDKfulee2p4uaEvWEn/Azrc0ewk6yA/4lB/X7lXsYxy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$U9LeumByh1F1dUkqbSIAbODEZ/ixur2hzMnYe1SLyyePdrTE9MH32");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$EYS9EovP9r.DCqzETOZ0MeMVKNkFMKUtrDMxgYUYUADSG6v1UG5bi");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$JwKqOfg/8zplUPB2xy2l8eUMKyMxV5O/NGzZUpaG5CvnxjLnsxyFK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$Z9tdK6F97oXD1s5OKSAXU.ss1t2rTAzm2vk04NgJQa8NHPngqTmAC");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$25ZiicCYRMmquO03.PD4yeHtJ3Nlkuge9Zw6mIlvCZxBVLRFxKSvy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$avMb42swdQfwZyKB.8lxyuRUtbdrd.YZ2Za8Sk7Br8ANdUrwhCBBi");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$g1TQPxGq4AvaRALEfwiDsOeopE97WpWFw.TBrLWIZBFalwCEYDtbW");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$OaF0oIzxzWiUz4JkbO7pOe71vZ./2HUjdF/FMlvlb1QM8avjwN1Pu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$8i0ZipIjlMQ3xX/tKWAIveDjUy6Y2MJ6pbTu7jrqpJJGW.rdPeg0i");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$FlTymv8DBvBFiTVGcZ/lNu4veIkIPJgwzQtY2jRirKx2jw165GMs2");
        }
    }
}
