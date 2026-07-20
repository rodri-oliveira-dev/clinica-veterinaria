using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal interface IAnimaisRepository
{
    Task AdicionarAsync(Animal animal, CancellationToken cancellationToken);

    Task<Animal?> ObterPorIdAsync(AnimalId animalId, CancellationToken cancellationToken);

    Task<Animal?> ObterPorIdSemTrackingAsync(AnimalId animalId, CancellationToken cancellationToken);

    Task<bool> ExisteTutorResponsavelAsync(
        TutorResponsavel tutorResponsavel,
        CancellationToken cancellationToken);

    Task<PesquisaDeAnimais> PesquisarAsync(
        FiltrosDePesquisaDeAnimais filtros,
        CancellationToken cancellationToken);

    Task SalvarAsync(CancellationToken cancellationToken);
}
