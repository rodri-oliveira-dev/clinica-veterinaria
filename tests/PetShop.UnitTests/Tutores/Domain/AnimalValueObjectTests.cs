using PetShop.Tutores.Domain;

namespace PetShop.UnitTests.Tutores.Domain;

public sealed class AnimalValueObjectTests
{
    private static readonly DateOnly Hoje = new(2026, 7, 20);

    [Theory]
    [InlineData("  Luna  ", "Luna")]
    [InlineData("Thor", "Thor")]
    public void NomeDoAnimal_Criar_NormalizaEspacosExternos(string valor, string esperado)
    {
        NomeDoAnimal nome = NomeDoAnimal.Criar(valor);

        Assert.Equal(esperado, nome.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeDoAnimal_Criar_ComValorVazio_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => NomeDoAnimal.Criar(valor));
    }

    [Fact]
    public void Especie_Criar_NormalizaEspacosExternos()
    {
        Especie especie = Especie.Criar("  Canina  ");

        Assert.Equal("Canina", especie.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Especie_Criar_ComValorVazio_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => Especie.Criar(valor));
    }

    [Fact]
    public void Raca_Criar_NormalizaEspacosExternos()
    {
        Raca raca = Raca.Criar("  SRD  ");

        Assert.Equal("SRD", raca.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Raca_Criar_ComValorVazio_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => Raca.Criar(valor));
    }

    [Fact]
    public void DataDeNascimento_Criar_ComDataHoje_Aceita()
    {
        DataDeNascimento dataDeNascimento = DataDeNascimento.Criar(Hoje, Hoje);

        Assert.Equal(Hoje, dataDeNascimento.Valor);
    }

    [Fact]
    public void DataDeNascimento_Criar_ComDataFutura_Rejeita()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            DataDeNascimento.Criar(Hoje.AddDays(1), Hoje));

        Assert.Contains("futuro", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DataDoFalecimento_Criar_ComDataHoje_Aceita()
    {
        DataDoFalecimento dataDoFalecimento = DataDoFalecimento.Criar(Hoje, Hoje);

        Assert.Equal(Hoje, dataDoFalecimento.Valor);
    }

    [Fact]
    public void DataDoFalecimento_Criar_ComDataFutura_Rejeita()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            DataDoFalecimento.Criar(Hoje.AddDays(1), Hoje));

        Assert.Contains("futuro", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CorOuPelagem_Criar_NormalizaEspacosExternos()
    {
        CorOuPelagem corOuPelagem = CorOuPelagem.Criar("  Preto e branco  ");

        Assert.Equal("Preto e branco", corOuPelagem.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CorOuPelagem_Criar_ComValorVazio_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => CorOuPelagem.Criar(valor));
    }

    [Fact]
    public void ObservacaoCadastral_Criar_NormalizaEspacosExternos()
    {
        ObservacaoCadastral observacao = ObservacaoCadastral.Criar("  Animal ansioso  ");

        Assert.Equal("Animal ansioso", observacao.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ObservacaoCadastral_Criar_ComValorVazio_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => ObservacaoCadastral.Criar(valor));
    }

    [Fact]
    public void TutorResponsavel_Criar_ComIdentificadorValido_CriaReferencia()
    {
        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        TutorResponsavel tutorResponsavel = TutorResponsavel.Criar(tutorId);

        Assert.Equal(tutorId, tutorResponsavel.TutorId);
    }

    [Fact]
    public void TutorResponsavel_Criar_ComIdentificadorVazio_Rejeita()
    {
        Assert.Throws<ArgumentException>(() => TutorResponsavel.Criar(Guid.Empty));
    }

    [Fact]
    public void ValueObjects_ComMesmoValorNormalizado_SaoIguais()
    {
        Guid tutorId = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

        Assert.Equal(NomeDoAnimal.Criar("Luna"), NomeDoAnimal.Criar(" Luna "));
        Assert.Equal(Especie.Criar("Canina"), Especie.Criar(" Canina "));
        Assert.Equal(Raca.Criar("SRD"), Raca.Criar(" SRD "));
        Assert.Equal(CorOuPelagem.Criar("Caramelo"), CorOuPelagem.Criar(" Caramelo "));
        Assert.Equal(ObservacaoCadastral.Criar("Observacao"), ObservacaoCadastral.Criar(" Observacao "));
        Assert.Equal(DataDeNascimento.Criar(Hoje, Hoje), DataDeNascimento.Criar(Hoje, Hoje));
        Assert.Equal(DataDoFalecimento.Criar(Hoje, Hoje), DataDoFalecimento.Criar(Hoje, Hoje));
        Assert.Equal(TutorResponsavel.Criar(tutorId), TutorResponsavel.Criar(tutorId));
    }
}
