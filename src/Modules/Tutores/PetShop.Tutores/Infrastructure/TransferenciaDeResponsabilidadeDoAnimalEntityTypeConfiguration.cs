using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class TransferenciaDeResponsabilidadeDoAnimalEntityTypeConfiguration :
    IEntityTypeConfiguration<TransferenciaDeResponsabilidadeDoAnimal>
{
    private readonly Expression<Func<Guid?>> _tenantIdAtual;

    public TransferenciaDeResponsabilidadeDoAnimalEntityTypeConfiguration(Expression<Func<Guid?>> tenantIdAtual)
    {
        _tenantIdAtual = tenantIdAtual;
    }

    public void Configure(EntityTypeBuilder<TransferenciaDeResponsabilidadeDoAnimal> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(
            "historico_transferencias_animais",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_id_not_empty",
                    "id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_tenant_id_not_empty",
                    "tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_animal_id_not_empty",
                    "animal_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_tutor_anterior_not_empty",
                    "tutor_anterior_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_tutor_novo_not_empty",
                    "tutor_novo_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_tutores_diferentes",
                    "tutor_anterior_id <> tutor_novo_id");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_subject_not_blank",
                    "length(btrim(subject)) > 0");
                table.HasCheckConstraint(
                    "ck_historico_transferencias_animais_motivo_not_blank",
                    "motivo IS NULL OR length(btrim(motivo)) > 0");
            });

        builder.HasKey(transferencia => transferencia.Id);

        builder.Property(transferencia => transferencia.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(transferencia => transferencia.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(
                tenantId => tenantId.Valor,
                valor => TenantId.Criar(valor))
            .IsRequired();

        builder.Property(transferencia => transferencia.AnimalId)
            .HasColumnName("animal_id")
            .HasConversion(
                animalId => animalId.Valor,
                valor => AnimalId.Criar(valor))
            .IsRequired();

        builder.Property(transferencia => transferencia.TutorAnteriorId)
            .HasColumnName("tutor_anterior_id")
            .HasConversion(
                tutorId => tutorId.Valor,
                valor => TutorId.Criar(valor))
            .IsRequired();

        builder.Property(transferencia => transferencia.TutorNovoId)
            .HasColumnName("tutor_novo_id")
            .HasConversion(
                tutorId => tutorId.Valor,
                valor => TutorId.Criar(valor))
            .IsRequired();

        builder.Property(transferencia => transferencia.RealizadaEm)
            .HasColumnName("realizada_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(transferencia => transferencia.Subject)
            .HasColumnName("subject")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(transferencia => transferencia.Motivo)
            .HasColumnName("motivo")
            .HasColumnType("text");

        builder.HasOne<Animal>()
            .WithMany()
            .HasForeignKey(transferencia => new { transferencia.TenantId, transferencia.AnimalId })
            .HasPrincipalKey(animal => new { animal.TenantId, animal.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hist_transf_animais_animais_tenant_id_animal_id");

        builder.HasOne<Tutor>()
            .WithMany()
            .HasForeignKey(transferencia => new { transferencia.TenantId, transferencia.TutorAnteriorId })
            .HasPrincipalKey(tutor => new { tutor.TenantId, tutor.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hist_transf_animais_tutores_tenant_id_tutor_anterior_id");

        builder.HasOne<Tutor>()
            .WithMany()
            .HasForeignKey(transferencia => new { transferencia.TenantId, transferencia.TutorNovoId })
            .HasPrincipalKey(tutor => new { tutor.TenantId, tutor.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_hist_transf_animais_tutores_tenant_id_tutor_novo_id");

        builder.HasIndex(transferencia => new { transferencia.TenantId, transferencia.AnimalId, transferencia.RealizadaEm })
            .HasDatabaseName("ix_historico_transferencias_animais_tenant_animal_data");

        builder.HasIndex(transferencia => new { transferencia.TenantId, transferencia.TutorAnteriorId })
            .HasDatabaseName("ix_hist_transf_animais_tenant_tutor_anterior");

        builder.HasIndex(transferencia => new { transferencia.TenantId, transferencia.TutorNovoId })
            .HasDatabaseName("ix_hist_transf_animais_tenant_tutor_novo");

        builder.HasQueryFilter(CriarFiltroPorTenant());
    }

    private Expression<Func<TransferenciaDeResponsabilidadeDoAnimal, bool>> CriarFiltroPorTenant()
    {
        ParameterExpression transferencia = Expression.Parameter(
            typeof(TransferenciaDeResponsabilidadeDoAnimal),
            "transferencia");
        Expression tenantIdAtual = _tenantIdAtual.Body;
        Expression tenantIdTemValor = Expression.Property(tenantIdAtual, nameof(Nullable<Guid>.HasValue));
        Expression tenantIdValor = Expression.Property(tenantIdAtual, nameof(Nullable<Guid>.Value));
        Expression transferenciaTenantId = Expression.Property(
            transferencia,
            nameof(TransferenciaDeResponsabilidadeDoAnimal.TenantId));
        Expression tenantIdDoFiltro = Expression.Call(
            typeof(TenantId),
            nameof(TenantId.Criar),
            typeArguments: null,
            tenantIdValor);

        return Expression.Lambda<Func<TransferenciaDeResponsabilidadeDoAnimal, bool>>(
            Expression.AndAlso(
                tenantIdTemValor,
                Expression.Equal(transferenciaTenantId, tenantIdDoFiltro)),
            transferencia);
    }
}
