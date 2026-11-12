using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AttendanceUI.Data;

/// <summary>
/// AES-256-GCM encryption for sensitive columns. <see cref="SchoolDbContext"/> attaches
/// value converters that call these methods, so values are encrypted on the way into the
/// database and decrypted on the way out: every page, report and export sees plain text.
///
/// Rows written before encryption was turned on are still plain text in the database.
/// They read back unchanged and are encrypted the next time they are saved, or all at once
/// by <see cref="Services.IDataEncryptionService.EncryptExistingAsync"/>.
///
/// Every device that shares a database must use the same key; data encrypted with a lost
/// key cannot be recovered.
/// </summary>
public static class DataEncryption
{
    /// <summary>Overrides the key from appsettings.json (Encryption:DataKey) when set.</summary>
    public const string KeyEnvironmentVariable = "EDUATTEND_DATA_KEY";

    /// <summary>Marks an encrypted text value: "enc:v1:" + base64(nonce | ciphertext | tag).</summary>
    public const string TextPrefix = "enc:v1:";

    /// <summary>Shown in place of a value that was encrypted with a different key.</summary>
    public const string Unreadable = "[unable to decrypt]";

    /// <summary>Marks an encrypted binary value: magic | nonce | ciphertext | tag.</summary>
    public static ReadOnlySpan<byte> BlobMagic => "EAENC1"u8;

    private const int NonceSize = 12;
    private const int TagSize = 16;

    private static byte[]? _key;

    /// <summary>
    /// Sets the key: the <see cref="KeyEnvironmentVariable"/> environment variable wins over
    /// <paramref name="configuredKey"/>. Both are base64 for 32 bytes (AES-256).
    /// </summary>
    public static void Configure(string? configuredKey)
    {
        var text = Environment.GetEnvironmentVariable(KeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(text))
            text = configuredKey;
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException(
                $"No data encryption key. Set Encryption:DataKey in appsettings.json or the {KeyEnvironmentVariable} environment variable.");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(text.Trim());
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("The data encryption key is not valid base64.");
        }

        if (key.Length != 32)
            throw new InvalidOperationException("The data encryption key must be 32 bytes (AES-256).");

        _key = key;
    }

    private static byte[] Key =>
        _key ?? throw new InvalidOperationException("Data encryption has not been configured.");

    /// <summary>
    /// A separate 32-byte key for another job (e.g. sealing records), derived from the data
    /// key with HKDF-SHA256 so the encryption key itself is never reused for anything else.
    /// </summary>
    internal static byte[] DeriveKey(string purpose) =>
        HKDF.DeriveKey(HashAlgorithmName.SHA256, Key, 32, info: Encoding.UTF8.GetBytes(purpose));

    /// <summary>
    /// A short, non-reversible identifier of the active key (first 8 bytes of its SHA-256),
    /// so two installs can confirm they share a key without ever showing the key itself.
    /// </summary>
    public static string KeyFingerprint =>
        Convert.ToHexString(SHA256.HashData(Key).AsSpan(0, 8)).ToLowerInvariant();

    /// <summary>Where the active key came from, for display only.</summary>
    public static string KeySource =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(KeyEnvironmentVariable))
            ? "appsettings.json"
            : "environment variable";

    public static bool IsEncrypted(string? value) =>
        value is not null && value.StartsWith(TextPrefix, StringComparison.Ordinal);

    public static bool IsEncrypted(byte[]? value) =>
        value is not null && value.AsSpan().StartsWith(BlobMagic);

    /// <summary>True when a MySQL json column holds an encrypted value (a JSON string).</summary>
    public static bool IsEncryptedJson(string? value) =>
        value is not null && value.StartsWith("\"" + TextPrefix, StringComparison.Ordinal);

    public static string EncryptText(string value) =>
        IsEncrypted(value) ? value : TextPrefix + Convert.ToBase64String(Seal(Encoding.UTF8.GetBytes(value)));

    /// <summary>Plain text passes through; a value from another key reads as <see cref="Unreadable"/>.</summary>
    public static string DecryptText(string value)
    {
        if (!IsEncrypted(value))
            return value;

        try
        {
            return Encoding.UTF8.GetString(Open(Convert.FromBase64String(value[TextPrefix.Length..])));
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            return Unreadable;
        }
    }

    public static byte[] EncryptBytes(byte[] value) =>
        IsEncrypted(value) ? value : [.. BlobMagic, .. Seal(value)];

    /// <summary>
    /// Plain bytes pass through. Bytes from another key are returned still encrypted, so
    /// saving the row again cannot destroy them.
    /// </summary>
    public static byte[] DecryptBytes(byte[] value)
    {
        if (!IsEncrypted(value))
            return value;

        try
        {
            return Open(value.AsSpan(BlobMagic.Length));
        }
        catch (CryptographicException)
        {
            return value;
        }
    }

    /// <summary>For a MySQL json column: the ciphertext is stored as a JSON string so the column accepts it.</summary>
    public static string EncryptJson(string value) =>
        IsEncryptedJson(value) ? value : JsonSerializer.Serialize(EncryptText(value));

    public static string DecryptJson(string value)
    {
        if (!IsEncryptedJson(value))
            return value;

        try
        {
            return DecryptText(JsonSerializer.Deserialize<string>(value) ?? string.Empty);
        }
        catch (JsonException)
        {
            return Unreadable;
        }
    }

    private static byte[] Seal(ReadOnlySpan<byte> plain)
    {
        var output = new byte[NonceSize + plain.Length + TagSize];
        var nonce = output.AsSpan(0, NonceSize);
        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(Key, TagSize);
        aes.Encrypt(nonce, plain, output.AsSpan(NonceSize, plain.Length), output.AsSpan(NonceSize + plain.Length));
        return output;
    }

    private static byte[] Open(ReadOnlySpan<byte> sealedData)
    {
        if (sealedData.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted value is too short.");

        var cipherLength = sealedData.Length - NonceSize - TagSize;
        var plain = new byte[cipherLength];

        using var aes = new AesGcm(Key, TagSize);
        aes.Decrypt(sealedData[..NonceSize], sealedData.Slice(NonceSize, cipherLength), sealedData[^TagSize..], plain);
        return plain;
    }
}
