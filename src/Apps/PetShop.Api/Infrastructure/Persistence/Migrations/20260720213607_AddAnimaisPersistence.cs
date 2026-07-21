using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnimaisPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ak_tutores_tenant_id_id",
                table: "tutores",
                columns: new[] { "tenant_id", "id" });

            migrationBuilder.CreateTable(
                name: "animais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    especie = table.Column<string>(type: "text", nullable: false),
                    raca = table.Column<string>(type: "text", nullable: true),
                    sexo = table.Column<int>(type: "integer", nullable: false),
                    data_de_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    cor_ou_pelagem = table.Column<string>(type: "text", nullable: true),
                    observacao_cadastral = table.Column<string>(type: "text", nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    tutor_responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_animais", x => x.id);
                    table.CheckConstraint("ck_animais_cor_ou_pelagem_not_blank", "cor_ou_pelagem IS NULL OR length(btrim(cor_ou_pelagem)) > 0");
                    table.CheckConstraint("ck_animais_especie_not_blank", "length(btrim(especie)) > 0");
                    table.CheckConstraint("ck_animais_id_not_empty", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_animais_nome_not_blank", "length(btrim(nome)) > 0");
                    table.CheckConstraint("ck_animais_observacao_cadastral_not_blank", "observacao_cadastral IS NULL OR length(btrim(observacao_cadastral)) > 0");
                    table.CheckConstraint("ck_animais_raca_not_blank", "raca IS NULL OR length(btrim(raca)) > 0");
                    table.CheckConstraint("ck_animais_sexo", "sexo IN (0, 1, 2)");
                    table.CheckConstraint("ck_animais_situacao", "situacao IN (1, 2)");
                    table.CheckConstraint("ck_animais_tenant_id_not_empty", "tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_animais_tutor_responsavel_id_not_empty", "tutor_responsavel_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.ForeignKey(
                        name: "fk_animais_tutores_tenant_id_tutor_responsavel_id",
                        columns: x => new { x.tenant_id, x.tutor_responsavel_id },
                        principalTable: "tutores",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_animais_tenant_id",
                table: "animais",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_animais_tenant_id_tutor_responsavel_id",
                table: "animais",
                columns: new[] { "tenant_id", "tutor_responsavel_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "animais");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_tutores_tenant_id_id",
                table: "tutores");
        }
    }
}
