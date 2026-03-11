using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayRex.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeInCutoffHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "timeInCutoffHours",
                table: "companySettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "companySettings",
                keyColumn: "companyId",
                keyValue: 2,
                column: "timeInCutoffHours",
                value: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timeInCutoffHours",
                table: "companySettings");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 1,
                column: "passwordHash",
                value: "$2a$11$4/Be9fB.kAu0aNQJj1z/te5oCLNZzmo8HLgse9B4vW7r9QgoIB.te");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 2,
                column: "passwordHash",
                value: "$2a$11$wncMZ6v.EMamgCDqlLWHYOWDTVagC/umOxet9TETnturZpN76Umii");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 3,
                column: "passwordHash",
                value: "$2a$11$4VXDIRMmp645NR/.yckSQuRwGhJSIC71X3/huI56g0n2nI7IBCqpa");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 4,
                column: "passwordHash",
                value: "$2a$11$kaXYPZ0pc4WTuFNDMMxOu.GIZYPMDY2RKwNxDUDGpa2Ls9dde8pRu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 5,
                column: "passwordHash",
                value: "$2a$11$1ct3nl5djmiXbHOnJclrJuGYSYVaka.CQYTRncWW0cPfhpsfYK8zm");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 6,
                column: "passwordHash",
                value: "$2a$11$84dy53jznpdOAdY7E2gLZeDuEsJM25l3bqSiUzy8MDn/KjDcxoeGm");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 7,
                column: "passwordHash",
                value: "$2a$11$BGdIBqAEPZw6/Wr2zkssWehpMybqWTSUA285Y0UL7Ks3tMyrES6J.");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 8,
                column: "passwordHash",
                value: "$2a$11$ySmzbRInLrVuGghLg5AtLuC6W9CgvugzCsC7wAd2y4VRowfkm.Vgu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 9,
                column: "passwordHash",
                value: "$2a$11$51JNfW/Y2BdyHJXEtOeyj.vCoMQZMH4P/HxPQ4STRxXYkAZN4MEqW");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 10,
                column: "passwordHash",
                value: "$2a$11$d/0mN0YErsFnphLHTT1XKeeOoTT2dTCkqbhCa1kz/vtHeaROT4gp6");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 11,
                column: "passwordHash",
                value: "$2a$11$rpLD89tIZuYhWpcWZxbEFOAzn2zAO6VpUMNsLKxLTPEWOgej8oEeu");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "userId",
                keyValue: 12,
                column: "passwordHash",
                value: "$2a$11$o8IsQupZxiyV.yonuYXTneVwMWAn6m4s8Ak3QduE/01dj.FBu5HnW");
        }
    }
}
