using System.Security.Cryptography;

namespace AttendanceUI.Services;

public static class PasswordHelper
{
    private const char Delimiter = '$';
    private const string Algorithm = "PBKDF2";
    private const int DefaultIterationCount = 100000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            DefaultIterationCount,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            Delimiter,
            Algorithm,
            DefaultIterationCount,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static bool Verify(string password, string passwordHash)
    {
        var segments = passwordHash.Split(Delimiter);
        if (segments.Length != 4 || segments[0] != Algorithm)
            return false;

        if (!int.TryParse(segments[1], out var iterationCount))
            return false;

        var salt = Convert.FromBase64String(segments[2]);
        var storedHash = Convert.FromBase64String(segments[3]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterationCount,
            HashAlgorithmName.SHA256,
            storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }
}
