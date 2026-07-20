using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal sealed class AnimaisApplicationService
{
    private readonly IContextoTenantAtual _contextoTenantAtual;
    private readonly IAnimaisRepository _repository;
    private readonly TimeProvider _timeProvider;

    public AnimaisApplicationService(
        IContextoTenantAtual contextoTenantAtual,
        IAnimaisRepository repository,
        TimeProvider timeProvider)
    {
        _contextoTenantAtual = contextoTenantAtual;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<AnimalDetalhe> CadastrarAsync(
        CadastrarAnimalCommand command,
        CancellationToken cancellationToken)
    {
        DadosDoAnimal dados = ValidarDadosDoAnimal(command);

        if (!await _repository.ExisteTutorResponsavelAsync(dados.TutorResponsavel, cancellationToken))
        {
            throw new TutorResponsavelNaoEncontradoException();
        }

        DateTimeOffset agora = _timeProvider.GetUtcNow();
        Animal animal = Animal.Cadastrar(
            AnimalId.Novo(),
            ObterTenantAtual(),
            dados.Nome,
            dados.Especie,
            dados.Raca,
            dados.Sexo,
            dados.DataDeNascimento,
            dados.CorOuPelagem,
            dados.ObservacaoCadastral,
            dados.TutorResponsavel,
            agora);

        await _repository.AdicionarAsync(animal, cancellationToken);
        await _repository.SalvarAsync(cancellationToken);

        return MapearDetalhe(animal);
    }

    public async Task<AnimalDetalhe> ConsultarPorIdAsync(
        Guid animalId,
        CancellationToken cancellationToken)
    {
        Animal? animal = await _repository.ObterPorIdSemTrackingAsync(
            ValidarAnimalId(animalId),
            cancellationToken);

        return animal is null ? throw new AnimalNaoEncontradoException() : MapearDetalhe(animal);
    }

    public async Task<AnimalDetalhe> InativarAsync(
        Guid animalId,
        CancellationToken cancellationToken)
    {
        Animal? animal = await _repository.ObterPorIdAsync(
            ValidarAnimalId(animalId),
            cancellationToken);

        if (animal is null)
        {
            throw new AnimalNaoEncontradoException();
        }

        try
        {
            animal.Inativar(_timeProvider.GetUtcNow());
        }
        catch (InvalidOperationException)
        {
            throw new AnimalJaInativoException();
        }

        await _repository.SalvarAsync(cancellationToken);

        return MapearDetalhe(animal);
    }

    private TenantId ObterTenantAtual()
    {
        if (!_contextoTenantAtual.TenantId.HasValue)
        {
            throw new InvalidOperationException(
                "O tenant autenticado deve estar resolvido antes de executar casos de uso de animais.");
        }

        return TenantId.Criar(_contextoTenantAtual.TenantId.Value);
    }

    private static AnimalId ValidarAnimalId(Guid animalId)
    {
        try
        {
            return AnimalId.Criar(animalId);
        }
        catch (ArgumentException ex)
        {
            throw new EntradaInvalidaException(new Dictionary<string, string[]>
            {
                ["animalId"] = [ex.Message]
            });
        }
    }

    private DadosDoAnimal ValidarDadosDoAnimal(CadastrarAnimalCommand command)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);

        TutorResponsavel? tutorResponsavel = CriarValor(
            () => TutorResponsavel.Criar(command.TutorResponsavelId),
            "tutorResponsavelId",
            erros);
        NomeDoAnimal? nome = CriarValor(
            () => NomeDoAnimal.Criar(command.Nome),
            "nome",
            erros);
        Especie? especie = CriarValor(
            () => Especie.Criar(command.Especie),
            "especie",
            erros);
        Raca? raca = string.IsNullOrWhiteSpace(command.Raca)
            ? null
            : CriarValor(() => Raca.Criar(command.Raca), "raca", erros);
        SexoDoAnimal sexo = ParseSexo(command.Sexo, erros);
        DataDeNascimento? dataDeNascimento = command.DataDeNascimento.HasValue
            ? CriarValor(
                () => DataDeNascimento.Criar(command.DataDeNascimento.Value, DataAtual()),
                "dataDeNascimento",
                erros)
            : null;
        CorOuPelagem? corOuPelagem = string.IsNullOrWhiteSpace(command.CorOuPelagem)
            ? null
            : CriarValor(() => CorOuPelagem.Criar(command.CorOuPelagem), "corOuPelagem", erros);
        ObservacaoCadastral? observacaoCadastral = string.IsNullOrWhiteSpace(command.ObservacaoCadastral)
            ? null
            : CriarValor(() => ObservacaoCadastral.Criar(command.ObservacaoCadastral), "observacaoCadastral", erros);

        if (erros.Count > 0 || tutorResponsavel is null || nome is null || especie is null)
        {
            throw new EntradaInvalidaException(erros);
        }

        return new DadosDoAnimal(
            tutorResponsavel.Value,
            nome.Value,
            especie.Value,
            raca,
            sexo,
            dataDeNascimento,
            corOuPelagem,
            observacaoCadastral);
    }

    private DateOnly DataAtual()
    {
        DateTimeOffset agora = _timeProvider.GetUtcNow();

        return DateOnly.FromDateTime(agora.UtcDateTime);
    }

    private static T? CriarValor<T>(
        Func<T> factory,
        string campo,
        Dictionary<string, string[]> erros)
        where T : struct
    {
        try
        {
            return factory();
        }
        catch (ArgumentException ex)
        {
            erros[campo] = [ex.Message];

            return null;
        }
    }

    private static SexoDoAnimal ParseSexo(
        string? valor,
        Dictionary<string, string[]> erros)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return SexoDoAnimal.NaoInformado;
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "naoinformado" or "nao_informado" or "nao-informado" => SexoDoAnimal.NaoInformado,
            "macho" => SexoDoAnimal.Macho,
            "femea" => SexoDoAnimal.Femea,
            _ => RegistrarErro(
                erros,
                "sexo",
                "O sexo do animal deve ser naoInformado, macho ou femea.",
                SexoDoAnimal.NaoInformado)
        };
    }

    private static T RegistrarErro<T>(
        Dictionary<string, string[]> erros,
        string campo,
        string mensagem,
        T fallback)
    {
        erros[campo] = [mensagem];

        return fallback;
    }

    private static AnimalDetalhe MapearDetalhe(Animal animal) =>
        new(
            animal.Id.Valor,
            animal.TutorResponsavel.TutorId,
            animal.Nome.Valor,
            animal.Especie.Valor,
            animal.Raca?.Valor,
            MapearSexo(animal.Sexo),
            animal.DataDeNascimento?.Valor,
            animal.CorOuPelagem?.Valor,
            animal.ObservacaoCadastral?.Valor,
            MapearSituacao(animal.Situacao),
            animal.CriadoEm,
            animal.AtualizadoEm,
            animal.InativadoEm);

    private static string MapearSexo(SexoDoAnimal sexo) =>
        sexo switch
        {
            SexoDoAnimal.NaoInformado => "naoInformado",
            SexoDoAnimal.Macho => "macho",
            SexoDoAnimal.Femea => "femea",
            _ => throw new ArgumentOutOfRangeException(nameof(sexo), sexo, null)
        };

    private static string MapearSituacao(SituacaoDoAnimal situacao) =>
        situacao switch
        {
            SituacaoDoAnimal.Ativo => "ativo",
            SituacaoDoAnimal.Inativo => "inativo",
            _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null)
        };

    private readonly record struct DadosDoAnimal(
        TutorResponsavel TutorResponsavel,
        NomeDoAnimal Nome,
        Especie Especie,
        Raca? Raca,
        SexoDoAnimal Sexo,
        DataDeNascimento? DataDeNascimento,
        CorOuPelagem? CorOuPelagem,
        ObservacaoCadastral? ObservacaoCadastral);
}
