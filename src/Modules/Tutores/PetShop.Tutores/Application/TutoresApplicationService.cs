using PetShop.Tutores.Domain;

namespace PetShop.Tutores.Application;

internal sealed class TutoresApplicationService
{
    public const int PaginaPadrao = 1;
    public const int TamanhoPaginaPadrao = 20;
    public const int TamanhoPaginaMaximo = 100;

    private readonly IContextoTenantAtual _contextoTenantAtual;
    private readonly ITutoresRepository _repository;
    private readonly TimeProvider _timeProvider;

    public TutoresApplicationService(
        IContextoTenantAtual contextoTenantAtual,
        ITutoresRepository repository,
        TimeProvider timeProvider)
    {
        _contextoTenantAtual = contextoTenantAtual;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<TutorDetalhe> CadastrarAsync(
        CadastrarTutorCommand command,
        CancellationToken cancellationToken)
    {
        DadosDoTutor dados = ValidarDadosDoTutor(
            command.Nome,
            command.Cpf,
            command.Email,
            command.Telefone);

        if (dados.Documento.HasValue &&
            await _repository.ExisteDocumentoAsync(dados.Documento.Value, excetoTutorId: null, cancellationToken))
        {
            throw new DocumentoDoTutorDuplicadoException();
        }

        Tutor tutor = Tutor.Cadastrar(
            TutorId.Novo(),
            ObterTenantAtual(),
            dados.Nome,
            dados.Documento,
            dados.Email,
            dados.Telefone,
            _timeProvider.GetUtcNow());

        await _repository.AdicionarAsync(tutor, cancellationToken);
        await _repository.SalvarAsync(cancellationToken);

        return MapearDetalhe(tutor);
    }

    public async Task<TutorDetalhe> ConsultarPorIdAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        Tutor tutor = await ObterTutorSemTrackingOuFalharAsync(tutorId, cancellationToken);

        return MapearDetalhe(tutor);
    }

    public async Task<TutorDetalhe> AtualizarAsync(
        AtualizarTutorCommand command,
        CancellationToken cancellationToken)
    {
        DadosDoTutor dados = ValidarDadosDoTutor(
            command.Nome,
            command.Cpf,
            command.Email,
            command.Telefone);
        TutorId tutorId = ValidarTutorId(command.TutorId);
        Tutor tutor = await ObterTutorOuFalharAsync(tutorId, cancellationToken);

        if (dados.Documento.HasValue &&
            await _repository.ExisteDocumentoAsync(dados.Documento.Value, tutorId, cancellationToken))
        {
            throw new DocumentoDoTutorDuplicadoException();
        }

        tutor.AlterarCadastro(
            dados.Nome,
            dados.Documento,
            dados.Email,
            dados.Telefone,
            _timeProvider.GetUtcNow());

        await _repository.SalvarAsync(cancellationToken);

        return MapearDetalhe(tutor);
    }

    public Task<PesquisaDeTutores> PesquisarAsync(
        PesquisarTutoresQuery query,
        CancellationToken cancellationToken)
    {
        FiltrosDePesquisaDeTutores filtros = ValidarFiltrosDePesquisa(query);

        return _repository.PesquisarAsync(filtros, cancellationToken);
    }

