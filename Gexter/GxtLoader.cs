using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gexter;

#if NETFRAMEWORK || NETSTANDARD2_0
internal struct EntryOffsetHash
{
    public int Offset;
    public uint KeyHash;

    public EntryOffsetHash(int offset, uint keyHash)
    {
        Offset = offset;
        KeyHash = keyHash;
    }
}

internal struct EntryOffsetKey
{
    public int Offset;
    public string Key;

    public EntryOffsetKey(int offset, string key)
    {
        Offset = offset;
        Key = key;
    }
}
#endif

/// <summary>
/// Loads and parses GXT (game text) files from GTA III, Vice City, San Andreas, and GTA IV.
/// 
/// <para>
/// <b>Text Encoding:</b>
/// <list type="bullet">
/// <item><description>GTA III/VC: Uses UTF-16LE encoding, supporting all Unicode characters including multi-byte languages (Chinese, Japanese, Korean, Arabic, etc.)</description></item>
/// <item><description>GTA SA/IV: Uses Windows-1252 encoding, limited to Western European languages (256 characters). Languages requiring multi-byte encodings are not supported by the game format.</description></item>
/// </list>
/// </para>
/// 
/// <para>
/// <b>Formatting Codes:</b>
/// GTA games use special formatting codes in text values that are preserved during reading:
/// <list type="bullet">
/// <item><description>~r~ = Red text</description></item>
/// <item><description>~g~ = Green text</description></item>
/// <item><description>~b~ = Blue text</description></item>
/// <item><description>~w~ = White text</description></item>
/// <item><description>~y~ = Yellow text</description></item>
/// <item><description>~h~ = Highlight</description></item>
/// <item><description>~k~ = Key binding</description></item>
/// <item><description>And many more...</description></item>
/// </list>
/// These codes are preserved as-is in the string values and should not be modified unless you understand their purpose.
/// </para>
/// </summary>
public class GxtLoader : IDisposable
{
    private static readonly Encoding Windows1252;
    private static bool _encodingRegistered;

    static GxtLoader()
    {
        // Register code pages encoding provider for Windows-1252 support (not needed for .NET Framework)
        if (!_encodingRegistered)
        {
#if !NETFRAMEWORK
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            _encodingRegistered = true;
        }

        // Get Windows-1252 encoding
        // On .NET Framework, Windows-1252 is built-in and should work without issues
        // On .NET Core/5+, we need the CodePages provider registered above
        try
        {
            Windows1252 = Encoding.GetEncoding(1252);
        }
        catch (ArgumentException)
        {
            // Fallback if encoding is not available (shouldn't happen with proper setup)
            Windows1252 = Encoding.GetEncoding("windows-1252");
        }
    }

    private readonly BinaryReader _reader;
    private readonly bool _ownsStream;
    private readonly List<GxtTableHeader> _tableHeaders = new();
    private long _currentPosition;
    private bool _disposed;

    /// <summary>
    /// Gets the detected GXT version.
    /// </summary>
    public GxtVersion Version { get; private set; }

    /// <summary>
    /// Gets the text encoding to use when reading strings.
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// Gets whether to preserve original key names (for VC/III format).
    /// </summary>
    public bool KeepKeyNames { get; set; }

    /// <summary>
    /// Gets the number of tables in the GXT file.
    /// </summary>
    public int TableCount => _tableHeaders.Count;

    /// <summary>
    /// Gets the table headers.
    /// </summary>
    public IReadOnlyList<GxtTableHeader> TableHeaders => _tableHeaders;

    /// <summary>
    /// Gets whether this is a GTA III single-table format.
    /// </summary>
    public bool IsSingleTableFormat { get; private set; }

