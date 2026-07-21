using PetShop.Tutores.Domain;

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

internal sealed record AtualizarAnimalCommand(
    Guid AnimalId,
    string? Nome,
    string? Especie,
    string? Raca,
    string? Sexo,
    DateOnly? DataDeNascimento,
    string? CorOuPelagem,
    string? ObservacaoCadastral);

internal sealed record TransferirResponsabilidadeDoAnimalCommand(
    Guid AnimalId,
    Guid NovoTutorId,
    int Versao,
    string? Motivo);

internal sealed record RegistrarFalecimentoDoAnimalCommand(
    Guid AnimalId,
    DateOnly? DataDoFalecimento);

internal sealed record PesquisarAnimaisQuery(
    int? Pagina,
    int? TamanhoPagina,
    string? Nome,
    Guid? TutorResponsavelId,
    string? Especie,
    string? Situacao,
    string? OrdenarPor,
    string? Direcao);

internal sealed record AnimalDetalhe(
    Guid AnimalId,
    Guid TutorResponsavelId,
    string Nome,
    string Especie,
    string? Raca,
    string Sexo,
    DateOnly? DataDeNascimento,
    DateOnly? DataDoFalecimento,
    string? CorOuPelagem,
    string? ObservacaoCadastral,
    string Situacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm,
    int Versao,
    DateTimeOffset? InativadoEm);

internal sealed record AnimalResumo(
    Guid AnimalId,
    Guid TutorResponsavelId,
    string Nome,
    string Especie,
    string Situacao);

internal sealed record PesquisaDeAnimais(
    IReadOnlyCollection<AnimalResumo> Itens,
    int Pagina,
    int TamanhoPagina,
    int Total);

internal sealed record FiltrosDePesquisaDeAnimais(
    int Pagina,
    int TamanhoPagina,
    string? Nome,
    TutorResponsavel? TutorResponsavel,
    Especie? Especie,
    SituacaoDoAnimal? Situacao,
    OrdenacaoDeAnimais OrdenarPor,
    DirecaoDaOrdenacao Direcao);

internal enum OrdenacaoDeAnimais
{
    Nome,
    CriadoEm
}
