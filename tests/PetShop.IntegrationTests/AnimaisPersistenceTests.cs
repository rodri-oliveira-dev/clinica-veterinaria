using Microsoft.EntityFrameworkCore;

using Npgsql;

using PetShop.Api.Infrastructure.Persistence;
using PetShop.Tutores.Application;
using PetShop.Tutores.Domain;
using PetShop.Tutores.Infrastructure;

namespace PetShop.IntegrationTests;

public sealed class AnimaisPersistenceTests : IClassFixture<PostgreSqlFixture>
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly DateTimeOffset CriadoEm = new(2026, 7, 20, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Hoje = new(2026, 7, 20);

    private readonly PostgreSqlFixture _postgresql;

    public AnimaisPersistenceTests(PostgreSqlFixture postgresql)
    {
        _postgresql = postgresql;
    }

    [Fact]
    public async Task CriacaoELeituraNoMesmoTenant_PersisteAnimalEVinculo()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1");
        Guid animalId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb1");

        await SeedTutorAsync(tutorId, TenantA);

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Animal>().Add(CriarAnimal(animalId, TenantA, tutorId));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leitura = _postgresql.CreateDbContext(TenantA);

        Animal? animal = await leitura.Set<Animal>()
            .SingleOrDefaultAsync(animal => animal.Id == AnimalId.Criar(animalId), TestContext.Current.CancellationToken);

        Assert.NotNull(animal);
        Assert.Equal(TenantId.Criar(TenantA), animal.TenantId);
        Assert.Equal(tutorId, animal.TutorResponsavel.TutorId);
        Assert.Equal("Luna", animal.Nome.Valor);
        Assert.Equal(SituacaoDoAnimal.Ativo, animal.Situacao);
    }

    [Fact]
    public async Task LeituraEmOutroTenant_TrataAnimalComoInexistente()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2");
        Guid animalId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb2");

        await SeedTutorAsync(tutorId, TenantA);

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Animal>().Add(CriarAnimal(animalId, TenantA, tutorId));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext outroTenant = _postgresql.CreateDbContext(TenantB);

        Animal? animal = await outroTenant.Set<Animal>()
            .SingleOrDefaultAsync(animal => animal.Id == AnimalId.Criar(animalId), TestContext.Current.CancellationToken);

        Assert.Null(animal);
    }

    [Fact]
    public async Task CadastrarAnimal_ComTutorDoMesmoTenant_PersistePelaApplication()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa3");
        await SeedTutorAsync(tutorId, TenantA);

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA);
        var service = new AnimaisApplicationService(
            new ContextoTenantTeste(TenantA),
            new ContextoUsuarioTeste(),
            new AnimaisRepository(dbContext),
            new FixedTimeProvider(CriadoEm));

        AnimalDetalhe detalhe = await service.CadastrarAsync(
            new CadastrarAnimalCommand(
                tutorId,
                " Luna ",
                " Canina ",
                " SRD ",
                "femea",
                new DateOnly(2020, 5, 2),
                " Caramelo ",
                " Resgata com medo de fogos "),
            TestContext.Current.CancellationToken);

        Assert.NotEqual(Guid.Empty, detalhe.AnimalId);
        Assert.Equal(tutorId, detalhe.TutorResponsavelId);
        Assert.Equal("Luna", detalhe.Nome);
        Assert.Equal("Canina", detalhe.Especie);
        Assert.Equal("SRD", detalhe.Raca);
        Assert.Equal("femea", detalhe.Sexo);
        Assert.Equal("ativo", detalhe.Situacao);
    }

    [Fact]
    public async Task CadastrarAnimal_ComTutorDeOutroTenant_TrataTutorComoInexistente()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorOutroTenant = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb4");
        await SeedTutorAsync(tutorOutroTenant, TenantB);

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA);
        var service = new AnimaisApplicationService(
            new ContextoTenantTeste(TenantA),
            new ContextoUsuarioTeste(),
            new AnimaisRepository(dbContext),
            new FixedTimeProvider(CriadoEm));

        await Assert.ThrowsAsync<TutorResponsavelNaoEncontradoException>(() =>
            service.CadastrarAsync(
                CriarCadastrarAnimalCommand(tutorOutroTenant),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TutorInexistente_EhRejeitadoPelaForeignKeyComposta()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorInexistente = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa5");

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA);

        dbContext.Set<Animal>().Add(CriarAnimal(
            Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb5"),
            TenantA,
            tutorInexistente));

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, UnwrapPostgresException(exception).SqlState);
    }

    [Fact]
    public async Task TutorDeOutroTenant_EhRejeitadoPelaForeignKeyComposta()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorOutroTenant = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb6");
        await SeedTutorAsync(tutorOutroTenant, TenantB);

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA);

        dbContext.Set<Animal>().Add(CriarAnimal(
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa6"),
            TenantA,
            tutorOutroTenant));

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() =>
            dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, UnwrapPostgresException(exception).SqlState);
    }

    [Fact]
    public async Task AlteracaoCrossTenant_EhBloqueadaAntesDeSalvar()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa7");
        Guid animalId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb7");

        await SeedTutorAsync(tutorId, TenantA);

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Animal>().Add(CriarAnimal(animalId, TenantA, tutorId));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext outroTenant = _postgresql.CreateDbContext(TenantB);
        Animal animalDoTenantA = CriarAnimal(animalId, TenantA, tutorId);

        animalDoTenantA.AlterarCadastro(
            NomeDoAnimal.Criar("Sol"),
            Especie.Criar("Felina"),
            raca: null,
            SexoDoAnimal.Femea,
            dataDeNascimento: null,
            corOuPelagem: null,
            observacaoCadastral: null,
            CriadoEm.AddHours(1));

        outroTenant.Update(animalDoTenantA);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            outroTenant.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Contains("animal alterado", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TenantObrigatorio_EhRejeitadoPeloBanco()
    {
        await _postgresql.ResetDatabaseAsync();

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext();

        Exception exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            dbContext.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO animais (id, nome, especie, sexo, situacao, tutor_responsavel_id, criado_em, atualizado_em)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})
                """,
                Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb8"),
                "Animal Sem Tenant",
                "Canina",
                (int)SexoDoAnimal.Femea,
                (int)SituacaoDoAnimal.Ativo,
                Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa8"),
                CriadoEm,
                CriadoEm));

        Assert.Equal(PostgresErrorCodes.NotNullViolation, UnwrapPostgresException(exception).SqlState);
    }

    [Fact]
    public async Task TenantObrigatorio_EhRejeitadoAoAlterarEntidadeTenantOwned()
    {
        await _postgresql.ResetDatabaseAsync();

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext();

        dbContext.Set<Animal>().Add(CriarAnimal(
            Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb9"),
            TenantA,
            Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa9")));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Contains("tenant autenticado", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValueObjects_SaoPersistidosNormalizados()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa10");
        Guid animalId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb10");

        await SeedTutorAsync(tutorId, TenantA);

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Animal>().Add(Animal.Cadastrar(
                AnimalId.Criar(animalId),
                TenantId.Criar(TenantA),
                NomeDoAnimal.Criar("  Luna  "),
                Especie.Criar("  Canina  "),
                Raca.Criar("  SRD  "),
                SexoDoAnimal.Femea,
                DataDeNascimento.Criar(new DateOnly(2020, 5, 2), Hoje),
                CorOuPelagem.Criar("  Caramelo  "),
                ObservacaoCadastral.Criar("  Resgata com medo de fogos  "),
                TutorResponsavel.Criar(tutorId),
                CriadoEm));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leitura = _postgresql.CreateDbContext(TenantA);

        Animal animal = await leitura.Set<Animal>()
            .SingleAsync(animal => animal.Id == AnimalId.Criar(animalId), TestContext.Current.CancellationToken);

        Assert.Equal("Luna", animal.Nome.Valor);
        Assert.Equal("Canina", animal.Especie.Valor);
        Assert.Equal("SRD", animal.Raca?.Valor);
        Assert.Equal(new DateOnly(2020, 5, 2), animal.DataDeNascimento?.Valor);
        Assert.Equal("Caramelo", animal.CorOuPelagem?.Valor);
        Assert.Equal("Resgata com medo de fogos", animal.ObservacaoCadastral?.Valor);
    }

    [Fact]
    public async Task Inativacao_PersisteSituacaoETimestamp()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa11");
        Guid animalId = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb11");
        DateTimeOffset inativadoEm = CriadoEm.AddDays(1);

        await SeedTutorAsync(tutorId, TenantA);

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            dbContext.Set<Animal>().Add(CriarAnimal(animalId, TenantA, tutorId));

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA))
        {
            Animal animal = await dbContext.Set<Animal>()
                .SingleAsync(animal => animal.Id == AnimalId.Criar(animalId), TestContext.Current.CancellationToken);

            animal.Inativar(inativadoEm);

            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using PetShopDbContext leitura = _postgresql.CreateDbContext(TenantA);
        Animal animalInativo = await leitura.Set<Animal>()
            .SingleAsync(animal => animal.Id == AnimalId.Criar(animalId), TestContext.Current.CancellationToken);

        Assert.Equal(SituacaoDoAnimal.Inativo, animalInativo.Situacao);
        Assert.False(animalInativo.EstaAtivo);
        Assert.Equal(inativadoEm, animalInativo.InativadoEm);
        Assert.Equal(inativadoEm, animalInativo.AtualizadoEm);
    }

    [Fact]
    public async Task SexoForaDoDominio_EhRejeitadoPeloBanco()
    {
        await _postgresql.ResetDatabaseAsync();

        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa12");
        await SeedTutorAsync(tutorId, TenantA);

        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(TenantA);

        Exception exception = await Assert.ThrowsAnyAsync<Exception>(() =>
            dbContext.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO animais (
                    id, tenant_id, nome, especie, sexo, situacao, tutor_responsavel_id, criado_em, atualizado_em)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})
                """,
                Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbb12"),
                TenantA,
                "Luna",
                "Canina",
                99,
                (int)SituacaoDoAnimal.Ativo,
                tutorId,
                CriadoEm,
                CriadoEm));

        Assert.Equal(PostgresErrorCodes.CheckViolation, UnwrapPostgresException(exception).SqlState);
    }

    private async Task SeedTutorAsync(Guid tutorId, Guid tenantId)
    {
        await using PetShopDbContext dbContext = _postgresql.CreateDbContext(tenantId);

        dbContext.Set<Tutor>().Add(Tutor.Cadastrar(
            TutorId.Criar(tutorId),
            TenantId.Criar(tenantId),
            NomeDoTutor.Criar("Maria Oliveira"),
            documento: null,
            Email.Criar($"maria.{tutorId:N}@example.com"),
            Telefone.Criar("(11) 98765-4321"),
            CriadoEm));

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static Animal CriarAnimal(Guid animalId, Guid tenantId, Guid tutorId) =>
        Animal.Cadastrar(
            AnimalId.Criar(animalId),
            TenantId.Criar(tenantId),
            NomeDoAnimal.Criar("Luna"),
            Especie.Criar("Canina"),
            Raca.Criar("SRD"),
            SexoDoAnimal.Femea,
            DataDeNascimento.Criar(new DateOnly(2020, 5, 2), Hoje),
            CorOuPelagem.Criar("Caramelo"),
            ObservacaoCadastral.Criar("Resgata com medo de fogos"),
            TutorResponsavel.Criar(tutorId),
            CriadoEm);

    private static CadastrarAnimalCommand CriarCadastrarAnimalCommand(Guid tutorId) =>
        new(
            tutorId,
            "Luna",
            "Canina",
            "SRD",
            "femea",
            new DateOnly(2020, 5, 2),
            "Caramelo",
            "Resgata com medo de fogos");

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

    private sealed class ContextoTenantTeste : IContextoTenantAtual
    {
        public ContextoTenantTeste(Guid tenantId)
        {
            TenantId = tenantId;
        }

        public Guid? TenantId { get; }
    }

    private sealed class ContextoUsuarioTeste : IContextoUsuarioAtual
    {
        public string? Subject => "integration-test-subject";
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
