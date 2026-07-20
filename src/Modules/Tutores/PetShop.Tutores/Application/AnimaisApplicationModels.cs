namespace PetShop.Tutores.Application;

internal sealed record CadastrarAnimalCommand(
    Guid TutorResponsavelId,
    string? Nome,
    string? Especie,
    string? Raca,
    string? Sexo,
    DateOnly? DataDeNascimento,
    string? CorOuPelagem,
    string? ObservacaoCadastral);

internal sealed record AnimalDetalhe(
    Guid AnimalId,
    Guid TutorResponsavelId,
    string Nome,
    string Especie,
    string? Raca,
    string Sexo,
    DateOnly? DataDeNascimento,
    string? CorOuPelagem,
    string? ObservacaoCadastral,
    string Situacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm,
    DateTimeOffset? InativadoEm);
