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

        RouteGroupBuilder animais = endpoints
            .MapGroup("/animais")
            .WithTags("Animais");

        if (authorizationPolicy is null)
        {
            animais.RequireAuthorization();
        }
        else
        {
            animais.RequireAuthorization(authorizationPolicy);
        }

        animais.MapPost(
                string.Empty,
                async (
                    CadastrarAnimalRequest request,
                    AnimaisApplicationService animaisService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        AnimalDetalhe animal = await animaisService.CadastrarAsync(
                            new CadastrarAnimalCommand(
                                request.TutorResponsavelId,
                                request.Nome,
                                request.Especie,
                                request.Raca,
                                request.Sexo,
                                request.DataDeNascimento,
                                request.CorOuPelagem,
                                request.ObservacaoCadastral),
                            cancellationToken);

                        return Results.Created(
                            $"/animais/{animal.AnimalId:D}",
                            AnimalResponse.From(animal));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (TutorResponsavelNaoEncontradoException)
                    {
                        return TutorResponsavelNaoEncontrado();
                    }
                })
            .WithName("CadastrarAnimal")
            .Produces<AnimalResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        animais.MapGet(
                "/{animalId:guid}",
                async (
                    Guid animalId,
                    AnimaisApplicationService animaisService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        AnimalDetalhe animal = await animaisService.ConsultarPorIdAsync(animalId, cancellationToken);

                        return Results.Ok(AnimalResponse.From(animal));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (AnimalNaoEncontradoException)
                    {
                        return AnimalNaoEncontrado();
                    }
                })
            .WithName("ConsultarAnimalPorId")
            .Produces<AnimalResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        animais.MapPut(
                "/{animalId:guid}",
                async (
                    Guid animalId,
                    AtualizarAnimalRequest request,
                    AnimaisApplicationService animaisService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        AnimalDetalhe animal = await animaisService.AtualizarAsync(
                            new AtualizarAnimalCommand(
                                animalId,
                                request.Nome,
                                request.Especie,
                                request.Raca,
                                request.Sexo,
                                request.DataDeNascimento,
                                request.CorOuPelagem,
                                request.ObservacaoCadastral),
                            cancellationToken);

                        return Results.Ok(AnimalResponse.From(animal));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (AnimalNaoEncontradoException)
                    {
                        return AnimalNaoEncontrado();
                    }
                    catch (ConflitoDeConcorrenciaDoAnimalException)
                    {
                        return ConflitoDeConcorrenciaDoAnimal();
                    }
                })
            .WithName("AtualizarAnimal")
            .Produces<AnimalResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        animais.MapPost(
                "/{animalId:guid}/transferencias-de-responsabilidade",
                async (
                    Guid animalId,
                    TransferirResponsabilidadeDoAnimalRequest request,
                    AnimaisApplicationService animaisService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        AnimalDetalhe animal = await animaisService.TransferirResponsabilidadeAsync(
                            new TransferirResponsabilidadeDoAnimalCommand(
                                animalId,
                                request.NovoTutorId,
                                request.Versao,
                                request.Motivo),
                            cancellationToken);

                        return Results.Ok(AnimalResponse.From(animal));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (AnimalNaoEncontradoException)
                    {
                        return AnimalNaoEncontrado();
                    }
                    catch (TutorResponsavelNaoEncontradoException)
                    {
                        return TutorResponsavelNaoEncontrado();
                    }
                    catch (TutorResponsavelInativoException)
                    {
                        return TutorResponsavelInativo();
                    }
                    catch (MesmoTutorResponsavelException)
                    {
                        return MesmoTutorResponsavel();
                    }
                    catch (ConflitoDeConcorrenciaDoAnimalException)
                    {
                        return ConflitoDeConcorrenciaDoAnimal();
                    }
                    catch (SubjectAutenticadoNaoEncontradoException)
                    {
                        return SubjectAutenticadoNaoEncontrado();
                    }
                })
            .WithName("TransferirResponsabilidadeDoAnimal")
            .Produces<AnimalResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        animais.MapGet(
                string.Empty,
                async (
                    int? pagina,
                    int? tamanhoPagina,
                    string? nome,
                    Guid? tutorResponsavelId,
                    string? especie,
                    string? situacao,
                    string? ordenarPor,
                    string? direcao,
                    AnimaisApplicationService animaisService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        PesquisaDeAnimais pesquisa = await animaisService.PesquisarAsync(
                            new PesquisarAnimaisQuery(
                                pagina,
                                tamanhoPagina,
                                nome,
                                tutorResponsavelId,
                                especie,
                                situacao,
                                ordenarPor,
                                direcao),
                            cancellationToken);

                        return Results.Ok(PesquisarAnimaisResponse.From(pesquisa));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                })
            .WithName("PesquisarAnimais")
            .Produces<PesquisarAnimaisResponse>()
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        animais.MapPost(
                "/{animalId:guid}/inativacao",
                async (
                    Guid animalId,
                    AnimaisApplicationService animaisService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        AnimalDetalhe animal = await animaisService.InativarAsync(animalId, cancellationToken);

                        return Results.Ok(AnimalResponse.From(animal));
                    }
                    catch (EntradaInvalidaException ex)
                    {
                        return EntradaInvalida(ex);
                    }
                    catch (AnimalNaoEncontradoException)
                    {
                        return AnimalNaoEncontrado();
                    }
                    catch (AnimalJaInativoException)
                    {
                        return Results.Problem(
                            statusCode: StatusCodes.Status409Conflict,
                            title: "Animal ja inativo.",
                            detail: "O animal informado ja esta inativo.",
                            type: "urn:petshop:error:resource.conflict");
                    }
                    catch (ConflitoDeConcorrenciaDoAnimalException)
                    {
                        return ConflitoDeConcorrenciaDoAnimal();
                    }
                })
            .WithName("InativarAnimal")
            .Produces<AnimalResponse>()
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

    private static IResult AnimalNaoEncontrado() =>
        Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Animal nao encontrado.",
            detail: "O animal solicitado nao existe ou nao esta visivel no tenant atual.",
            type: "urn:petshop:error:resource.not_found");

    private static IResult TutorResponsavelNaoEncontrado() =>
        Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Tutor responsavel nao encontrado.",
            detail: "O tutor responsavel informado nao existe ou nao esta visivel no tenant atual.",
            type: "urn:petshop:error:resource.not_found");

    private static IResult TutorResponsavelInativo() =>
        Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Tutor responsavel inativo.",
            detail: "O novo tutor responsavel informado esta inativo.",
            type: "urn:petshop:error:resource.conflict");

    private static IResult MesmoTutorResponsavel() =>
        Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Tutor responsavel inalterado.",
            detail: "O novo tutor responsavel deve ser diferente do tutor atual.",
            type: "urn:petshop:error:resource.conflict");

    private static IResult ConflitoDeConcorrenciaDoAnimal() =>
        Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Versao do animal desatualizada.",
            detail: "Recarregue o animal e tente novamente com a versao atual.",
            type: "urn:petshop:error:resource.conflict");

    private static IResult SubjectAutenticadoNaoEncontrado() =>
        Results.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Subject autenticado obrigatorio.",
            detail: "A identidade autenticada deve possuir subject para registrar a transferencia.",
            type: "urn:petshop:error:auth.forbidden");

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

    private sealed class CadastrarAnimalRequest
    {
        public Guid TutorResponsavelId { get; init; }

        public string? Nome { get; init; }

        public string? Especie { get; init; }

        public string? Raca { get; init; }

        public string? Sexo { get; init; }

        public DateOnly? DataDeNascimento { get; init; }

        public string? CorOuPelagem { get; init; }

        public string? ObservacaoCadastral { get; init; }
    }

    private sealed class AtualizarAnimalRequest
    {
        public string? Nome { get; init; }

        public string? Especie { get; init; }

        public string? Raca { get; init; }

        public string? Sexo { get; init; }

        public DateOnly? DataDeNascimento { get; init; }

        public string? CorOuPelagem { get; init; }

        public string? ObservacaoCadastral { get; init; }
    }

    private sealed class TransferirResponsabilidadeDoAnimalRequest
    {
        public Guid NovoTutorId { get; init; }

        public int Versao { get; init; }

        public string? Motivo { get; init; }
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

    private sealed record AnimalResponse(
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
        int Versao,
        DateTimeOffset? InativadoEm)
    {
        public static AnimalResponse From(AnimalDetalhe animal) =>
            new(
                animal.AnimalId,
                animal.TutorResponsavelId,
                animal.Nome,
                animal.Especie,
                animal.Raca,
                animal.Sexo,
                animal.DataDeNascimento,
                animal.CorOuPelagem,
                animal.ObservacaoCadastral,
                animal.Situacao,
                animal.CriadoEm,
                animal.AtualizadoEm,
                animal.Versao,
                animal.InativadoEm);
    }

    private sealed record AnimalResumoResponse(
        Guid AnimalId,
        Guid TutorResponsavelId,
        string Nome,
        string Especie,
        string Situacao)
    {
        public static AnimalResumoResponse From(AnimalResumo animal) =>
            new(
                animal.AnimalId,
                animal.TutorResponsavelId,
                animal.Nome,
                animal.Especie,
                animal.Situacao);
    }

    private sealed record PesquisarAnimaisResponse(
        IReadOnlyCollection<AnimalResumoResponse> Itens,
        int Pagina,
        int TamanhoPagina,
        int Total)
    {
        public static PesquisarAnimaisResponse From(PesquisaDeAnimais pesquisa) =>
            new(
                pesquisa.Itens.Select(AnimalResumoResponse.From).ToArray(),
                pesquisa.Pagina,
                pesquisa.TamanhoPagina,
                pesquisa.Total);
    }
}
