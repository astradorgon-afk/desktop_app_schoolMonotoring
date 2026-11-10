using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using OpenMcdf;

namespace AttendanceUI.Services;

/// <summary>
/// Password-protects an Office Open XML file (.xlsx) the way Excel's "Encrypt with Password"
/// does: ECMA-376 Agile Encryption [MS-OFFCRYPTO 2.3.4.10–2.3.4.15] with AES-256-CBC and
/// SHA-512 (100,000 password-hash iterations), stored in a compound file next to the
/// <c>\u0006DataSpaces</c> streams Office expects. Excel, LibreOffice and other Office readers
/// ask for the password when the file is opened; nothing else is needed to open it.
/// </summary>
public static class OfficeEncryption
{
    private const int SpinCount = 100_000;
    private const int KeyBytes = 32;      // AES-256
    private const int BlockSize = 16;     // AES block
    private const int SaltSize = 16;
    private const int HashSize = 64;      // SHA-512
    private const int SegmentSize = 4096; // EncryptedPackage is encrypted in 4 KB segments

    // Block keys from MS-OFFCRYPTO 2.3.4.11 / 2.3.4.13 / 2.3.4.14.
    private static readonly byte[] VerifierHashInputBlock = [0xfe, 0xa7, 0xd2, 0x76, 0x3b, 0x4b, 0x9e, 0x79];
    private static readonly byte[] VerifierHashValueBlock = [0xd7, 0xaa, 0x0f, 0x6d, 0x30, 0x61, 0x34, 0x4e];
    private static readonly byte[] KeyValueBlock = [0x14, 0x6e, 0x0b, 0xe7, 0xab, 0xac, 0xd0, 0xd6];
    private static readonly byte[] IntegrityKeyBlock = [0x5f, 0xb2, 0xad, 0x01, 0x0c, 0xb9, 0xe1, 0xf6];
    private static readonly byte[] IntegrityValueBlock = [0xa0, 0x67, 0x7f, 0x02, 0xb2, 0x2c, 0x84, 0x33];

    /// <summary>Returns <paramref name="package"/> (an unencrypted .xlsx) encrypted with <paramref name="password"/>.</summary>
    public static byte[] Encrypt(byte[] package, string password)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentException.ThrowIfNullOrEmpty(password);

        // The key that actually encrypts the file, and the salts. All random per file.
        var intermediateKey = RandomNumberGenerator.GetBytes(KeyBytes);
        var keyDataSalt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordSalt = RandomNumberGenerator.GetBytes(SaltSize);

        var encryptedPackage = EncryptPackage(package, intermediateKey, keyDataSalt);

        // Data integrity: an HMAC over the whole EncryptedPackage stream, its key and value
        // encrypted with the intermediate key.
        var hmacKey = RandomNumberGenerator.GetBytes(HashSize);
        var hmacValue = HMACSHA512.HashData(hmacKey, encryptedPackage);
        var encryptedHmacKey = AesCbc(intermediateKey, BlockIv(keyDataSalt, IntegrityKeyBlock), hmacKey);
        var encryptedHmacValue = AesCbc(intermediateKey, BlockIv(keyDataSalt, IntegrityValueBlock), hmacValue);

        // Password key encryptor: a verifier (so a wrong password is detected) and the
        // intermediate key, each encrypted with a key derived from the password.
        var passwordHash = HashPassword(password, passwordSalt);
        var verifierHashInput = RandomNumberGenerator.GetBytes(SaltSize);
        var encryptedVerifierHashInput = AesCbc(DeriveKey(passwordHash, VerifierHashInputBlock), passwordSalt, verifierHashInput);
        var encryptedVerifierHashValue = AesCbc(DeriveKey(passwordHash, VerifierHashValueBlock), passwordSalt, SHA512.HashData(verifierHashInput));
        var encryptedKeyValue = AesCbc(DeriveKey(passwordHash, KeyValueBlock), passwordSalt, intermediateKey);

        var info = EncryptionInfo(keyDataSalt, encryptedHmacKey, encryptedHmacValue,
            passwordSalt, encryptedVerifierHashInput, encryptedVerifierHashValue, encryptedKeyValue);

