using System.Text;

namespace PetShop.Tutores.Domain;

internal readonly record struct Telefone
{
    private Telefone(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Telefone Criar(string? valor)
    {
        string telefone = valor?.Trim() ?? string.Empty;

        if (telefone.Length == 0)
        {
            throw new ArgumentException("O telefone deve ser informado quando usado.", nameof(valor));
        }

        string digitos = Normalizar(telefone);

        if (telefone.StartsWith("+55", StringComparison.Ordinal) && digitos.Length is 12 or 13)
        {
            digitos = digitos[2..];
        }

        if (digitos.Length is not (10 or 11) || digitos.All(digito => digito == digitos[0]))
        {
            throw new ArgumentException("O telefone informado deve possuir DDD e numero validos.", nameof(valor));
        }

        return new Telefone(digitos);
    }

    public override string ToString() => Valor;

    private static string Normalizar(string telefone)
    {
        var digitos = new StringBuilder(capacity: 13);

        for (int indice = 0; indice < telefone.Length; indice++)
        {
            char caractere = telefone[indice];

            if (caractere is >= '0' and <= '9')
            {
                _ = digitos.Append(caractere);
                continue;
            }

            if (caractere == '+' && indice == 0)
            {
                continue;
            }

            if (caractere is '(' or ')' or '-' or ' ')
            {
                continue;
            }

            throw new ArgumentException("O telefone informado contem caracteres invalidos.", nameof(telefone));
        }

        return digitos.ToString();
    }
}
