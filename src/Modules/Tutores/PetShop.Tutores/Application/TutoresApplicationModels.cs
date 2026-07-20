using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal sealed record CadastrarTutorCommand(
    string? Nome,
    string? Cpf,
    string? Email,
    string? Telefone);

internal sealed record AtualizarTutorCommand(
    Guid TutorId,
    string? Nome,
    string? Cpf,
    string? Email,
    string? Telefone);

internal sealed record PesquisarTutoresQuery(
    int? Pagina,
    int? TamanhoPagina,
    string? Nome,
    string? Cpf,
    string? Situacao,
    string? OrdenarPor,
    string? Direcao);

internal sealed record TutorDetalhe(
    Guid TutorId,
    string Nome,
    string? CpfMascarado,
    string? Email,
    string? Telefone,
    string Situacao,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm,
    DateTimeOffset? InativadoEm);

internal sealed record TutorResumo(
    Guid TutorId,
    string Nome,
    string? CpfMascarado,
    string Situacao);

internal sealed record PesquisaDeTutores(
    IReadOnlyCollection<TutorResumo> Itens,
    int Pagina,
    int TamanhoPagina,
    int Total);

internal sealed record FiltrosDePesquisaDeTutores(
    int Pagina,
    int TamanhoPagina,
    string? Nome,
    Cpf? Cpf,
    SituacaoDoTutor? Situacao,
    OrdenacaoDeTutores OrdenarPor,
    DirecaoDaOrdenacao Direcao);

internal enum OrdenacaoDeTutores
{
    Nome,
    CriadoEm
}

internal enum DirecaoDaOrdenacao
{
    Asc,
    Desc
}