    /// <summary>
    /// Creates a new GXT loader from a stream.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="encoding">Optional text encoding override.</param>
    /// <param name="leaveOpen">Whether to leave the stream open when disposing.</param>
    public GxtLoader(Stream stream, Encoding? encoding = null, bool leaveOpen = false)
    {
        _reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen);
        _ownsStream = !leaveOpen;
        Encoding = encoding ?? Encoding.UTF8;
        Initialize();
    }

    /// <summary>
    /// Creates a new GXT loader from a file path.
    /// </summary>
    /// <param name="filePath">The path to the GXT file.</param>
    /// <param name="encoding">Optional text encoding override.</param>
    public GxtLoader(string filePath, Encoding? encoding = null)
        : this(File.OpenRead(filePath), encoding, false)
    {
    }

    private void Initialize()
    {
        // Read first 4 bytes to determine format
        var header = _reader.ReadBytes(4);
        _currentPosition = 4;

        if (IsTKeyMarker(header))
        {
            // GTA III format: starts directly with "TKEY" (single table)
            InitializeGta3Format(header);
        }
        else if (IsTableMarker(header))
        {
            // GTA VC format: starts with "TABL" (multi-table)
            Version = GxtVersion.ViceCityIII;
            IsSingleTableFormat = false;
            InitializeMultiTableFormat();
        }
        else
        {
            // GTA SA/IV format: version header + "TABL"
            Version = GxtVersion.SanAndreasIV;
            IsSingleTableFormat = false;

            var tabl = _reader.ReadBytes(4);
            _currentPosition += 4;

            if (!IsTableMarker(tabl))
                throw new GxtException("Invalid GXT file: expected 'TABL' marker");

            InitializeMultiTableFormat();
        }
    }

    private void InitializeGta3Format(byte[] tkeyHeader)
    {
        // GTA III has a single table starting directly with TKEY
        Version = GxtVersion.ViceCityIII;
        IsSingleTableFormat = true;

        // Create a synthetic "MAIN" table header
        // The TKEY starts at position 0, so offset is 0
        _tableHeaders.Add(new GxtTableHeader("MAIN", 0));

        // Reset position back to start for reading
        _reader.BaseStream.Seek(0, SeekOrigin.Begin);
        _currentPosition = 0;
    }

    private void InitializeMultiTableFormat()
    {
        // Read table header size
        int headerSize = _reader.ReadInt32();
        _currentPosition += 4;

        if (headerSize < 0)
            throw new GxtException($"Invalid table header size: {headerSize}");

        if (headerSize % 12 != 0)
            throw new GxtException($"Invalid table header size (must be multiple of 12): {headerSize}");

        int tableCount = headerSize / 12;

        // Read all table headers
        for (int i = 0; i < tableCount; i++)
        {
            var nameBytes = _reader.ReadBytes(8);
            int offset = _reader.ReadInt32();
            _currentPosition += 12;

            if (offset < 0)
                throw new GxtException($"Invalid table offset: {offset}");

            // Convert name bytes to string (null-terminated)
            string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
            _tableHeaders.Add(new GxtTableHeader(name, offset));
        }
    }

    private static bool IsTableMarker(byte[] data)
    {
        return data.Length >= 4 &&
               data[0] == 'T' &&
               data[1] == 'A' &&
               data[2] == 'B' &&
               data[3] == 'L';
    }

    private static bool IsTKeyMarker(byte[] data)
    {
        return data.Length >= 4 &&
               data[0] == 'T' &&
               data[1] == 'K' &&
               data[2] == 'E' &&
               data[3] == 'Y';
    }

    private static bool IsTDatMarker(byte[] data)
    {
        return data.Length >= 4 &&
               data[0] == 'T' &&
               data[1] == 'D' &&
               data[2] == 'A' &&
               data[3] == 'T';
    }

    /// <summary>
    /// Reads a specific table by index.
    /// </summary>
    /// <param name="tableIndex">The index of the table to read.</param>
    /// <returns>The loaded GXT table.</returns>
    public GxtTable ReadTable(int tableIndex)
    {
        if (tableIndex < 0 || tableIndex >= _tableHeaders.Count)
            throw new ArgumentOutOfRangeException(nameof(tableIndex));

        return ReadTable(_tableHeaders[tableIndex]);
    }

    /// <summary>
    /// Reads a table by its header.
    /// </summary>
    /// <param name="header">The table header.</param>
    /// <returns>The loaded GXT table.</returns>
    public GxtTable ReadTable(GxtTableHeader header)
    {
        if (IsSingleTableFormat)
        {
            // GTA III single-table format
            return ReadGta3Table(header);
        }

        // Calculate the actual offset
        long offset = header.Offset;

        // Non-MAIN tables have their name repeated at the start
        if (!header.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase))
            offset += 8;

        SeekTo(offset);

        // Read TKEY marker
        var tkeyMarker = _reader.ReadBytes(4);
        _currentPosition += 4;

        if (!IsTKeyMarker(tkeyMarker))
            throw new GxtException($"Expected 'TKEY' marker at table {header.Name}");

        int tkeySize = _reader.ReadInt32();
        _currentPosition += 4;

        return Version == GxtVersion.SanAndreasIV
            ? ReadSaIvTable(header, tkeySize)
            : ReadVc3Table(header, tkeySize);
    }

    private GxtTable ReadGta3Table(GxtTableHeader header)
    {
        // GTA III: File starts directly with TKEY
        SeekTo(0);

        // Read TKEY marker
        var tkeyMarker = _reader.ReadBytes(4);
        _currentPosition += 4;

        if (!IsTKeyMarker(tkeyMarker))
            throw new GxtException("Expected 'TKEY' marker at start of GTA III file");

        int tkeySize = _reader.ReadInt32();
        _currentPosition += 4;

        // GTA III uses the same format as Vice City (UTF-16LE with string keys)
        return ReadVc3Table(header, tkeySize);
    }

    private GxtTable ReadSaIvTable(GxtTableHeader header, int tkeySize)
    {
        // SA/IV format: 8-byte entries (offset + keyHash)
        int numEntries = tkeySize / 8;
        var encoding = Encoding ?? GetDefaultEncoding();
        var table = new GxtTable(header.Name, encoding, KeepKeyNames);

        // Read all entries
#if NETFRAMEWORK || NETSTANDARD2_0
        var entries = new List<EntryOffsetHash>(numEntries);
        for (int i = 0; i < numEntries; i++)
        {
            int entryOffset = _reader.ReadInt32();
            uint keyHash = _reader.ReadUInt32();
            entries.Add(new EntryOffsetHash(entryOffset, keyHash));
        }
#else
        var entries = new List<(int offset, uint keyHash)>(numEntries);
        for (int i = 0; i < numEntries; i++)
        {
            int entryOffset = _reader.ReadInt32();
            uint keyHash = _reader.ReadUInt32();
            entries.Add((entryOffset, keyHash));
        }
#endif
        _currentPosition += 8 * numEntries;

        // Skip TDAT marker and size
        _reader.ReadBytes(4); // "TDAT"
        _reader.ReadInt32();  // TDAT size
        _currentPosition += 8;

        // Sort entries by offset to read strings in order
#if NETFRAMEWORK || NETSTANDARD2_0
        entries.Sort((a, b) => a.Offset.CompareTo(b.Offset));
#else
        entries.Sort((a, b) => a.offset.CompareTo(b.offset));
#endif

        long tdatStart = _currentPosition;

#if NETFRAMEWORK || NETSTANDARD2_0
        foreach (var entry in entries)
        {
            int entryOffset = entry.Offset;
            uint keyHash = entry.KeyHash;
#else
        foreach (var (entryOffset, keyHash) in entries)
        {
#endif
            // Seek to string position
            long stringPos = tdatStart + entryOffset;
            if (_currentPosition != stringPos)
                SeekTo(stringPos);

            // Read null-terminated string (8-bit encoding)
            string text = ReadNullTerminatedString8();
            table.SetValue(keyHash, text);
        }

        return table;
    }

    private GxtTable ReadVc3Table(GxtTableHeader header, int tkeySize)
    {
        // VC3 format: 12-byte entries (offset + 8-byte key name)
        int numEntries = tkeySize / 12;
        var encoding = Encoding ?? GetDefaultEncoding();
        var table = new GxtTable(header.Name, encoding, KeepKeyNames);

        // Read all entries
#if NETFRAMEWORK || NETSTANDARD2_0
        var entries = new List<EntryOffsetKey>(numEntries);
        for (int i = 0; i < numEntries; i++)
        {
            int entryOffset = _reader.ReadInt32();
            var keyBytes = _reader.ReadBytes(8);
            string key = Encoding.ASCII.GetString(keyBytes).TrimEnd('\0');
            entries.Add(new EntryOffsetKey(entryOffset, key));
        }
#else
        var entries = new List<(int offset, string key)>(numEntries);
        for (int i = 0; i < numEntries; i++)
        {
            int entryOffset = _reader.ReadInt32();
            var keyBytes = _reader.ReadBytes(8);
            string key = Encoding.ASCII.GetString(keyBytes).TrimEnd('\0');
            entries.Add((entryOffset, key));
        }
#endif
        _currentPosition += 12 * numEntries;

        // Sort entries by offset
#if NETFRAMEWORK || NETSTANDARD2_0
        entries.Sort((a, b) => a.Offset.CompareTo(b.Offset));
#else
        entries.Sort((a, b) => a.offset.CompareTo(b.offset));
#endif

        // Read TDAT marker and size
        var tdatMarker = _reader.ReadBytes(4);
        _currentPosition += 4;

        if (!IsTDatMarker(tdatMarker))
            throw new GxtException($"Expected 'TDAT' marker at table {header.Name}");

        int tdatSize = _reader.ReadInt32();
        _currentPosition += 4;

        long tdatStart = _currentPosition;

        // Read strings using calculated lengths
        for (int i = 0; i < entries.Count; i++)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            var entry = entries[i];
            int entryOffset = entry.Offset;
            string key = entry.Key;

            // Calculate string length (to next entry or end of TDAT)
            int maxLen;
            if (i < entries.Count - 1)
                maxLen = entries[i + 1].Offset - entryOffset;
            else
                maxLen = tdatSize - entryOffset;
#else
            var (entryOffset, key) = entries[i];

            // Calculate string length (to next entry or end of TDAT)
            int maxLen;
            if (i < entries.Count - 1)
                maxLen = entries[i + 1].offset - entryOffset;
            else
                maxLen = tdatSize - entryOffset;
#endif

            // Seek to string position
            long stringPos = tdatStart + entryOffset;
            if (_currentPosition != stringPos)
                SeekTo(stringPos);

            // Read UTF-16LE string
            var stringBytes = _reader.ReadBytes(maxLen);
            _currentPosition += maxLen;

            string text = ReadUtf16String(stringBytes);
            table.SetValue(key, text);
        }

        return table;
    }

    private string ReadNullTerminatedString8()
    {
        var bytes = new List<byte>();
        byte b;
        while ((b = _reader.ReadByte()) != 0)
        {
            bytes.Add(b);
        }
        _currentPosition += bytes.Count + 1;

        if (bytes.Count == 0)
            return string.Empty;

        // Use Windows-1252 encoding for GTA SA/IV (common for Western languages)
        // Windows-1252 supports all byte values 0-255, so we should never get replacement characters
        var byteArray = bytes.ToArray();
        var decoder = Windows1252.GetDecoder();
        var charCount = decoder.GetCharCount(byteArray, 0, byteArray.Length);
        var chars = new char[charCount];
        decoder.GetChars(byteArray, 0, byteArray.Length, chars, 0);
        return new string(chars);
    }

    private static string ReadUtf16String(byte[] data)
    {
        // Find null terminator (two zero bytes)
        int length = 0;
        for (int i = 0; i < data.Length - 1; i += 2)
        {
            if (data[i] == 0 && data[i + 1] == 0)
                break;
            length += 2;
        }

        if (length == 0)
            return string.Empty;

        // Use decoder to ensure proper handling across all platforms
        var decoder = Encoding.Unicode.GetDecoder();
        var charCount = decoder.GetCharCount(data, 0, length);
        var chars = new char[charCount];
        decoder.GetChars(data, 0, length, chars, 0);
        return new string(chars);
    }

    private Encoding GetDefaultEncoding()
    {
        return Version == GxtVersion.SanAndreasIV
            ? Windows1252        // Windows-1252 for SA/IV
            : Encoding.Unicode;  // UTF-16LE for VC/III
    }

    private void SeekTo(long position)
    {
        _reader.BaseStream.Seek(position, SeekOrigin.Begin);
        _currentPosition = position;
    }

    /// <summary>
    /// Reads all tables from the GXT file.
    /// </summary>
    /// <returns>A list of all GXT tables.</returns>
    public List<GxtTable> ReadAllTables()
    {
        var tables = new List<GxtTable>(_tableHeaders.Count);
        foreach (var header in _tableHeaders)
        {
            tables.Add(ReadTable(header));
        }
        return tables;
    }

    /// <summary>
    /// Loads a GXT file and returns all tables.
    /// </summary>
    /// <param name="filePath">The path to the GXT file.</param>
    /// <param name="encoding">Optional text encoding override.</param>
    /// <param name="keepKeyNames">Whether to preserve original key names.</param>
    /// <returns>A GxtFile containing all tables.</returns>
    public static GxtFile Load(string filePath, Encoding? encoding = null, bool keepKeyNames = true)
    {
        using var loader = new GxtLoader(filePath, encoding);
        loader.KeepKeyNames = keepKeyNames;
        return new GxtFile(loader.Version, loader.ReadAllTables());
    }

    /// <summary>
    /// Loads a GXT file from a stream and returns all tables.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="encoding">Optional text encoding override.</param>
    /// <param name="keepKeyNames">Whether to preserve original key names.</param>
    /// <returns>A GxtFile containing all tables.</returns>
    public static GxtFile Load(Stream stream, Encoding? encoding = null, bool keepKeyNames = true)
    {
        using var loader = new GxtLoader(stream, encoding, true);
        loader.KeepKeyNames = keepKeyNames;
        return new GxtFile(loader.Version, loader.ReadAllTables());
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsStream)
                _reader.Dispose();
            _disposed = true;
        }
    }
}
