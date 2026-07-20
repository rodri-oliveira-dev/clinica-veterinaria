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

    public Task<bool> ExisteTutorResponsavelAsync(
        TutorResponsavel tutorResponsavel,
        CancellationToken cancellationToken) =>
        _dbContext.Set<Tutor>()
            .AnyAsync(tutor => tutor.Id == TutorId.Criar(tutorResponsavel.TutorId), cancellationToken);

    public Task SalvarAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
