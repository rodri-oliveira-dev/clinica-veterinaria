using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class AnimalEntityTypeConfiguration : IEntityTypeConfiguration<Animal>
{
    private readonly Expression<Func<Guid?>> _tenantIdAtual;

    public AnimalEntityTypeConfiguration(Expression<Func<Guid?>> tenantIdAtual)
    {
        _tenantIdAtual = tenantIdAtual;
    }

    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable(
            "animais",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_animais_id_not_empty",
                    "id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_animais_tenant_id_not_empty",
                    "tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_animais_tutor_responsavel_id_not_empty",
                    "tutor_responsavel_id <> '00000000-0000-0000-0000-000000000000'::uuid");
                table.HasCheckConstraint(
                    "ck_animais_nome_not_blank",
                    "length(btrim(nome)) > 0");
                table.HasCheckConstraint(
                    "ck_animais_especie_not_blank",
                    "length(btrim(especie)) > 0");
                table.HasCheckConstraint(
                    "ck_animais_raca_not_blank",
                    "raca IS NULL OR length(btrim(raca)) > 0");
                table.HasCheckConstraint(
                    "ck_animais_cor_ou_pelagem_not_blank",
                    "cor_ou_pelagem IS NULL OR length(btrim(cor_ou_pelagem)) > 0");
                table.HasCheckConstraint(
                    "ck_animais_observacao_cadastral_not_blank",
                    "observacao_cadastral IS NULL OR length(btrim(observacao_cadastral)) > 0");
                table.HasCheckConstraint(
                    "ck_animais_sexo",
                    "sexo IN (0, 1, 2)");
                table.HasCheckConstraint(
                    "ck_animais_situacao",
                    "situacao IN (1, 2, 3)");
                table.HasCheckConstraint(
                    "ck_animais_data_falecimento_situacao",
                    "(situacao = 3 AND data_do_falecimento IS NOT NULL) OR (situacao <> 3 AND data_do_falecimento IS NULL)");
            });

        builder.HasKey(animal => animal.Id);

        builder.HasAlternateKey(animal => new { animal.TenantId, animal.Id })
            .HasName("ak_animais_tenant_id_id");

        builder.Property(animal => animal.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Valor,
                valor => AnimalId.Criar(valor))
            .ValueGeneratedNever();

        builder.Property(animal => animal.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(
                tenantId => tenantId.Valor,
                valor => TenantId.Criar(valor))
            .IsRequired();

        builder.Property(animal => animal.Nome)
            .HasColumnName("nome")
            .HasColumnType("text")
            .HasConversion(
                nome => nome.Valor,
                valor => NomeDoAnimal.Criar(valor))
            .IsRequired();

        builder.Property(animal => animal.Especie)
            .HasColumnName("especie")
            .HasColumnType("text")
            .HasConversion(
                especie => especie.Valor,
                valor => Especie.Criar(valor))
            .IsRequired();

        builder.Property(animal => animal.Raca)
            .HasColumnName("raca")
            .HasColumnType("text")
            .HasConversion(
                raca => raca.HasValue ? raca.Value.Valor : null,
                valor => valor == null ? null : Raca.Criar(valor));

        builder.Property(animal => animal.Sexo)
            .HasColumnName("sexo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(animal => animal.DataDeNascimento)
            .HasColumnName("data_de_nascimento")
            .HasColumnType("date")
            .HasConversion(
                data => data.HasValue ? data.Value.Valor : (DateOnly?)null,
                valor => valor.HasValue ? DataDeNascimento.Criar(valor.Value, valor.Value) : (DataDeNascimento?)null);

        builder.Property(animal => animal.DataDoFalecimento)
            .HasColumnName("data_do_falecimento")
            .HasColumnType("date")
            .HasConversion(
                data => data.HasValue ? data.Value.Valor : (DateOnly?)null,
                valor => valor.HasValue ? DataDoFalecimento.Criar(valor.Value, valor.Value) : (DataDoFalecimento?)null);

        builder.Property(animal => animal.CorOuPelagem)
            .HasColumnName("cor_ou_pelagem")
            .HasColumnType("text")
            .HasConversion(
                corOuPelagem => corOuPelagem.HasValue ? corOuPelagem.Value.Valor : null,
                valor => valor == null ? null : CorOuPelagem.Criar(valor));

        builder.Property(animal => animal.ObservacaoCadastral)
            .HasColumnName("observacao_cadastral")
            .HasColumnType("text")
            .HasConversion(
                observacao => observacao.HasValue ? observacao.Value.Valor : null,
                valor => valor == null ? null : ObservacaoCadastral.Criar(valor));

        builder.Property(animal => animal.Situacao)
            .HasColumnName("situacao")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(animal => animal.TutorResponsavelId)
            .HasColumnName("tutor_responsavel_id")
            .HasConversion(
                tutorId => tutorId.Valor,
                valor => TutorId.Criar(valor))
            .IsRequired();

        builder.Property(animal => animal.Versao)
            .HasColumnName("versao")
            .HasDefaultValue(1)
            .IsConcurrencyToken()
            .IsRequired();

        builder.Property(animal => animal.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(animal => animal.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(animal => animal.InativadoEm)
            .HasColumnName("inativado_em")
            .HasColumnType("timestamp with time zone");

        builder.HasOne<Tutor>()
            .WithMany()
            .HasForeignKey(animal => new { animal.TenantId, animal.TutorResponsavelId })
            .HasPrincipalKey(tutor => new { tutor.TenantId, tutor.Id })
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_animais_tutores_tenant_id_tutor_responsavel_id");

        builder.HasIndex(animal => animal.TenantId)
            .HasDatabaseName("ix_animais_tenant_id");

        builder.HasIndex(animal => new { animal.TenantId, animal.TutorResponsavelId })
            .HasDatabaseName("ix_animais_tenant_id_tutor_responsavel_id");

        builder.HasQueryFilter(CriarFiltroPorTenant());
    }

    private Expression<Func<Animal, bool>> CriarFiltroPorTenant()
    {
        ParameterExpression animal = Expression.Parameter(typeof(Animal), "animal");
        Expression tenantIdAtual = _tenantIdAtual.Body;
        Expression tenantIdTemValor = Expression.Property(tenantIdAtual, nameof(Nullable<Guid>.HasValue));
        Expression tenantIdValor = Expression.Property(tenantIdAtual, nameof(Nullable<Guid>.Value));
        Expression animalTenantId = Expression.Property(animal, nameof(Animal.TenantId));
        Expression tenantIdDoFiltro = Expression.Call(
            typeof(TenantId),
            nameof(TenantId.Criar),
            typeArguments: null,
            tenantIdValor);

        return Expression.Lambda<Func<Animal, bool>>(
            Expression.AndAlso(
                tenantIdTemValor,
                Expression.Equal(animalTenantId, tenantIdDoFiltro)),
            animal);
    }
}
