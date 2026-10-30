using System.Security.Cryptography;

namespace AttendanceUI.Services;

public static class PasswordHelper
{
    private const char Delimiter = '$';
    private const string Algorithm = "PBKDF2";
    private const int DefaultIterationCount = 100000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    /// <summary>
    /// Hashes a password with PBKDF2-SHA256.
    /// </summary>
    /// <param name="password">The plaintext password.</param>
    /// <param name="iterationCount">
    /// Work factor. Leave at the default for any real credential. It is only lowered
    /// for throwaway demo data (see <c>SqliteSeeder</c>), where hashing hundreds of
    /// accounts at full cost would stall the app. <see cref="Verify"/> reads the count
    /// back out of the stored hash, so records written at any cost still verify.
    /// </param>
    public static string Hash(string password, int iterationCount = DefaultIterationCount)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterationCount,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            Delimiter,
            Algorithm,
            iterationCount,
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
