using System.Security.Cryptography;

namespace AgroVerde.Application.Common;

/// <summary>
/// Hash de senha com PBKDF2 (nativo do .NET, sem pacote extra).
/// Formato salvo: "{iteracoes}.{salt base64}.{hash base64}"
/// </summary>
public static class SenhaHasher
{
    private const int TamanhoSaltBytes = 16;
    private const int TamanhoHashBytes = 32;
    private const int Iteracoes = 100_000;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHashBytes);

        return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string hashArmazenado)
    {
        var partes = hashArmazenado.Split('.');

        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteracoes))
        {
            return false;
        }

        var salt = Convert.FromBase64String(partes[1]);
        var hashEsperado = Convert.FromBase64String(partes[2]);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
