using Microsoft.EntityFrameworkCore;

using Npgsql;

using PetShop.Api.Infrastructure.Persistence;
using PetShop.Tutores.Domain;

namespace PetShop.IntegrationTests;

public sealed class TutoresPersistenceTests : IClassFixture<PostgreSqlFixture>
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlFixture _postgresql;

    public TutoresPersistenceTests(PostgreSqlFixture postgresql)
    {
        _postgresql = postgresql;
    }

    [Fact]
    public async Task CriacaoELeituraNoMesmoTenant_PersisteTutor()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1");

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Tutor>().Add(CriarTutor(tutorId, TenantA));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leitura = _postgresql.CreateDbContext(TenantA);

        Tutor? tutor = await leitura.Set<Tutor>()
            .SingleOrDefaultAsync(tutor => tutor.Id == TutorId.Criar(tutorId), TestContext.Current.CancellationToken);

        Assert.NotNull(tutor);
        Assert.Equal(TenantId.Criar(TenantA), tutor.TenantId);
        Assert.Equal("Maria Oliveira", tutor.Nome.Valor);
        Assert.Equal(SituacaoDoTutor.Ativo, tutor.Situacao);
    }

    [Fact]
    public async Task LeituraEmOutroTenant_TrataTutorComoInexistente()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2");

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Tutor>().Add(CriarTutor(tutorId, TenantA));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext outroTenant = _postgresql.CreateDbContext(TenantB);

        Tutor? tutor = await outroTenant.Set<Tutor>()
            .SingleOrDefaultAsync(tutor => tutor.Id == TutorId.Criar(tutorId), TestContext.Current.CancellationToken);

        Assert.Null(tutor);
    }

    [Fact]
    public async Task AlteracaoCrossTenant_EhBloqueadaAntesDeSalvar()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3");

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Tutor>().Add(CriarTutor(tutorId, TenantA));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext outroTenant = _postgresql.CreateDbContext(TenantB);
        Tutor tutorDoTenantA = CriarTutor(tutorId, TenantA);

        tutorDoTenantA.AlterarCadastro(
            NomeDoTutor.Criar("Maria Cross Tenant"),
            Cpf.Criar("52998224725"),
            Email.Criar("maria.cross@example.com"),
            Telefone.Criar("(11) 98765-4321"),
            CriadoEm.AddHours(1));

        outroTenant.Update(tutorDoTenantA);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            outroTenant.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Contains("tenant autenticado", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CpfIgual_EhPermitidoEmTenantsDiferentes()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorTenantA = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa4");
        Guid tutorTenantB = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb4");

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Tutor>().Add(CriarTutor(tutorTenantA, TenantA));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantB))
        {
            dbContext.Set<Tutor>().Add(CriarTutor(tutorTenantB, TenantB));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leituraTenantA = _postgresql.CreateDbContext(TenantA);
        await using PetShopDbContext leituraTenantB = _postgresql.CreateDbContext(TenantB);

        Assert.Equal(1, await leituraTenantA.Set<Tutor>().CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await leituraTenantB.Set<Tutor>().CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CpfIgual_NoMesmoTenant_EhRejeitadoPeloBanco()
    {
        await _postgresql.ResetDatabaseAsync();

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA);

        dbContext.Set<Tutor>().Add(CriarTutor(Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa5"), TenantA));
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        dbContext.Set<Tutor>().Add(CriarTutor(Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb5"), TenantA));

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, UnwrapPostgresException(exception).SqlState);
    }

    [Fact]
    public async Task TenantObrigatorio_EhRejeitadoPeloBanco()
    {
        await _postgresql.ResetDatabaseAsync();

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext();

        Exception exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            dbContext.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO tutores (id, nome, email, situacao, criado_em, atualizado_em)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5})
                """,
                Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6"),
                "Tutor Sem Tenant",
                "sem.tenant@example.com",
                (int)SituacaoDoTutor.Ativo,
                CriadoEm,
                CriadoEm));

        Assert.Equal(PostgresErrorCodes.NotNullViolation, UnwrapPostgresException(exception).SqlState);
    }

    [Fact]
    public async Task TenantObrigatorio_EhRejeitadoAoAlterarEntidadeTenantOwned()
    {
        await _postgresql.ResetDatabaseAsync();

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext();

        dbContext.Set<Tutor>().Add(CriarTutor(Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7"), TenantA));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Contains("tenant autenticado", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValueObjects_SaoPersistidosNormalizados()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8");

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Tutor>().Add(Tutor.Cadastrar(
                TutorId.Criar(tutorId),
                TenantId.Criar(TenantA),
                NomeDoTutor.Criar("  Ana Souza  "),
                Cpf.Criar("529.982.247-25"),
                Email.Criar("Ana@Example.com"),
                Telefone.Criar("+55 (11) 98765-4321"),
                CriadoEm));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leitura = _postgresql.CreateDbContext(TenantA);

        Tutor tutor = await leitura.Set<Tutor>()
            .SingleAsync(tutor => tutor.Id == TutorId.Criar(tutorId), TestContext.Current.CancellationToken);

        Assert.Equal("Ana Souza", tutor.Nome.Valor);
        Assert.Equal("52998224725", tutor.Documento?.Valor);
        Assert.Equal("ana@example.com", tutor.Email?.Valor);
        Assert.Equal("11987654321", tutor.Telefone?.Valor);
    }

    [Fact]
    public async Task Inativacao_PersisteSituacaoETimestamp()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa9");
        DateTimeOffset inativadoEm = CriadoEm.AddDays(1);

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Tutor>().Add(CriarTutor(tutorId, TenantA));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            Tutor tutor = await dbContext.Set<Tutor>()
                .SingleAsync(tutor => tutor.Id == TutorId.Criar(tutorId), TestContext.Current.CancellationToken);

            tutor.Inativar(inativadoEm);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leitura = _postgresql.CreateDbContext(TenantA);
        Tutor tutorInativo = await leitura.Set<Tutor>()
            .SingleAsync(tutor => tutor.Id == TutorId.Criar(tutorId), TestContext.Current.CancellationToken);

        Assert.Equal(SituacaoDoTutor.Inativo, tutorInativo.Situacao);
        Assert.False(tutorInativo.EstaAtivo);
        Assert.Equal(inativadoEm, tutorInativo.InativadoEm);
        Assert.Equal(inativadoEm, tutorInativo.AtualizadoEm);
    }

    private static Tutor CriarTutor(Guid tutorId, Guid tenantId) =>
        Tutor.Cadastrar(
            TutorId.Criar(tutorId),
            TenantId.Criar(tenantId),
            NomeDoTutor.Criar("Maria Oliveira"),
            Cpf.Criar("529.982.247-25"),
            Email.Criar("Maria@Example.com"),
            Telefone.Criar("(11) 98765-4321"),
            CriadoEm);

    private static PostgresException UnwrapPostgresException(Exception exception)
    {
        Exception? current = exception;

        while (current is not null)
        {
            if (current is PostgresException postgresException)
            {
                return postgresException;
            }

            current = current.InnerException;
        }

        throw new InvalidOperationException("A excecao do PostgreSQL era esperada.", exception);
    }
}
