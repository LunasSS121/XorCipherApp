using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XorEncryptor.Services;

/// <summary>
/// Stateless service for XOR encryption/decryption.
/// XOR is symmetric: encrypt == decrypt with the same key.
/// </summary>
public static class XorService
{
    // ── Core ────────────────────────────────────────────────────────────────

    public static byte[] Process(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        if (key.IsEmpty)
            throw new ArgumentException("Key must not be empty.", nameof(key));

        var result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
            result[i] = (byte)(data[i] ^ key[i % key.Length]);

        return result;
    }

    // ── Text ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Encrypts UTF-8 <paramref name="plainText"/> and returns Base64
    /// so the result is always a printable string.
    /// </summary>
    public static string EncryptText(string plainText, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        ArgumentException.ThrowIfNullOrEmpty(key);

        byte[] encrypted = Process(
            Encoding.UTF8.GetBytes(plainText),
            Encoding.UTF8.GetBytes(key));

        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypts Base64 <paramref name="cipherBase64"/> back to UTF-8 text.
    /// Throws <see cref="FormatException"/> if the input is not valid Base64.
    /// </summary>
    public static string DecryptText(string cipherBase64, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherBase64);
        ArgumentException.ThrowIfNullOrEmpty(key);

        byte[] decrypted = Process(
            Convert.FromBase64String(cipherBase64),   // throws FormatException on bad input
            Encoding.UTF8.GetBytes(key));

        return Encoding.UTF8.GetString(decrypted);
    }

    // ── Stream (files) ────────────────────────────────────────────────────────

    /// <summary>
    /// Processes <paramref name="input"/> → <paramref name="output"/> in 80 KB chunks.
    /// Reports progress in [0.0 … 1.0] via <paramref name="onProgress"/>.
    /// </summary>
    public static async Task ProcessStreamAsync(
        Stream input,
        Stream output,
        byte[] keyBytes,
        IProgress<double>? onProgress = null,
        CancellationToken ct = default,
        int bufferSize = 81_920)
    {
        if (keyBytes is null || keyBytes.Length == 0)
            throw new ArgumentException("Key must not be empty.", nameof(keyBytes));

        long total    = input.CanSeek ? input.Length : -1;
        long written  = 0;
        int keyOffset = 0;

        var buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = await input.ReadAsync(buffer, ct)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                buffer[i] ^= keyBytes[keyOffset % keyBytes.Length];
                keyOffset++;
            }

            await output.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            written += bytesRead;

            if (total > 0)
                onProgress?.Report((double)written / total);
        }

        onProgress?.Report(1.0);
    }
}
