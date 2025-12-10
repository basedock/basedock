namespace BaseDock.Infrastructure.Security;

using System;
using System.Security.Cryptography;
using System.Text;
using BaseDock.Application.Abstractions.Security;
using NSec.Cryptography;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Prefix = "argon2id";

    private readonly PasswordBasedKeyDerivationAlgorithm _algorithm;

    public Argon2PasswordHasher()
    {
        var parameters = new Argon2Parameters
        {
            DegreeOfParallelism = 1,
            MemorySize = 19456,
            NumberOfPasses = 2
        };

        _algorithm = PasswordBasedKeyDerivationAlgorithm.Argon2id(parameters);
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hash = _algorithm.DeriveBytes(passwordBytes, salt, HashSize);

        return $"{Prefix}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('$');
        if (parts.Length != 3 || parts[0] != Prefix)
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var actualHash = _algorithm.DeriveBytes(passwordBytes, salt, HashSize);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}
