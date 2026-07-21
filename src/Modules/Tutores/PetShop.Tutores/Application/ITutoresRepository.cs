using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal interface ITutoresRepository
{
    Task AdicionarAsync(Tutor tutor, CancellationToken cancellationToken);

    Task<Tutor?> ObterPorIdAsync(TutorId tutorId, CancellationToken cancellationToken);

    Task<Tutor?> ObterPorIdParaAtualizacaoAsync(TutorId tutorId, CancellationToken cancellationToken);

    Task<Tutor?> ObterPorIdSemTrackingAsync(TutorId tutorId, CancellationToken cancellationToken);

    Task<bool> ExisteDocumentoAsync(
        Cpf documento,
        TutorId? excetoTutorId,
        CancellationToken cancellationToken);

    Task<bool> ExisteAnimalAtivoVinculadoAsync(
        TutorId tutorId,
        CancellationToken cancellationToken);

    Task<PesquisaDeTutores> PesquisarAsync(
        FiltrosDePesquisaDeTutores filtros,
        CancellationToken cancellationToken);

    Task<TResult> ExecutarEmTransacaoAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operacao,
        CancellationToken cancellationToken);

    Task SalvarAsync(CancellationToken cancellationToken);
}
