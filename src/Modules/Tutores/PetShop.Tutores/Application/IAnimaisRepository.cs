using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal interface IAnimaisRepository
{
    Task AdicionarAsync(Animal animal, CancellationToken cancellationToken);

    Task<Animal?> ObterPorIdAsync(AnimalId animalId, CancellationToken cancellationToken);

    Task<Animal?> ObterPorIdSemTrackingAsync(AnimalId animalId, CancellationToken cancellationToken);

    Task<Tutor?> ObterTutorResponsavelPorIdAsync(TutorId tutorId, CancellationToken cancellationToken);

    Task<Tutor?> ObterTutorResponsavelPorIdParaAtualizacaoAsync(
        TutorId tutorId,
        CancellationToken cancellationToken);

    Task AdicionarTransferenciaAsync(
        TransferenciaDeResponsabilidadeDoAnimal transferencia,
        CancellationToken cancellationToken);

    Task<PesquisaDeAnimais> PesquisarAsync(
        FiltrosDePesquisaDeAnimais filtros,
        CancellationToken cancellationToken);

    Task<TResult> ExecutarEmTransacaoAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacao,
        CancellationToken cancellationToken);

    Task SalvarAsync(CancellationToken cancellationToken);
}
