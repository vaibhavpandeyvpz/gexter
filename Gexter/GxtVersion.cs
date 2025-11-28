namespace Gexter;

/// <summary>
/// Represents the GXT file format version.
/// </summary>
public enum GxtVersion
{
    /// <summary>
    /// GTA San Andreas and GTA IV format.
    /// Uses 8-bit encoding with CRC32 key hashes.
    /// </summary>
    SanAndreasIV,

    /// <summary>
    /// GTA Vice City and GTA III format.
    /// Uses 16-bit (UTF-16LE) encoding with 8-character string keys.
    /// </summary>
    ViceCityIII
}

