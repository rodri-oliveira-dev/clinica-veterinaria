using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimalResponsibilityTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "versao",
                table: "animais",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddUniqueConstraint(
                name: "ak_animais_tenant_id_id",
                table: "animais",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateTable(
                name: "historico_transferencias_animais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    animal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tutor_anterior_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tutor_novo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    realizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    subject = table.Column<string>(type: "text", nullable: false),
                    motivo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historico_transferencias_animais", x => x.id);
                    table.CheckConstraint("ck_historico_transferencias_animais_animal_id_not_empty", "animal_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_historico_transferencias_animais_id_not_empty", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_historico_transferencias_animais_motivo_not_blank", "motivo IS NULL OR length(btrim(motivo)) > 0");
                    table.CheckConstraint("ck_historico_transferencias_animais_subject_not_blank", "length(btrim(subject)) > 0");
                    table.CheckConstraint("ck_historico_transferencias_animais_tenant_id_not_empty", "tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_historico_transferencias_animais_tutor_anterior_not_empty", "tutor_anterior_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_historico_transferencias_animais_tutor_novo_not_empty", "tutor_novo_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_historico_transferencias_animais_tutores_diferentes", "tutor_anterior_id <> tutor_novo_id");
                    table.ForeignKey(
                        name: "fk_hist_transf_animais_animais_tenant_id_animal_id",
                        columns: x => new { x.tenant_id, x.animal_id },
                        principalTable: "animais",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hist_transf_animais_tutores_tenant_id_tutor_anterior_id",
                        columns: x => new { x.tenant_id, x.tutor_anterior_id },
                        principalTable: "tutores",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hist_transf_animais_tutores_tenant_id_tutor_novo_id",
                        columns: x => new { x.tenant_id, x.tutor_novo_id },
                        principalTable: "tutores",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hist_transf_animais_tenant_tutor_anterior",
                table: "historico_transferencias_animais",
                columns: new[] { "tenant_id", "tutor_anterior_id" });

            migrationBuilder.CreateIndex(
                name: "ix_hist_transf_animais_tenant_tutor_novo",
                table: "historico_transferencias_animais",
                columns: new[] { "tenant_id", "tutor_novo_id" });

            migrationBuilder.CreateIndex(
                name: "ix_historico_transferencias_animais_tenant_animal_data",
                table: "historico_transferencias_animais",
                columns: new[] { "tenant_id", "animal_id", "realizada_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historico_transferencias_animais");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_animais_tenant_id_id",
                table: "animais");

            migrationBuilder.DropColumn(
                name: "versao",
                table: "animais");
        }
    }
}
