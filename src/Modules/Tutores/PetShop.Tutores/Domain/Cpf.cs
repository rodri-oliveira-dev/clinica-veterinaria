using System.Text;

namespace PetShop.Tutores.Domain;

internal readonly record struct Cpf
{
    private Cpf(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Cpf Criar(string? valor)
    {
        string digitos = Normalizar(valor);

        if (!EhValido(digitos))
        {
            throw new ArgumentException("O CPF informado deve ser valido.", nameof(valor));
        }

        return new Cpf(digitos);
    }

    public override string ToString() => Valor;

    private static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O CPF deve ser informado quando usado.", nameof(valor));
        }

        var digitos = new StringBuilder(capacity: 11);

        foreach (char caractere in valor.Trim())
        {
            if (caractere is >= '0' and <= '9')
            {
                _ = digitos.Append(caractere);
                continue;
            }

            if (caractere is '.' or '-' or ' ')
            {
                continue;
            }

            throw new ArgumentException("O CPF informado contem caracteres invalidos.", nameof(valor));
        }

        return digitos.ToString();
    }

    private static bool EhValido(string digitos)
    {
        if (digitos.Length != 11)
        {
            return false;
        }

        if (digitos.All(digito => digito == digitos[0]))
        {
            return false;
        }

        return CalcularDigito(digitos, tamanho: 9) == DigitoEm(digitos, indice: 9) &&
            CalcularDigito(digitos, tamanho: 10) == DigitoEm(digitos, indice: 10);
    }

    private static int CalcularDigito(string digitos, int tamanho)
    {
        int soma = 0;

        for (int indice = 0; indice < tamanho; indice++)
        {
            soma += DigitoEm(digitos, indice) * (tamanho + 1 - indice);
        }

        int resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }

    private static int DigitoEm(string digitos, int indice) => digitos[indice] - '0';
}
