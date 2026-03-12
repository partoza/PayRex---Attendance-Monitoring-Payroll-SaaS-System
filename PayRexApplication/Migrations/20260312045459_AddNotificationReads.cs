using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificationReads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    notificationId = table.Column<int>(type: "int", nullable: false),
                    readAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificationReads", x => x.id);
                    table.ForeignKey(
                        name: "FK_notificationReads_systemNotifications_notificationId",
                        column: x => x.notificationId,
                        principalTable: "systemNotifications",
                        principalColumn: "notificationId",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_notificationReads_notificationId",
                table: "notificationReads",
                column: "notificationId");

            migrationBuilder.CreateIndex(
                name: "IX_notificationReads_userId_notificationId",
                table: "notificationReads",
                columns: new[] { "userId", "notificationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificationReads");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$c27gKASdKqIPWmlfQrXsT.pXkVcSrXuTqTj0JNgQ2DOcOeFeulnsG");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$q7mDt6L1cPKGEsTdvBeOg.LX60d8OjvOCXL1Hbxeci6H/CpQUWgRq");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$pBg7urKMdYe9SBYeS964Ye.45XU8ReC0p.ckwGOelSYiytwfeWt8K");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$Ezff1rB9ZQRC3fsH.ZAvb.IQQGPa.ai/gMqCCZIaRM3bevkeKsGiC");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$TdMse77uYnyt.eNfZ4pe7OtwqUYFG/GMa2DJghaVkhxxDSNG0ziHK");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$gsprHmEix9gGKg.h46e4/.FyLtVhHG74mzjC9rM43JvbJCKs5w.yC");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$Sm7ZP4X6O.Kj3mJP3pOFwO.hsb0FdxUPH0PJuMh.WYwXwRWMBIG7u");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$SkfEpTHvxqIaCBHx3U7Xwuk6eBxgCAWyQWlWM0S4pfwaoqSOKrgjO");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$8W7wHu69Ohc3aSrbXq5s9e0//8DG2w7rCZfjOHC7XU7NNoj5Og11K");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$GAMeahyisUA9sOlD1z9XzuHZVx.MWRCucGVL1j2ijtaaGEwuHNYO.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$h61AxO0oBa.vniaTSAlbiec7QBDuLaL2EoHRBbWpcG08A6t8l9vs.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$KMp3nbLq2dFY9Gs36bS5yerSy68gPg9WvA0vdpZa/xoovDzM6duC6");
        }
    }
}