    public async Task<TutorDetalhe> InativarAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        return await _repository.ExecutarEmTransacaoAsync(
            async ct =>
            {
                TutorId tutorIdValidado = ValidarTutorId(tutorId);
                Tutor tutor = await ObterTutorParaAtualizacaoOuFalharAsync(tutorIdValidado, ct);

                if (await _repository.ExisteAnimalAtivoVinculadoAsync(tutorIdValidado, ct))
                {
                    throw new TutorComAnimaisAtivosVinculadosException();
                }

                try
                {
                    tutor.Inativar(_timeProvider.GetUtcNow());
                }
                catch (InvalidOperationException)
                {
                    throw new TutorJaInativoException();
                }

                await _repository.SalvarAsync(ct);

                return MapearDetalhe(tutor);
            },
            cancellationToken);
    }

    private async Task<Tutor> ObterTutorSemTrackingOuFalharAsync(
        Guid tutorId,
        CancellationToken cancellationToken)
    {
        Tutor? tutor = await _repository.ObterPorIdSemTrackingAsync(
            ValidarTutorId(tutorId),
            cancellationToken);

        return tutor ?? throw new TutorNaoEncontradoException();
    }

    private async Task<Tutor> ObterTutorOuFalharAsync(
        TutorId tutorId,
        CancellationToken cancellationToken)
    {
        Tutor? tutor = await _repository.ObterPorIdAsync(tutorId, cancellationToken);

        return tutor ?? throw new TutorNaoEncontradoException();
    }

    private async Task<Tutor> ObterTutorParaAtualizacaoOuFalharAsync(
        TutorId tutorId,
        CancellationToken cancellationToken)
    {
        Tutor? tutor = await _repository.ObterPorIdParaAtualizacaoAsync(tutorId, cancellationToken);

        return tutor ?? throw new TutorNaoEncontradoException();
    }

    private TenantId ObterTenantAtual()
    {
        if (!_contextoTenantAtual.TenantId.HasValue)
        {
            throw new InvalidOperationException(
                "O tenant autenticado deve estar resolvido antes de executar casos de uso de tutores.");
        }

        return TenantId.Criar(_contextoTenantAtual.TenantId.Value);
    }

    private static TutorId ValidarTutorId(Guid tutorId)
    {
        try
        {
            return TutorId.Criar(tutorId);
        }
        catch (ArgumentException ex)
        {
            throw new EntradaInvalidaException(new Dictionary<string, string[]>
            {
                ["tutorId"] = [ex.Message]
            });
        }
    }

    private static DadosDoTutor ValidarDadosDoTutor(
        string? nome,
        string? cpf,
        string? email,
        string? telefone)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);

        NomeDoTutor? nomeDoTutor = CriarValor(
            () => NomeDoTutor.Criar(nome),
            "nome",
            erros);
        Cpf? documento = string.IsNullOrWhiteSpace(cpf)
            ? null
            : CriarValor(() => Cpf.Criar(cpf), "cpf", erros);
        Email? emailDoTutor = string.IsNullOrWhiteSpace(email)
            ? null
            : CriarValor(() => Email.Criar(email), "email", erros);
        Telefone? telefoneDoTutor = string.IsNullOrWhiteSpace(telefone)
            ? null
            : CriarValor(() => Telefone.Criar(telefone), "telefone", erros);

        if (emailDoTutor is null && telefoneDoTutor is null)
        {
            erros["contato"] = ["Informe ao menos e-mail ou telefone do tutor."];
        }

        if (erros.Count > 0 || nomeDoTutor is null)
        {
            throw new EntradaInvalidaException(erros);
        }

        return new DadosDoTutor(nomeDoTutor.Value, documento, emailDoTutor, telefoneDoTutor);
    }

    private static FiltrosDePesquisaDeTutores ValidarFiltrosDePesquisa(PesquisarTutoresQuery query)
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

        Cpf? cpf = string.IsNullOrWhiteSpace(query.Cpf)
            ? null
            : CriarValor(() => Cpf.Criar(query.Cpf), "cpf", erros);
        SituacaoDoTutor? situacao = ParseSituacao(query.Situacao, erros);
        OrdenacaoDeTutores ordenarPor = ParseOrdenacao(query.OrdenarPor, erros);
        DirecaoDaOrdenacao direcao = ParseDirecao(query.Direcao, erros);

        if (erros.Count > 0)
        {
            throw new EntradaInvalidaException(erros);
        }

        return new FiltrosDePesquisaDeTutores(
            pagina,
            tamanhoPagina,
            string.IsNullOrWhiteSpace(query.Nome) ? null : query.Nome.Trim(),
            cpf,
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

    private static SituacaoDoTutor? ParseSituacao(
        string? valor,
        Dictionary<string, string[]> erros)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "ativo" => SituacaoDoTutor.Ativo,
            "inativo" => SituacaoDoTutor.Inativo,
            _ => RegistrarErro<SituacaoDoTutor?>(
                erros,
                "situacao",
                "A situacao deve ser ativo ou inativo.")
        };
    }

    private static OrdenacaoDeTutores ParseOrdenacao(
        string? valor,
        Dictionary<string, string[]> erros)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return OrdenacaoDeTutores.Nome;
        }

        return valor.Trim().ToLowerInvariant() switch
        {
            "nome" => OrdenacaoDeTutores.Nome,
            "criadoem" or "criado_em" => OrdenacaoDeTutores.CriadoEm,
            _ => RegistrarErro(
                erros,
                "ordenarPor",
                "A ordenacao deve ser nome ou criadoEm.",
                OrdenacaoDeTutores.Nome)
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

    private static TutorDetalhe MapearDetalhe(Tutor tutor) =>
        new(
            tutor.Id.Valor,
            tutor.Nome.Valor,
            MascararCpf(tutor.Documento),
            tutor.Email?.Valor,
            tutor.Telefone?.Valor,
            MapearSituacao(tutor.Situacao),
            tutor.CriadoEm,
            tutor.AtualizadoEm,
            tutor.InativadoEm);

    private static string MapearSituacao(SituacaoDoTutor situacao) =>
        situacao switch
        {
            SituacaoDoTutor.Ativo => "ativo",
            SituacaoDoTutor.Inativo => "inativo",
            _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null)
        };

    private static string? MascararCpf(Cpf? documento)
    {
        if (!documento.HasValue)
        {
            return null;
        }

        string valor = documento.Value.Valor;

        return $"***.***.***-{valor[^2..]}";
    }

    private readonly record struct DadosDoTutor(
        NomeDoTutor Nome,
        Cpf? Documento,
        Email? Email,
        Telefone? Telefone);
}
