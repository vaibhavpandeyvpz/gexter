namespace Gexter;

/// <summary>
/// Provides CRC32 hash computation for GXT key names.
/// Uses the standard CRC32 polynomial with upper-case conversion.
/// </summary>
public static class Crc32
{
    private static readonly uint[] CrcTable;

    static Crc32()
    {
        CrcTable = new uint[256];
        const uint polynomial = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ polynomial;
                else
                    crc >>= 1;
            }
            CrcTable[i] = crc;
        }
    }

    /// <summary>
    /// Computes the CRC32 hash of a key name.
    /// The key is converted to upper-case ASCII before hashing.
    /// </summary>
    /// <param name="key">The key name to hash.</param>
    /// <returns>The CRC32 hash value.</returns>
    public static uint Compute(string key)
    {
        if (string.IsNullOrEmpty(key))
            return 0;

        uint crc = 0xFFFFFFFF;

        foreach (char c in key)
        {
            byte b = (byte)char.ToUpperInvariant(c);
            crc = (crc >> 8) ^ CrcTable[(crc ^ b) & 0xFF];
        }

        return ~crc;
    }

    /// <summary>
    /// Computes the CRC32 hash of raw bytes.
    /// </summary>
    /// <param name="data">The byte array to hash.</param>
    /// <returns>The CRC32 hash value.</returns>
    public static uint Compute(byte[] data)
    {
        if (data == null || data.Length == 0)
            return 0;

        uint crc = 0xFFFFFFFF;

        foreach (byte b in data)
        {
            crc = (crc >> 8) ^ CrcTable[(crc ^ b) & 0xFF];
        }

        return ~crc;
    }

#if !NETFRAMEWORK && !NETSTANDARD2_0
    /// <summary>
    /// Computes the CRC32 hash of raw bytes using a span.
    /// </summary>
    /// <param name="data">The byte span to hash.</param>
    /// <returns>The CRC32 hash value.</returns>
    public static uint Compute(System.ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return 0;

        uint crc = 0xFFFFFFFF;

        foreach (byte b in data)
        {
            crc = (crc >> 8) ^ CrcTable[(crc ^ b) & 0xFF];
        }

        return ~crc;
    }
#endif
}

