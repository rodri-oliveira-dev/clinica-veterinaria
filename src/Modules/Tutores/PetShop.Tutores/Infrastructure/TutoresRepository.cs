using System.Data;
using System.Text;

using Microsoft.EntityFrameworkCore;

using PetShop.Tutores.Application;
using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class TutoresRepository : ITutoresRepository
{
    private readonly IContextoTenantAtual _contextoTenantAtual;
    private readonly DbContext _dbContext;

    public TutoresRepository(
        IContextoTenantAtual contextoTenantAtual,
        DbContext dbContext)
    {
        _contextoTenantAtual = contextoTenantAtual;
        _dbContext = dbContext;
    }

    public Task AdicionarAsync(Tutor tutor, CancellationToken cancellationToken)
    {
        _dbContext.Set<Tutor>().Add(tutor);

        return Task.CompletedTask;
    }

    public Task<Tutor?> ObterPorIdAsync(TutorId tutorId, CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .SingleOrDefaultAsync(tutor => tutor.Id == tutorId, cancellationToken);

    public Task<Tutor?> ObterPorIdParaAtualizacaoAsync(TutorId tutorId, CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .FromSqlRaw(
                "SELECT * FROM tutores WHERE tenant_id = {0} AND id = {1} FOR UPDATE",
                ObterTenantAtual(),
                tutorId.Valor)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(cancellationToken);

    public Task<Tutor?> ObterPorIdSemTrackingAsync(TutorId tutorId, CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .AsNoTracking()
            .SingleOrDefaultAsync(tutor => tutor.Id == tutorId, cancellationToken);

    public Task<bool> ExisteDocumentoAsync(
        Cpf documento,
        TutorId? excetoTutorId,
        CancellationToken cancellationToken)
    {
        IQueryable<Tutor> query = _dbContext.Set<Tutor>()
            .Where(tutor => tutor.Documento == documento);

        if (excetoTutorId.HasValue)
        {
            query = query.Where(tutor => tutor.Id != excetoTutorId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExisteAnimalAtivoVinculadoAsync(
        TutorId tutorId,
        CancellationToken cancellationToken) =>
        _dbContext.Set<Animal>()
            .AnyAsync(
                animal => animal.TutorResponsavelId == tutorId && animal.Situacao == SituacaoDoAnimal.Ativo,
                cancellationToken);

    public async Task<PesquisaDeTutores> PesquisarAsync(
        FiltrosDePesquisaDeTutores filtros,
        CancellationToken cancellationToken)
    {
        (string sql, object[] parameters) = CriarSqlPesquisa(filtros, incluirPaginacao: false);
        int total = await _dbContext.Set<Tutor>()
            .FromSqlRaw(sql, parameters)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        (string sqlPaginado, object[] parametrosPaginados) = CriarSqlPesquisa(filtros, incluirPaginacao: true);
        Tutor[] tutores = await _dbContext.Set<Tutor>()
            .FromSqlRaw(sqlPaginado, parametrosPaginados)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return new PesquisaDeTutores(
            tutores.Select(MapearResumo).ToArray(),
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

    public Task SalvarAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private (string Sql, object[] Parameters) CriarSqlPesquisa(
        FiltrosDePesquisaDeTutores filtros,
        bool incluirPaginacao)
    {
        var sql = new StringBuilder("SELECT * FROM tutores WHERE tenant_id = {0}");
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

        if (filtros.Cpf.HasValue)
        {
            sql.Append(" AND documento = {")
                .Append(parameterIndex)
                .Append('}');
            parameters.Add(filtros.Cpf.Value.Valor);
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
                .Append(filtros.OrdenarPor == OrdenacaoDeTutores.CriadoEm ? "criado_em" : "lower(nome)")
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

    private static TutorResumo MapearResumo(Tutor tutor) =>
        new(
            tutor.Id.Valor,
            tutor.Nome.Valor,
            MascararCpf(tutor.Documento),
            MapearSituacao(tutor.Situacao));

    private static string MapearSituacao(SituacaoDoTutor situacao) =>
        situacao switch
        {
            SituacaoDoTutor.Ativo => "ativo",
            SituacaoDoTutor.Inativo => "inativo",
            _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null)
        };

    private static string? MascararCpf(Cpf? documento)
    {
        if (!documento.HasValue)
        {
            return null;
        }

        string valor = documento.Value.Valor;

        return $"***.***.***-{valor[^2..]}";
    }

    private Guid ObterTenantAtual() =>
        _contextoTenantAtual.TenantId
        ?? throw new InvalidOperationException(
            "O tenant autenticado deve estar resolvido antes de consultar tutores.");

    private static string CriarPadraoBusca(string valor) =>
        $"%{valor.Trim().Replace(@"\", @"\\", StringComparison.Ordinal).Replace("%", @"\%", StringComparison.Ordinal).Replace("_", @"\_", StringComparison.Ordinal)}%";
}