        return WriteCompoundFile(info, encryptedPackage);
    }

    /// <summary>StreamSize (8 bytes) then each 4 KB segment encrypted with its own IV.</summary>
    private static byte[] EncryptPackage(byte[] package, byte[] key, byte[] keyDataSalt)
    {
        using var output = new MemoryStream(8 + package.Length + BlockSize);
        Span<byte> size = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(size, (ulong)package.Length);
        output.Write(size);

        Span<byte> index = stackalloc byte[4];
        for (int offset = 0, segment = 0; offset < package.Length; offset += SegmentSize, segment++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(index, (uint)segment);
            var iv = BlockIv(keyDataSalt, index.ToArray());
            var chunk = package.AsSpan(offset, Math.Min(SegmentSize, package.Length - offset));
            output.Write(AesCbc(key, iv, chunk));
        }

        return output.ToArray();
    }

    /// <summary>H0 = SHA512(salt + password); Hn = SHA512(iterator + Hn-1), 100,000 times.</summary>
    private static byte[] HashPassword(string password, byte[] salt)
    {
        var hash = SHA512.HashData([.. salt, .. Encoding.Unicode.GetBytes(password)]);
        var buffer = new byte[4 + HashSize];
        for (uint i = 0; i < SpinCount; i++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, i);
            hash.CopyTo(buffer, 4);
            hash = SHA512.HashData(buffer);
        }
        return hash;
    }

    /// <summary>SHA512(passwordHash + blockKey), cut to the key size.</summary>
    private static byte[] DeriveKey(byte[] passwordHash, byte[] blockKey) =>
        SHA512.HashData([.. passwordHash, .. blockKey])[..KeyBytes];

    /// <summary>SHA512(keyDataSalt + blockKey), cut to the AES block size, used as an IV.</summary>
    private static byte[] BlockIv(byte[] keyDataSalt, byte[] blockKey) =>
        SHA512.HashData([.. keyDataSalt, .. blockKey])[..BlockSize];

    /// <summary>AES-CBC without padding; the input is zero-padded to a whole number of blocks.</summary>
    private static byte[] AesCbc(byte[] key, byte[] iv, ReadOnlySpan<byte> data)
    {
        var padded = new byte[(data.Length + BlockSize - 1) / BlockSize * BlockSize];
        data.CopyTo(padded);

        using var aes = Aes.Create();
        aes.Key = key;
        return aes.EncryptCbc(padded, iv, PaddingMode.None);
    }

    private static byte[] EncryptionInfo(byte[] keyDataSalt, byte[] encryptedHmacKey, byte[] encryptedHmacValue,
        byte[] passwordSalt, byte[] verifierHashInput, byte[] verifierHashValue, byte[] keyValue)
    {
        const string cipher = "saltSize=\"16\" blockSize=\"16\" keyBits=\"256\" hashSize=\"64\" " +
                              "cipherAlgorithm=\"AES\" cipherChaining=\"ChainingModeCBC\" hashAlgorithm=\"SHA512\"";

        var xml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n" +
            "<encryption xmlns=\"http://schemas.microsoft.com/office/2006/encryption\" " +
            "xmlns:p=\"http://schemas.microsoft.com/office/2006/keyEncryptor/password\" " +
            "xmlns:c=\"http://schemas.microsoft.com/office/2006/keyEncryptor/certificate\">" +
            $"<keyData {cipher} saltValue=\"{B64(keyDataSalt)}\"/>" +
            $"<dataIntegrity encryptedHmacKey=\"{B64(encryptedHmacKey)}\" encryptedHmacValue=\"{B64(encryptedHmacValue)}\"/>" +
            "<keyEncryptors><keyEncryptor uri=\"http://schemas.microsoft.com/office/2006/keyEncryptor/password\">" +
            $"<p:encryptedKey spinCount=\"{SpinCount}\" {cipher} saltValue=\"{B64(passwordSalt)}\" " +
            $"encryptedVerifierHashInput=\"{B64(verifierHashInput)}\" encryptedVerifierHashValue=\"{B64(verifierHashValue)}\" " +
            $"encryptedKeyValue=\"{B64(keyValue)}\"/>" +
            "</keyEncryptor></keyEncryptors></encryption>";

        // Version 4.4 (agile), flags 0x40, then the XML descriptor.
        using var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);
        w.Write((ushort)4);
        w.Write((ushort)4);
        w.Write(0x40u);
        w.Write(Encoding.UTF8.GetBytes(xml));
        w.Flush();
        return stream.ToArray();
    }

    private static string B64(byte[] value) => Convert.ToBase64String(value);

    // ── Compound file with the data-spaces streams [MS-OFFCRYPTO 2.2] ──

    private const string DataSpaceName = "StrongEncryptionDataSpace";
    private const string TransformName = "StrongEncryptionTransform";

    private static byte[] WriteCompoundFile(byte[] encryptionInfo, byte[] encryptedPackage)
    {
        using var output = new MemoryStream();
        using (var root = RootStorage.Create(output, OpenMcdf.Version.V3, StorageModeFlags.LeaveOpen))
        {
            var dataSpaces = root.CreateStorage("\u0006DataSpaces");
            WriteStream(dataSpaces, "Version", VersionStream());
            WriteStream(dataSpaces, "DataSpaceMap", DataSpaceMapStream());
            WriteStream(dataSpaces.CreateStorage("DataSpaceInfo"), DataSpaceName, DataSpaceDefinitionStream());
            WriteStream(dataSpaces.CreateStorage("TransformInfo").CreateStorage(TransformName), "\u0006Primary", PrimaryStream());

            WriteStream(root, "EncryptionInfo", encryptionInfo);
            WriteStream(root, "EncryptedPackage", encryptedPackage);
            root.Flush();
        }

        return output.ToArray();
    }

    private static void WriteStream(Storage storage, string name, byte[] data)
    {
        using var stream = storage.CreateStream(name);
        stream.Write(data, 0, data.Length);
    }

    /// <summary>A length-prefixed UTF-16 string padded to a multiple of 4 bytes (UNICODE-LP-P4).</summary>
    private static void WriteLpP4(BinaryWriter w, string value)
    {
        var bytes = Encoding.Unicode.GetBytes(value);
        w.Write((uint)bytes.Length);
        w.Write(bytes);
        for (var pad = bytes.Length % 4; pad is > 0 and < 4; pad++)
            w.Write((byte)0);
    }

    private static void WriteVersion(BinaryWriter w)
    {
        // Reader, updater and writer version 1.0.
        for (var i = 0; i < 3; i++)
        {
            w.Write((ushort)1);
            w.Write((ushort)0);
        }
    }

    private static byte[] Build(Action<BinaryWriter> write)
    {
        using var stream = new MemoryStream();
        using var w = new BinaryWriter(stream);
        write(w);
        w.Flush();
        return stream.ToArray();
    }

    private static byte[] VersionStream() => Build(w =>
    {
        WriteLpP4(w, "Microsoft.Container.DataSpaces");
        WriteVersion(w);
    });

    private static byte[] DataSpaceMapStream()
    {
        // One map entry: the EncryptedPackage stream belongs to the strong-encryption data space.
        var entry = Build(w =>
        {
            w.Write(1u);   // reference component count
            w.Write(0u);   // component type: stream
            WriteLpP4(w, "EncryptedPackage");
            WriteLpP4(w, DataSpaceName);
        });

        return Build(w =>
        {
            w.Write(8u);   // header length
            w.Write(1u);   // entry count
            w.Write((uint)(entry.Length + 4));
            w.Write(entry);
        });
    }

    private static byte[] DataSpaceDefinitionStream() => Build(w =>
    {
        w.Write(8u);   // header length
        w.Write(1u);   // transform reference count
        WriteLpP4(w, TransformName);
    });

    private static byte[] PrimaryStream() => Build(w =>
    {
        const string transformId = "{FF9A3F03-56EF-4613-BDD5-5A41C1D07246}";
        var idBytes = Encoding.Unicode.GetByteCount(transformId);
        var idPadded = (idBytes + 3) / 4 * 4;

        // TransformInfoHeader: length up to the name, type 1, id, name, versions.
        w.Write((uint)(4 + 4 + 4 + idPadded));
        w.Write(1u);
        WriteLpP4(w, transformId);
        WriteLpP4(w, "Microsoft.Container.EncryptionTransform");
        WriteVersion(w);

        // EncryptionTransformInfo: empty name, block size 0, cipher mode 0, reserved 4.
        w.Write(0u);
        w.Write(0u);
        w.Write(0u);
        w.Write(4u);
    });
}
