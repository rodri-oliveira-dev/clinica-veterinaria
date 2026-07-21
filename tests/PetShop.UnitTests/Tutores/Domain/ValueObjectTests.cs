using PetShop.Tutores.Domain;

namespace PetShop.UnitTests.Tutores.Domain;

public sealed class ValueObjectTests
{
    [Fact]
    public void NomeDoTutor_Criar_NormalizaEspacosExternos()
    {
        NomeDoTutor nome = NomeDoTutor.Criar("  Ana Souza  ");

        Assert.Equal("Ana Souza", nome.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NomeDoTutor_Criar_ComValorVazio_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => NomeDoTutor.Criar(valor));
    }

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Cpf_Criar_ComCpfValido_NormalizaDigitos(string valor)
    {
        Cpf cpf = Cpf.Criar(valor);

        Assert.Equal("52998224725", cpf.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-24")]
    [InlineData("5299822472A")]
    public void Cpf_Criar_ComCpfInvalido_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => Cpf.Criar(valor));
    }

    [Theory]
    [InlineData("Tutor@Example.com", "tutor@example.com")]
    [InlineData(" tutor@example.com ", "tutor@example.com")]
    public void Email_Criar_ComEmailValido_NormalizaValor(string valor, string esperado)
    {
        Email email = Email.Criar(valor);

        Assert.Equal(esperado, email.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sem-arroba")]
    [InlineData("Tutor <tutor@example.com>")]
    public void Email_Criar_ComEmailInvalido_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => Email.Criar(valor));
    }

    [Theory]
    [InlineData("(11) 98765-4321", "11987654321")]
    [InlineData("+55 (11) 98765-4321", "11987654321")]
    [InlineData("1133334444", "1133334444")]
    public void Telefone_Criar_ComTelefoneValido_NormalizaDigitos(string valor, string esperado)
    {
        Telefone telefone = Telefone.Criar(valor);

        Assert.Equal(esperado, telefone.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("(11) 11111-1111")]
    [InlineData("(11) 98765-ABCD")]
    public void Telefone_Criar_ComTelefoneInvalido_Rejeita(string? valor)
    {
        Assert.Throws<ArgumentException>(() => Telefone.Criar(valor));
    }

    [Fact]
    public void ValueObjects_ComMesmoValorNormalizado_SaoIguais()
    {
        Assert.Equal(Cpf.Criar("529.982.247-25"), Cpf.Criar("52998224725"));
        Assert.Equal(Email.Criar("Tutor@Example.com"), Email.Criar("tutor@example.com"));
        Assert.Equal(Telefone.Criar("+55 (11) 98765-4321"), Telefone.Criar("11987654321"));
        Assert.Equal(NomeDoTutor.Criar("Ana Souza"), NomeDoTutor.Criar(" Ana Souza "));
    }
}
