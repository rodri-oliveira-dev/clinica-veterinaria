using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using PetShop.Tutores.Application;

namespace PetShop.Tutores;

public static class ModuloTutoresEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapModuloTutores(
        this IEndpointRouteBuilder endpoints,
        string? authorizationPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        RouteGroupBuilder tutores = endpoints
            .MapGroup("/tutores")
            .WithTags("Tutores");

        if (authorizationPolicy is null)
        {
            tutores.RequireAuthorization();
        }
        else
        {
            tutores.RequireAuthorization(authorizationPolicy);
        }

        tutores.MapPost(
                string.Empty,
                async (
                    CadastrarTutorRequest request,
                    TutoresApplicationService tutoresService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        TutorDetalhe tutor = await tutoresService.CadastrarAsync(
                            new CadastrarTutorCommand(
                                request.Nome,
                                request.Cpf,
                                request.Email,
                                request.Telefone),
                            cancellationToken);

                        return Results.Created(
                            $"/tutores/{tutor.TutorId:D}",
                            TutorResponse.From(tutor));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (DocumentoDoTutorDuplicadoException)
                    {
                        return ConflitoDocumento();
                    }
                })
            .WithName("CadastrarTutor")
            .Produces<TutorResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        tutores.MapGet(
                "/{tutorId:guid}",
                async (
                    Guid tutorId,
                    TutoresApplicationService tutoresService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        TutorDetalhe tutor = await tutoresService.ConsultarPorIdAsync(tutorId, cancellationToken);

                        return Results.Ok(TutorResponse.From(tutor));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (TutorNaoEncontradoException)
                    {
                        return NaoEncontrado();
                    }
                })
            .WithName("ConsultarTutorPorId")
            .Produces<TutorResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tutores.MapPut(
                "/{tutorId:guid}",
                async (
                    Guid tutorId,
                    AtualizarTutorRequest request,
                    TutoresApplicationService tutoresService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        TutorDetalhe tutor = await tutoresService.AtualizarAsync(
                            new AtualizarTutorCommand(
                                tutorId,
                                request.Nome,
                                request.Cpf,
                                request.Email,
                                request.Telefone),
                            cancellationToken);

                        return Results.Ok(TutorResponse.From(tutor));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (TutorNaoEncontradoException)
                    {
                        return NaoEncontrado();
                    }
                    catch (DocumentoDoTutorDuplicadoException)
                    {
                        return ConflitoDocumento();
                    }
                })
            .WithName("AtualizarTutor")
            .Produces<TutorResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        tutores.MapGet(
                string.Empty,
                async (
                    int? pagina,
                    int? tamanhoPagina,
                    string? nome,
                    string? cpf,
                    string? situacao,
                    string? ordenarPor,
                    string? direcao,
                    TutoresApplicationService tutoresService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        PesquisaDeTutores pesquisa = await tutoresService.PesquisarAsync(
                            new PesquisarTutoresQuery(
                                pagina,
                                tamanhoPagina,
                                nome,
                                cpf,
                                situacao,
                                ordenarPor,
                                direcao),
                            cancellationToken);

                        return Results.Ok(PesquisarTutoresResponse.From(pesquisa));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                })
            .WithName("PesquisarTutores")
            .Produces<PesquisarTutoresResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        tutores.MapPost(
                "/{tutorId:guid}/inativacao",
                async (
                    Guid tutorId,
                    TutoresApplicationService tutoresService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        TutorDetalhe tutor = await tutoresService.InativarAsync(tutorId, cancellationToken);

                        return Results.Ok(TutorResponse.From(tutor));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (TutorNaoEncontradoException)
                    {
                        return NaoEncontrado();
                    }
                    catch (TutorJaInativoException)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Tutor ja inativo.",
                            detail: "O tutor informado ja esta inativo.",
                            type: "urn:petshop:error:resource.conflict");
                    }
                })
            .WithName("InativarTutor")
            .Produces<TutorResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static IResult EntradaInvalida(EntradaInvalidaException exception) =>
        Results.ValidationProblem(
            exception.Erros,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Entrada invalida.",
            detail: "A requisicao contem campos invalidos para o caso de uso.",
            type: "urn:petshop:error:request.invalid");

    private static IResult NaoEncontrado() =>
        Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Tutor nao encontrado.",
            detail: "O tutor solicitado nao existe ou nao esta visivel no tenant atual.",
            type: "urn:petshop:error:resource.not_found");

    private static IResult ConflitoDocumento() =>
        Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "CPF ja cadastrado.",
            detail: "Ja existe tutor cadastrado com este CPF no tenant atual.",
            type: "urn:petshop:error:resource.conflict");

    private sealed class CadastrarTutorRequest
    {
        public string? Nome { get; init; }

        public string? Cpf { get; init; }

        public string? Email { get; init; }

        public string? Telefone { get; init; }
    }

    private sealed class AtualizarTutorRequest
    {
        public string? Nome { get; init; }

        public string? Cpf { get; init; }

        public string? Email { get; init; }

        public string? Telefone { get; init; }
    }

    private sealed record TutorResponse(
        Guid TutorId,
        string Nome,
        string? CpfMascarado,
        string? Email,
        string? Telefone,
        string Situacao,
        DateTimeOffset CriadoEm,
        DateTimeOffset AtualizadoEm,
        DateTimeOffset? InativadoEm)
    {
        public static TutorResponse From(TutorDetalhe tutor) =>
            new(
                tutor.TutorId,
                tutor.Nome,
                tutor.CpfMascarado,
                tutor.Email,
                tutor.Telefone,
                tutor.Situacao,
                tutor.CriadoEm,
                tutor.AtualizadoEm,
                tutor.InativadoEm);
    }

    private sealed record TutorResumoResponse(
        Guid TutorId,
        string Nome,
        string? CpfMascarado,
        string Situacao)
    {
        public static TutorResumoResponse From(TutorResumo tutor) =>
            new(
                tutor.TutorId,
                tutor.Nome,
                tutor.CpfMascarado,
                tutor.Situacao);
    }

    private sealed record PesquisarTutoresResponse(
        IReadOnlyCollection<TutorResumoResponse> Itens,
        int Pagina,
        int TamanhoPagina,
        int Total)
    {
        public static PesquisarTutoresResponse From(PesquisaDeTutores pesquisa) =>
            new(
                pesquisa.Itens.Select(TutorResumoResponse.From).ToArray(),
                pesquisa.Pagina,
                pesquisa.TamanhoPagina,
                pesquisa.Total);
    }
}
