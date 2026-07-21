using System.Data;
using System.Text;

using Microsoft.EntityFrameworkCore;

using PetShop.Tutores.Application;
using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class AnimaisRepository : IAnimaisRepository
{
    private readonly IContextoTenantAtual _contextoTenantAtual;
    private readonly DbContext _dbContext;

    public AnimaisRepository(
        IContextoTenantAtual contextoTenantAtual,
        DbContext dbContext)
    {
        _contextoTenantAtual = contextoTenantAtual;
        _dbContext = dbContext;
    }

    public Task AdicionarAsync(Animal animal, CancellationToken cancellationToken)
    {
        _dbContext.Set<Animal>().Add(animal);

        return Task.CompletedTask;
    }

    public Task<Animal?> ObterPorIdAsync(AnimalId animalId, CancellationToken cancellationToken) =>
        _dbContext.Set<Animal>()
            .SingleOrDefaultAsync(animal => animal.Id == animalId, cancellationToken);

    public Task<Animal?> ObterPorIdSemTrackingAsync(AnimalId animalId, CancellationToken cancellationToken) =>
        _dbContext.Set<Animal>()
            .AsNoTracking()
            .SingleOrDefaultAsync(animal => animal.Id == animalId, cancellationToken);

    public Task<Tutor?> ObterTutorResponsavelPorIdAsync(TutorId tutorId, CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .SingleOrDefaultAsync(tutor => tutor.Id == tutorId, cancellationToken);

    public Task<Tutor?> ObterTutorResponsavelPorIdParaAtualizacaoAsync(
        TutorId tutorId,
        CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .FromSqlRaw(
                "SELECT * FROM tutores WHERE tenant_id = {0} AND id = {1} FOR UPDATE",
                ObterTenantAtual(),
                tutorId.Valor)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(cancellationToken);

    public Task AdicionarTransferenciaAsync(
        TransferenciaDeResponsabilidadeDoAnimal transferencia,
        CancellationToken cancellationToken)
    {
        _dbContext.Set<TransferenciaDeResponsabilidadeDoAnimal>().Add(transferencia);

        return Task.CompletedTask;
    }

    public async Task<PesquisaDeAnimais> PesquisarAsync(
        FiltrosDePesquisaDeAnimais filtros,
        CancellationToken cancellationToken)
    {
        (string sql, object[] parameters) = CriarSqlPesquisa(filtros, incluirPaginacao: false);
        int total = await _dbContext.Set<Animal>()
            .FromSqlRaw(sql, parameters)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        (string sqlPaginado, object[] parametrosPaginados) = CriarSqlPesquisa(filtros, incluirPaginacao: true);
        Animal[] animais = await _dbContext.Set<Animal>()
            .FromSqlRaw(sqlPaginado, parametrosPaginados)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return new PesquisaDeAnimais(
            animais.Select(MapearResumo).ToArray(),
            filtros.Pagina,
            filtros.TamanhoPagina,
            total);
    }

    public async Task<TResult> ExecutarEmTransacaoAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacao,
        CancellationToken cancellationToken)
    {
        Microsoft.EntityFrameworkCore.Storage.IExecutionStrategy strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
                    await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                TResult resultado = await operacao(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return resultado;
            });
    }

    public async Task SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflitoDeConcorrenciaDoAnimalException();
        }
    }

    private (string Sql, object[] Parameters) CriarSqlPesquisa(
        FiltrosDePesquisaDeAnimais filtros,
        bool incluirPaginacao)
    {
        var sql = new StringBuilder("SELECT * FROM animais WHERE tenant_id = {0}");
        var parameters = new List<object> { ObterTenantAtual() };
        int parameterIndex = parameters.Count;

        if (!string.IsNullOrWhiteSpace(filtros.Nome))
        {
            sql.Append(" AND nome ILIKE {")
                .Append(parameterIndex)
                .Append("} ESCAPE '\\'");
            parameters.Add(CriarPadraoBusca(filtros.Nome));
            parameterIndex++;
        }

        if (filtros.TutorResponsavel.HasValue)
        {
            sql.Append(" AND tutor_responsavel_id = {")
                .Append(parameterIndex)
                .Append('}');
            parameters.Add(filtros.TutorResponsavel.Value.TutorId);
            parameterIndex++;
        }

        if (filtros.Especie.HasValue)
        {
            sql.Append(" AND especie ILIKE {")
                .Append(parameterIndex)
                .Append("} ESCAPE '\\'");
            parameters.Add(filtros.Especie.Value.Valor);
            parameterIndex++;
        }

        if (filtros.Situacao.HasValue)
        {
            sql.Append(" AND situacao = {")
                .Append(parameterIndex)
                .Append('}');
            parameters.Add((int)filtros.Situacao.Value);
            parameterIndex++;
        }

        if (incluirPaginacao)
        {
            sql.Append(" ORDER BY ")
                .Append(filtros.OrdenarPor == OrdenacaoDeAnimais.CriadoEm ? "criado_em" : "lower(nome)")
                .Append(filtros.Direcao == DirecaoDaOrdenacao.Desc ? " DESC" : " ASC")
                .Append(", id ASC LIMIT {")
                .Append(parameterIndex)
                .Append("} OFFSET {")
                .Append(parameterIndex + 1)
                .Append('}');
            parameters.Add(filtros.TamanhoPagina);
            parameters.Add((filtros.Pagina - 1) * filtros.TamanhoPagina);
        }

        return (sql.ToString(), parameters.ToArray());
    }

    private static AnimalResumo MapearResumo(Animal animal) =>
        new(
            animal.Id.Valor,
            animal.TutorResponsavel.TutorId,
            animal.Nome.Valor,
            animal.Especie.Valor,
            MapearSituacao(animal.Situacao));

    private static string MapearSituacao(SituacaoDoAnimal situacao) =>
        situacao switch
        {
            SituacaoDoAnimal.Ativo => "ativo",
            SituacaoDoAnimal.Inativo => "inativo",
            SituacaoDoAnimal.Falecido => "falecido",
            _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null)
        };

    private Guid ObterTenantAtual() =>
        _contextoTenantAtual.TenantId
        ?? throw new InvalidOperationException(
            "O tenant autenticado deve estar resolvido antes de consultar animais.");

    private static string CriarPadraoBusca(string valor) =>
        $"%{valor.Trim().Replace(@"\", @"\\", StringComparison.Ordinal).Replace("%", @"\%", StringComparison.Ordinal).Replace("_", @"\_", StringComparison.Ordinal)}%";
}
