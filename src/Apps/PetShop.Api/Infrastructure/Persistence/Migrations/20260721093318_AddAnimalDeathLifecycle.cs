using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalDeathLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_animais_situacao",
                table: "animais");

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_do_falecimento",
                table: "animais",
                type: "date",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_animais_data_falecimento_situacao",
                table: "animais",
                sql: "(situacao = 3 AND data_do_falecimento IS NOT NULL) OR (situacao <> 3 AND data_do_falecimento IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_animais_situacao",
                table: "animais",
                sql: "situacao IN (1, 2, 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_animais_data_falecimento_situacao",
                table: "animais");

            migrationBuilder.DropCheckConstraint(
                name: "ck_animais_situacao",
                table: "animais");

            migrationBuilder.DropColumn(
                name: "data_do_falecimento",
                table: "animais");

            migrationBuilder.AddCheckConstraint(
                name: "ck_animais_situacao",
                table: "animais",
                sql: "situacao IN (1, 2)");
        }
    }
}
