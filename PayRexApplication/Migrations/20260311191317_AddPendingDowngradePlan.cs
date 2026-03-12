using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingDowngradePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "pendingDowngradePlanId",
                table: "subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "subscriptionId",
                keyValue: 1,
                column: "pendingDowngradePlanId",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$y7SfuvoreergeX4oWsLBZe16PYHzArkNaH5TqPx0qF48QcZSuxpnS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$BXZG/2P9fXTVE.Kp2ZT.AOf.6xWZcwZVSrend/CK0X5sDgxkf32V.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$nx.onoowFW4nFUbuNPAQqOtYM2fv87Nf/GOBapq3ssvEsSko0oaIu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$6Ga7ROiKqDDwVIIZ1jK5AO5YrQOlbLU0XRD8gYs/H2LKc8JjsRu42");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$GrV77xhWN56gr60QdChOr.rts0IX/00gJ/2NYzMpoPSXkLzEAiCxS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$UJt8u5r59NMDPr5dkuEIN.n.Z0oPppKTwz/fiazN1fFvgbionwbGO");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$VUtWArkwC5muknGURbA82OSYuhaF1tq4g6P6jBYYi2hmcO/dTaTHG");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$pm0R3ZUJ.zvNoRC.JmHJjOXKCvL9/uu3n0UvMnO7ktSYNnH75eXl.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$pWyoMXY5T67w14IBJXiume0oK3NVKcsjaUJurB6CAuMsqmzpLNUuy");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$huld..YYY76MNwHC5sNXDO3CcOnsqQaD.uNrgdyOFf5v6g3XbXI5G");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$S947.NG14RiokB.aw2EUIe2MFy9bLiZnRWrbcfisJfG9HQH/D3ebS");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$0blReD81NCHge8LshXNtbOIoR9sXjqYVkz48eemrs43vvWBSXMc4G");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_pendingDowngradePlanId",
                table: "subscriptions",
                column: "pendingDowngradePlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_subscriptionPlans_pendingDowngradePlanId",
                table: "subscriptions",
                column: "pendingDowngradePlanId",
                principalTable: "subscriptionPlans",
                principalColumn: "planId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_subscriptionPlans_pendingDowngradePlanId",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_pendingDowngradePlanId",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "pendingDowngradePlanId",
                table: "subscriptions");

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
    }
}
