using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class TutorEntityTypeConfiguration : IEntityTypeConfiguration<Tutor>
{
    private readonly Expression<Func<Guid?>> _tenantIdAtual;

    public TutorEntityTypeConfiguration(Expression<Func<Guid?>> tenantIdAtual)
    {
        _tenantIdAtual = tenantIdAtual;
    }

    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(
            "tutores",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_tutores_id_not_empty",
                    "id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_tutores_tenant_id_not_empty",
                    "tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_tutores_documento_len",
                    "documento IS NULL OR length(documento) = 11");
                table.HasCheckConstraint(
                    "ck_tutores_telefone_len",
                    "telefone IS NULL OR length(telefone) IN (10, 11)");
                table.HasCheckConstraint(
                    "ck_tutores_situacao",
                    "situacao IN (1, 2)");
                table.HasCheckConstraint(
                    "ck_tutores_contato_obrigatorio",
                    "email IS NOT NULL OR telefone IS NOT NULL");
            });

        builder.HasKey(tutor => tutor.Id);

        builder.Property(tutor => tutor.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Valor,
                valor => TutorId.Criar(valor))
            .ValueGeneratedNever();

        builder.Property(tutor => tutor.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(
                tenantId => tenantId.Valor,
                valor => TenantId.Criar(valor))
            .IsRequired();

        builder.Property(tutor => tutor.Nome)
            .HasColumnName("nome")
            .HasColumnType("text")
            .HasConversion(
                nome => nome.Valor,
                valor => NomeDoTutor.Criar(valor))
            .IsRequired();

        builder.Property(tutor => tutor.Documento)
            .HasColumnName("documento")
            .HasMaxLength(11)
            .HasConversion(
                documento => documento.HasValue ? documento.Value.Valor : null,
                valor => valor == null ? null : Cpf.Criar(valor));

        builder.Property(tutor => tutor.Email)
            .HasColumnName("email")
            .HasColumnType("text")
            .HasConversion(
                email => email.HasValue ? email.Value.Valor : null,
                valor => valor == null ? null : Email.Criar(valor));

        builder.Property(tutor => tutor.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(11)
            .HasConversion(
                telefone => telefone.HasValue ? telefone.Value.Valor : null,
                valor => valor == null ? null : Telefone.Criar(valor));

        builder.Property(tutor => tutor.Situacao)
            .HasColumnName("situacao")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tutor => tutor.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(tutor => tutor.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(tutor => tutor.InativadoEm)
            .HasColumnName("inativado_em")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(tutor => tutor.TenantId)
            .HasDatabaseName("ix_tutores_tenant_id");

        builder.HasIndex(tutor => new { tutor.TenantId, tutor.Documento })
            .IsUnique()
            .HasDatabaseName("ix_tutores_tenant_id_documento");

        builder.HasQueryFilter(CriarFiltroPorTenant());
    }

    private Expression<Func<Tutor, bool>> CriarFiltroPorTenant()
    {
        ParameterExpression tutor = Expression.Parameter(typeof(Tutor), "tutor");
        Expression tenantIdAtual = _tenantIdAtual.Body;
        Expression tenantIdTemValor = Expression.Property(tenantIdAtual, nameof(Nullable<Guid>.HasValue));
        Expression tenantIdValor = Expression.Property(tenantIdAtual, nameof(Nullable<Guid>.Value));
        Expression tutorTenantId = Expression.Property(tutor, nameof(Tutor.TenantId));
        Expression tenantIdDoFiltro = Expression.Call(
            typeof(TenantId),
            nameof(TenantId.Criar),
            typeArguments: null,
            tenantIdValor);

        return Expression.Lambda<Func<Tutor, bool>>(
            Expression.AndAlso(
                tenantIdTemValor,
                Expression.Equal(tutorTenantId, tenantIdDoFiltro)),
            tutor);
    }
}
