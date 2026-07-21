using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal sealed class AnimaisApplicationService
{
    public const int PaginaPadrao = 1;
    public const int TamanhoPaginaPadrao = 20;
    public const int TamanhoPaginaMaximo = 100;

    private readonly IContextoTenantAtual _contextoTenantAtual;
    private readonly IContextoUsuarioAtual _contextoUsuarioAtual;
    private readonly IAnimaisRepository _repository;
    private readonly TimeProvider _timeProvider;

    public AnimaisApplicationService(
        IContextoTenantAtual contextoTenantAtual,
        IContextoUsuarioAtual contextoUsuarioAtual,
        IAnimaisRepository repository,
        TimeProvider timeProvider)
    {
        _contextoTenantAtual = contextoTenantAtual;
        _contextoUsuarioAtual = contextoUsuarioAtual;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<AnimalDetalhe> CadastrarAsync(
        CadastrarAnimalCommand command,
        CancellationToken cancellationToken)
    {
        DadosDoAnimal dados = ValidarDadosDoAnimal(
            command.Nome,
            command.Especie,
            command.Raca,
            command.Sexo,
            command.DataDeNascimento,
            command.CorOuPelagem,
            command.ObservacaoCadastral);
        TutorResponsavel tutorResponsavel = ValidarTutorResponsavel(command.TutorResponsavelId);
        Tutor? tutor = await _repository.ObterTutorResponsavelPorIdAsync(
            TutorId.Criar(tutorResponsavel.TutorId),
            cancellationToken);
        if (tutor is null)
        {
            throw new TutorResponsavelNaoEncontradoException();
        }

        if (!tutor.EstaAtivo)
        {
            throw new TutorResponsavelInativoException();
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
            tutorResponsavel,
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

    public async Task<AnimalDetalhe> AtualizarAsync(
        AtualizarAnimalCommand command,
        CancellationToken cancellationToken)
    {
        DadosDoAnimal dados = ValidarDadosDoAnimal(
            command.Nome,
            command.Especie,
            command.Raca,
            command.Sexo,
            command.DataDeNascimento,
            command.CorOuPelagem,
            command.ObservacaoCadastral);
        Animal animal = await ObterAnimalOuFalharAsync(
            ValidarAnimalId(command.AnimalId),
            cancellationToken);

        animal.AlterarCadastro(
            dados.Nome,
            dados.Especie,
            dados.Raca,
            dados.Sexo,
            dados.DataDeNascimento,
            dados.CorOuPelagem,
            dados.ObservacaoCadastral,
            _timeProvider.GetUtcNow());

        await _repository.SalvarAsync(cancellationToken);

        return MapearDetalhe(animal);
    }

    public Task<PesquisaDeAnimais> PesquisarAsync(
        PesquisarAnimaisQuery query,
        CancellationToken cancellationToken)
    {
        FiltrosDePesquisaDeAnimais filtros = ValidarFiltrosDePesquisa(query);

        return _repository.PesquisarAsync(filtros, cancellationToken);
    }

    public async Task<AnimalDetalhe> TransferirResponsabilidadeAsync(
        TransferirResponsabilidadeDoAnimalCommand command,
        CancellationToken cancellationToken)
    {
        AnimalId animalId = ValidarAnimalId(command.AnimalId);
        TutorId novoTutorId = ValidarTutorId(command.NovoTutorId, "novoTutorId");
        ValidarVersao(command.Versao);

        Animal animal = await ObterAnimalOuFalharAsync(animalId, cancellationToken);
        if (animal.Versao != command.Versao)
        {
            throw new ConflitoDeConcorrenciaDoAnimalException();
        }

        if (!animal.EstaAtivo)
        {
            throw new AnimalInativoException();
        }

        Tutor? novoTutor = await _repository.ObterTutorResponsavelPorIdAsync(novoTutorId, cancellationToken);
        if (novoTutor is null)
        {
            throw new TutorResponsavelNaoEncontradoException();
        }

        if (!novoTutor.EstaAtivo)
        {
            throw new TutorResponsavelInativoException();
        }

        TutorId tutorAnteriorId = animal.TutorResponsavelId;
        DateTimeOffset agora = _timeProvider.GetUtcNow();

        try
        {
            animal.TransferirResponsabilidade(TutorResponsavel.Criar(novoTutor.Id.Valor), agora);
        }
        catch (InvalidOperationException)
        {
            throw new MesmoTutorResponsavelException();
        }

        await _repository.AdicionarTransferenciaAsync(
            TransferenciaDeResponsabilidadeDoAnimal.Registrar(
                Guid.NewGuid(),
                animal.TenantId,
                animal.Id,
                tutorAnteriorId,
                novoTutor.Id,
                agora,
                ObterSubjectAtual(),
                command.Motivo),
            cancellationToken);

        await _repository.SalvarAsync(cancellationToken);

        return MapearDetalhe(animal);
    }

    public async Task<AnimalDetalhe> InativarAsync(
        Guid animalId,
        CancellationToken cancellationToken)
    {
        Animal animal = await ObterAnimalOuFalharAsync(ValidarAnimalId(animalId), cancellationToken);

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

    private async Task<Animal> ObterAnimalOuFalharAsync(
        AnimalId animalId,
        CancellationToken cancellationToken)
    {
        Animal? animal = await _repository.ObterPorIdAsync(animalId, cancellationToken);

        return animal ?? throw new AnimalNaoEncontradoException();
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

    private static TutorResponsavel ValidarTutorResponsavel(Guid tutorResponsavelId)
    {
        try
        {
            return TutorResponsavel.Criar(tutorResponsavelId);
        }
        catch (ArgumentException ex)
        {
            throw new EntradaInvalidaException(new Dictionary<string, string[]>
            {
                ["tutorResponsavelId"] = [ex.Message]
            });
        }
    }

    private static TutorId ValidarTutorId(Guid tutorId, string campo)
    {
        try
        {
            return TutorId.Criar(tutorId);
        }
        catch (ArgumentException ex)
        {
            throw new EntradaInvalidaException(new Dictionary<string, string[]>
            {
                [campo] = [ex.Message]
            });
        }
    }

    private static void ValidarVersao(int versao)
    {
        if (versao < 1)
        {
            throw new EntradaInvalidaException(new Dictionary<string, string[]>
            {
                ["versao"] = ["A versao do animal deve ser maior ou igual a 1."]
            });
        }
    }

    private string ObterSubjectAtual()
    {
        if (string.IsNullOrWhiteSpace(_contextoUsuarioAtual.Subject))
        {
            throw new SubjectAutenticadoNaoEncontradoException();
        }

        return _contextoUsuarioAtual.Subject.Trim();
    }

    private DadosDoAnimal ValidarDadosDoAnimal(
        string? nome,
        string? especie,
        string? raca,
        string? sexo,
        DateOnly? dataDeNascimento,
        string? corOuPelagem,
        string? observacaoCadastral)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);

        NomeDoAnimal? nomeDoAnimal = CriarValor(
            () => NomeDoAnimal.Criar(nome),
            "nome",
            erros);
        Especie? especieDoAnimal = CriarValor(
            () => Especie.Criar(especie),
            "especie",
            erros);
        Raca? racaDoAnimal = string.IsNullOrWhiteSpace(raca)
            ? null
            : CriarValor(() => Raca.Criar(raca), "raca", erros);
        SexoDoAnimal sexoDoAnimal = ParseSexo(sexo, erros);
        DataDeNascimento? nascimento = dataDeNascimento.HasValue
            ? CriarValor(
                () => DataDeNascimento.Criar(dataDeNascimento.Value, DataAtual()),
                "dataDeNascimento",
                erros)
            : null;
        CorOuPelagem? cor = string.IsNullOrWhiteSpace(corOuPelagem)
            ? null
            : CriarValor(() => CorOuPelagem.Criar(corOuPelagem), "corOuPelagem", erros);
        ObservacaoCadastral? observacao = string.IsNullOrWhiteSpace(observacaoCadastral)
            ? null
            : CriarValor(() => ObservacaoCadastral.Criar(observacaoCadastral), "observacaoCadastral", erros);

        if (erros.Count > 0 || nomeDoAnimal is null || especieDoAnimal is null)
        {
            throw new EntradaInvalidaException(erros);
        }

        return new DadosDoAnimal(
            nomeDoAnimal.Value,
            especieDoAnimal.Value,
            racaDoAnimal,
            sexoDoAnimal,
            nascimento,
            cor,
            observacao);
    }

    private DateOnly DataAtual()
    {
        DateTimeOffset agora = _timeProvider.GetUtcNow();

        return DateOnly.FromDateTime(agora.UtcDateTime);
    }

    private static FiltrosDePesquisaDeAnimais ValidarFiltrosDePesquisa(PesquisarAnimaisQuery query)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);
        int pagina = query.Pagina ?? PaginaPadrao;
        int tamanhoPagina = query.TamanhoPagina ?? TamanhoPaginaPadrao;

        if (pagina < 1)
        {
            erros["pagina"] = ["A pagina deve ser maior ou igual a 1."];
        }

        if (tamanhoPagina < 1 || tamanhoPagina > TamanhoPaginaMaximo)
        {
            erros["tamanhoPagina"] = [$"O tamanho da pagina deve estar entre 1 e {TamanhoPaginaMaximo}."];
        }

        TutorResponsavel? tutorResponsavel = query.TutorResponsavelId.HasValue
            ? CriarValor(() => TutorResponsavel.Criar(query.TutorResponsavelId.Value), "tutorResponsavelId", erros)
            : null;
        Especie? especie = string.IsNullOrWhiteSpace(query.Especie)
            ? null
            : CriarValor(() => Especie.Criar(query.Especie), "especie", erros);
        SituacaoDoAnimal? situacao = ParseSituacao(query.Situacao, erros);
        OrdenacaoDeAnimais ordenarPor = ParseOrdenacao(query.OrdenarPor, erros);
        DirecaoDaOrdenacao direcao = ParseDirecao(query.Direcao, erros);

        if (erros.Count > 0)
        {
            throw new EntradaInvalidaException(erros);
        }

        return new FiltrosDePesquisaDeAnimais(
            pagina,
            tamanhoPagina,
            string.IsNullOrWhiteSpace(query.Nome) ? null : query.Nome.Trim(),
            tutorResponsavel,
            especie,
            situacao,
            ordenarPor,
            direcao);
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

    private static SituacaoDoAnimal? ParseSituacao(
        string? valor,
        Dictionary<string, string[]> erros)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "ativo" => SituacaoDoAnimal.Ativo,
            "inativo" => SituacaoDoAnimal.Inativo,
            _ => RegistrarErro<SituacaoDoAnimal?>(
                erros,
                "situacao",
                "A situacao deve ser ativo ou inativo.")
        };
    }

    private static OrdenacaoDeAnimais ParseOrdenacao(
        string? valor,
        Dictionary<string, string[]> erros)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return OrdenacaoDeAnimais.Nome;
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "nome" => OrdenacaoDeAnimais.Nome,
            "criadoem" or "criado_em" => OrdenacaoDeAnimais.CriadoEm,
            _ => RegistrarErro(
                erros,
                "ordenarPor",
                "A ordenacao deve ser nome ou criadoEm.",
                OrdenacaoDeAnimais.Nome)
        };
    }

    private static DirecaoDaOrdenacao ParseDirecao(
        string? valor,
        Dictionary<string, string[]> erros)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return DirecaoDaOrdenacao.Asc;
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "asc" => DirecaoDaOrdenacao.Asc,
            "desc" => DirecaoDaOrdenacao.Desc,
            _ => RegistrarErro(
                erros,
                "direcao",
                "A direcao deve ser asc ou desc.",
                DirecaoDaOrdenacao.Asc)
        };
    }

    private static T RegistrarErro<T>(
        Dictionary<string, string[]> erros,
        string campo,
        string mensagem,
        T fallback = default!)
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
            animal.Versao,
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
        NomeDoAnimal Nome,
        Especie Especie,
        Raca? Raca,
        SexoDoAnimal Sexo,
        DataDeNascimento? DataDeNascimento,
        CorOuPelagem? CorOuPelagem,
        ObservacaoCadastral? ObservacaoCadastral);
}
