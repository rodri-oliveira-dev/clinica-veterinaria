using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTutoresPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tutores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    documento = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    situacao = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    inativado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tutores", x => x.id);
                    table.CheckConstraint("ck_tutores_contato_obrigatorio", "email IS NOT NULL OR telefone IS NOT NULL");
                    table.CheckConstraint("ck_tutores_documento_len", "documento IS NULL OR length(documento) = 11");
                    table.CheckConstraint("ck_tutores_id_not_empty", "id <> '00000000-0000-0000-0000-000000000000'::uuid");
                    table.CheckConstraint("ck_tutores_situacao", "situacao IN (1, 2)");
                    table.CheckConstraint("ck_tutores_telefone_len", "telefone IS NULL OR length(telefone) IN (10, 11)");
                    table.CheckConstraint("ck_tutores_tenant_id_not_empty", "tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                });

            migrationBuilder.CreateIndex(
                name: "ix_tutores_tenant_id",
                table: "tutores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tutores_tenant_id_documento",
                table: "tutores",
                columns: new[] { "tenant_id", "documento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tutores");
        }
    }
}
