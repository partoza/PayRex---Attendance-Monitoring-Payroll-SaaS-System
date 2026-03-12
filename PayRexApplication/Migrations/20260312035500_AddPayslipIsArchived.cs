using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPayslipIsArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "archivedAt",
                table: "payslips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isArchived",
                table: "payslips",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "archivedAt",
                table: "payslips");

            migrationBuilder.DropColumn(
                name: "isArchived",
                table: "payslips");

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
        }
    }
}
