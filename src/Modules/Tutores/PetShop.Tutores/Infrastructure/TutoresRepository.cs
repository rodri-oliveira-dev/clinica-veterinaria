using Microsoft.EntityFrameworkCore;

using PetShop.Tutores.Application;
using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class TutoresRepository : ITutoresRepository
{
    private readonly DbContext _dbContext;

    public TutoresRepository(DbContext dbContext)
    {
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
        Tutor[] tutoresDoTenant = await _dbContext.Set<Tutor>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        IEnumerable<Tutor> query = tutoresDoTenant;

        if (!string.IsNullOrWhiteSpace(filtros.Nome))
        {
            string nome = filtros.Nome.Trim();
            query = query.Where(tutor =>
                tutor.Nome.Valor.Contains(nome, StringComparison.OrdinalIgnoreCase));
        }

        if (filtros.Cpf.HasValue)
        {
            query = query.Where(tutor => tutor.Documento == filtros.Cpf.Value);
        }

        if (filtros.Situacao.HasValue)
        {
            query = query.Where(tutor => tutor.Situacao == filtros.Situacao.Value);
        }

        int total = query.Count();

        query = Ordenar(query, filtros);

        Tutor[] tutores = query
            .Skip((filtros.Pagina - 1) * filtros.TamanhoPagina)
            .Take(filtros.TamanhoPagina)
            .ToArray();

        return new PesquisaDeTutores(
            tutores.Select(MapearResumo).ToArray(),
            filtros.Pagina,
            filtros.TamanhoPagina,
            total);
    }

    public Task SalvarAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    private static IOrderedEnumerable<Tutor> Ordenar(
        IEnumerable<Tutor> query,
        FiltrosDePesquisaDeTutores filtros)
    {
        return (filtros.OrdenarPor, filtros.Direcao) switch
        {
            (OrdenacaoDeTutores.CriadoEm, DirecaoDaOrdenacao.Desc) => query
                .OrderByDescending(tutor => tutor.CriadoEm)
                .ThenBy(tutor => tutor.Id.Valor),
            (OrdenacaoDeTutores.CriadoEm, _) => query
                .OrderBy(tutor => tutor.CriadoEm)
                .ThenBy(tutor => tutor.Id.Valor),
            (OrdenacaoDeTutores.Nome, DirecaoDaOrdenacao.Desc) => query
                .OrderByDescending(tutor => tutor.Nome.Valor, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tutor => tutor.Id.Valor),
            _ => query
                .OrderBy(tutor => tutor.Nome.Valor, StringComparer.OrdinalIgnoreCase)
                .ThenBy(tutor => tutor.Id.Valor)
        };
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
}
