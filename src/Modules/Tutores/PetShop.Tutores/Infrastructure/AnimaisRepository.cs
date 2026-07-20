using Microsoft.EntityFrameworkCore;

using PetShop.Tutores.Application;
using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Infrastructure;

internal sealed class AnimaisRepository : IAnimaisRepository
{
    private readonly DbContext _dbContext;

    public AnimaisRepository(DbContext dbContext)
    {
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

    public Task<bool> ExisteTutorResponsavelAsync(
        TutorResponsavel tutorResponsavel,
        CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .AnyAsync(tutor => tutor.Id == TutorId.Criar(tutorResponsavel.TutorId), cancellationToken);

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
        Animal[] animaisDoTenant = await _dbContext.Set<Animal>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
        IEnumerable<Animal> query = animaisDoTenant;

        if (!string.IsNullOrWhiteSpace(filtros.Nome))
        {
            string nome = filtros.Nome.Trim();
            query = query.Where(animal =>
                animal.Nome.Valor.Contains(nome, StringComparison.OrdinalIgnoreCase));
        }

        if (filtros.TutorResponsavel.HasValue)
        {
            query = query.Where(animal =>
                animal.TutorResponsavel == filtros.TutorResponsavel.Value);
        }

        if (filtros.Especie.HasValue)
        {
            query = query.Where(animal =>
                string.Equals(
                    animal.Especie.Valor,
                    filtros.Especie.Value.Valor,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (filtros.Situacao.HasValue)
        {
            query = query.Where(animal => animal.Situacao == filtros.Situacao.Value);
        }

        int total = query.Count();

        query = Ordenar(query, filtros);

        Animal[] animais = query
            .Skip((filtros.Pagina - 1) * filtros.TamanhoPagina)
            .Take(filtros.TamanhoPagina)
            .ToArray();

        return new PesquisaDeAnimais(
            animais.Select(MapearResumo).ToArray(),
            filtros.Pagina,
            filtros.TamanhoPagina,
            total);
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

    private static IOrderedEnumerable<Animal> Ordenar(
        IEnumerable<Animal> query,
        FiltrosDePesquisaDeAnimais filtros)
    {
        return (filtros.OrdenarPor, filtros.Direcao) switch
        {
            (OrdenacaoDeAnimais.CriadoEm, DirecaoDaOrdenacao.Desc) => query
                .OrderByDescending(animal => animal.CriadoEm)
                .ThenBy(animal => animal.Id.Valor),
            (OrdenacaoDeAnimais.CriadoEm, _) => query
                .OrderBy(animal => animal.CriadoEm)
                .ThenBy(animal => animal.Id.Valor),
            (OrdenacaoDeAnimais.Nome, DirecaoDaOrdenacao.Desc) => query
                .OrderByDescending(animal => animal.Nome.Valor, StringComparer.OrdinalIgnoreCase)
                .ThenBy(animal => animal.Id.Valor),
            _ => query
                .OrderBy(animal => animal.Nome.Valor, StringComparer.OrdinalIgnoreCase)
                .ThenBy(animal => animal.Id.Valor)
        };
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
            _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null)
        };
}
