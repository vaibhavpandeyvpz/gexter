using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gexter;

#if NETFRAMEWORK || NETSTANDARD2_0
internal struct EntryKeyValueOffset
{
    public string Key;
    public string Value;
    public int Offset;

    public EntryKeyValueOffset(string key, string value, int offset)
    {
        Key = key;
        Value = value;
        Offset = offset;
    }
}

internal struct EntryHashValueOffset
{
    public uint KeyHash;
    public string Value;
    public int Offset;

    public EntryHashValueOffset(uint keyHash, string value, int offset)
    {
        KeyHash = keyHash;
        Value = value;
        Offset = offset;
    }
}

internal struct TableOffset
{
    public GxtTable Table;
    public int Offset;

    public TableOffset(GxtTable table, int offset)
    {
        Table = table;
        Offset = offset;
    }
}
#endif

/// <summary>
/// Writes GXT files to streams or files.
/// </summary>
public class GxtWriter : IDisposable
{
    private static readonly Encoding Windows1252;

    static GxtWriter()
    {
        // Register code pages encoding provider for Windows-1252 support (not needed for .NET Framework)
#if !NETFRAMEWORK
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch
        {
            // Already registered
        }
#endif

        try
        {
            Windows1252 = Encoding.GetEncoding(1252);
        }
        catch (ArgumentException)
        {
            Windows1252 = Encoding.GetEncoding("windows-1252");
        }
    }

    private readonly BinaryWriter _writer;
    private readonly bool _ownsStream;
    private bool _disposed;

    /// <summary>
    /// Creates a new GXT writer for a stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="leaveOpen">Whether to leave the stream open when disposing.</param>
    public GxtWriter(Stream stream, bool leaveOpen = false)
    {
        _writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen);
        _ownsStream = !leaveOpen;
    }

    /// <summary>
    /// Creates a new GXT writer for a file.
    /// </summary>
    /// <param name="filePath">The path to the output file.</param>
    public GxtWriter(string filePath)
        : this(File.Create(filePath), false)
    {
    }

    /// <summary>
    /// Writes a GXT file to the stream.
    /// </summary>
    /// <param name="gxtFile">The GXT file to write.</param>
    public void Write(GxtFile gxtFile)
    {
        if (gxtFile.Version == GxtVersion.ViceCityIII && gxtFile.TableCount == 1)
        {
            // GTA III single-table format
            WriteGta3Format(gxtFile);
        }
        else if (gxtFile.Version == GxtVersion.ViceCityIII)
        {
            // GTA VC multi-table format
            WriteVcFormat(gxtFile);
        }
        else
        {
            // GTA SA/IV format
            WriteSaIvFormat(gxtFile);
        }
    }

    private void WriteGta3Format(GxtFile gxtFile)
    {
        var table = gxtFile.Tables[0];

        // Write TKEY marker
        _writer.Write(Encoding.ASCII.GetBytes("TKEY"));

        // Calculate TKEY size (12 bytes per entry)
        int tkeySize = table.Count * 12;
        _writer.Write(tkeySize);

        // First pass: collect all entries with their key names
#if NETFRAMEWORK || NETSTANDARD2_0
        var entries = new List<EntryKeyValueOffset>();
#else
        var entries = new List<(string key, string value, int offset)>();
#endif

        // Collect entries and normalize key names
        foreach (var entry in table)
        {
            string key = table.GetKeyName(entry.Key) ?? $"KEY{entry.Key:X8}";
            if (key.Length > 8)
                key = key.Substring(0, 8);
            else if (key.Length < 8)
                key = key.PadRight(8, '\0');

#if NETFRAMEWORK || NETSTANDARD2_0
            entries.Add(new EntryKeyValueOffset(key, entry.Value, 0)); // Offset will be calculated after sorting
#else
            entries.Add((key, entry.Value, 0)); // Offset will be calculated after sorting
#endif
        }

        // Sort entries by key name (ASCII order) - this is what the reference code does
        entries.Sort((a, b) =>
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            return string.Compare(a.Key, b.Key, StringComparison.Ordinal);
#else
            return string.Compare(a.key, b.key, StringComparison.Ordinal);
#endif
        });

        // Calculate offsets in sorted order
        int currentOffset = 0;
        for (int i = 0; i < entries.Count; i++)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            int stringLength = (entries[i].Value.Length + 1) * 2; // UTF-16LE: 2 bytes per char + null terminator
            entries[i] = new EntryKeyValueOffset(entries[i].Key, entries[i].Value, currentOffset);
#else
            int stringLength = (entries[i].value.Length + 1) * 2; // UTF-16LE: 2 bytes per char + null terminator
            entries[i] = (entries[i].key, entries[i].value, currentOffset);
#endif
            currentOffset += stringLength;
        }

        // Write TKEY entries (offset + 8-byte key) - NOTE: offset comes FIRST, then key
        foreach (var entry in entries)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            _writer.Write(entry.Offset);
            var keyBytes = new byte[8];
            var keyNameBytes = Encoding.ASCII.GetBytes(entry.Key);
            Array.Copy(keyNameBytes, 0, keyBytes, 0, Math.Min(keyNameBytes.Length, 8));
            _writer.Write(keyBytes);
#else
            _writer.Write(entry.offset);
            var keyBytes = new byte[8];
            var keyNameBytes = Encoding.ASCII.GetBytes(entry.key);
            Array.Copy(keyNameBytes, 0, keyBytes, 0, Math.Min(keyNameBytes.Length, 8));
            _writer.Write(keyBytes);
#endif
        }

        // Write TDAT marker
        _writer.Write(Encoding.ASCII.GetBytes("TDAT"));
        _writer.Write(currentOffset);

        // Write strings in sorted order (same order as TKEY entries)
        foreach (var entry in entries)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            var bytes = Encoding.Unicode.GetBytes(entry.Value);
#else
            var bytes = Encoding.Unicode.GetBytes(entry.value);
#endif
            _writer.Write(bytes);
            _writer.Write((ushort)0); // Null terminator
        }
    }

    private void WriteVcFormat(GxtFile gxtFile)
    {
        // Sort tables alphabetically, with MAIN first (as per reference code)
        var sortedTables = gxtFile.Tables.OrderBy(t => t.Name, StringComparer.Ordinal).ToList();
        var mainTable = sortedTables.FirstOrDefault(t => t.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase));
        if (mainTable != null)
        {
            sortedTables.Remove(mainTable);
            sortedTables.Insert(0, mainTable);
        }

        // Write TABL marker
        _writer.Write(Encoding.ASCII.GetBytes("TABL"));

        // Calculate header size (12 bytes per table)
        int headerSize = sortedTables.Count * 12;
        _writer.Write(headerSize);

        // Calculate table offsets (first table starts after TABL header)
        int currentOffset = 4 + 4 + headerSize; // TABL marker + size + headers
#if NETFRAMEWORK || NETSTANDARD2_0
        var tableOffsets = new List<TableOffset>();
#else
        var tableOffsets = new List<(GxtTable table, int offset)>();
#endif

        // First pass: calculate table sizes and offsets
        foreach (var table in sortedTables)
        {
            // Calculate table size
            int tableSize = 4 + 4; // TKEY marker + size
            if (!table.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase))
                tableSize += 8; // Table name repeated

            tableSize += table.Count * 12; // Entry headers

            tableSize += 4 + 4; // TDAT marker + size

            // Calculate TDAT size (UTF-16LE strings)
            int tdatSize = 0;
            foreach (var entry in table)
            {
                tdatSize += (entry.Value.Length + 1) * 2; // String + null terminator
            }
            tableSize += tdatSize;

#if NETFRAMEWORK || NETSTANDARD2_0
            tableOffsets.Add(new TableOffset(table, currentOffset));
#else
            tableOffsets.Add((table, currentOffset));
#endif
            currentOffset += tableSize;
        }

        // Second pass: write table headers
        foreach (var tableOffset in tableOffsets)
        {
            var nameBytes = new byte[8];
#if NETFRAMEWORK || NETSTANDARD2_0
            var name = Encoding.ASCII.GetBytes(tableOffset.Table.Name);
#else
            var name = Encoding.ASCII.GetBytes(tableOffset.table.Name);
#endif
            Array.Copy(name, 0, nameBytes, 0, Math.Min(name.Length, 8));
            _writer.Write(nameBytes);
#if NETFRAMEWORK || NETSTANDARD2_0
            _writer.Write(tableOffset.Offset);
#else
            _writer.Write(tableOffset.offset);
#endif
        }

        // Third pass: write table data
        bool isFirstTable = true;
        foreach (var tableOffset in tableOffsets)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            WriteVcTable(tableOffset.Table, isFirstTable);
#else
            WriteVcTable(tableOffset.table, isFirstTable);
#endif
            isFirstTable = false;
        }
    }

    private void WriteVcTable(GxtTable table, bool isFirstTable)
    {
        // Write table name if not MAIN and not first table
        if (!table.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase) && !isFirstTable)
        {
            var nameBytes = new byte[8];
            var name = Encoding.ASCII.GetBytes(table.Name);
            Array.Copy(name, 0, nameBytes, 0, Math.Min(name.Length, 8));
            _writer.Write(nameBytes);
        }

        // Write TKEY marker
        _writer.Write(Encoding.ASCII.GetBytes("TKEY"));

        // Calculate TKEY size (12 bytes per entry)
        int tkeySize = table.Count * 12;
        _writer.Write(tkeySize);

        // First pass: collect all entries with their key names
#if NETFRAMEWORK || NETSTANDARD2_0
        var entries = new List<EntryKeyValueOffset>();
#else
        var entries = new List<(string key, string value, int offset)>();
#endif

        // Collect entries and normalize key names
        foreach (var entry in table)
        {
            string key = table.GetKeyName(entry.Key) ?? $"KEY{entry.Key:X8}";
            if (key.Length > 8)
                key = key.Substring(0, 8);
            else if (key.Length < 8)
                key = key.PadRight(8, '\0');

#if NETFRAMEWORK || NETSTANDARD2_0
            entries.Add(new EntryKeyValueOffset(key, entry.Value, 0)); // Offset will be calculated after sorting
#else
            entries.Add((key, entry.Value, 0)); // Offset will be calculated after sorting
#endif
        }

        // Sort entries by key name (ASCII order) - this is what the reference code does
        entries.Sort((a, b) =>
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            return string.Compare(a.Key, b.Key, StringComparison.Ordinal);
#else
            return string.Compare(a.key, b.key, StringComparison.Ordinal);
#endif
        });

        // Calculate offsets in sorted order
        int currentOffset = 0;
        for (int i = 0; i < entries.Count; i++)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            int stringLength = (entries[i].Value.Length + 1) * 2; // UTF-16LE: 2 bytes per char + null terminator
            entries[i] = new EntryKeyValueOffset(entries[i].Key, entries[i].Value, currentOffset);
#else
            int stringLength = (entries[i].value.Length + 1) * 2; // UTF-16LE: 2 bytes per char + null terminator
            entries[i] = (entries[i].key, entries[i].value, currentOffset);
#endif
            currentOffset += stringLength;
        }

        // Write TKEY entries (offset + 8-byte key) - NOTE: offset comes FIRST, then key
        foreach (var entry in entries)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            _writer.Write(entry.Offset);
            var keyBytes = new byte[8];
            var keyNameBytes = Encoding.ASCII.GetBytes(entry.Key);
            Array.Copy(keyNameBytes, 0, keyBytes, 0, Math.Min(keyNameBytes.Length, 8));
            _writer.Write(keyBytes);
#else
            _writer.Write(entry.offset);
            var keyBytes = new byte[8];
            var keyNameBytes = Encoding.ASCII.GetBytes(entry.key);
            Array.Copy(keyNameBytes, 0, keyBytes, 0, Math.Min(keyNameBytes.Length, 8));
            _writer.Write(keyBytes);
#endif
        }

        // Write TDAT marker
        _writer.Write(Encoding.ASCII.GetBytes("TDAT"));
        _writer.Write(currentOffset);

        // Write strings in sorted order (same order as TKEY entries)
        foreach (var entry in entries)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            var bytes = Encoding.Unicode.GetBytes(entry.Value);
#else
            var bytes = Encoding.Unicode.GetBytes(entry.value);
#endif
            _writer.Write(bytes);
            _writer.Write((ushort)0); // Null terminator
        }
    }

    private void WriteSaIvFormat(GxtFile gxtFile)
    {
        // Write version (usually 0x00000001)
        _writer.Write(1);

        // Write TABL marker
        _writer.Write(Encoding.ASCII.GetBytes("TABL"));

        // Calculate header size (12 bytes per table)
        int headerSize = gxtFile.TableCount * 12;
        _writer.Write(headerSize);

        // Calculate table offsets
        int currentOffset = 4 + 4 + 4 + headerSize; // Version + TABL marker + size + headers
#if NETFRAMEWORK || NETSTANDARD2_0
        var tableOffsets = new List<TableOffset>();
#else
        var tableOffsets = new List<(GxtTable table, int offset)>();
#endif

        foreach (var table in gxtFile.Tables)
        {
            // Write table header (8-byte name + offset)
            var nameBytes = new byte[8];
            var name = Encoding.ASCII.GetBytes(table.Name);
            Array.Copy(name, 0, nameBytes, 0, Math.Min(name.Length, 8));
            _writer.Write(nameBytes);
            _writer.Write(currentOffset);

            // Calculate table size
            int tableSize = 4 + 4; // TKEY marker + size
            if (!table.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase))
                tableSize += 8; // Table name repeated

            tableSize += table.Count * 8; // Entry headers (offset + hash)

            tableSize += 4 + 4; // TDAT marker + size

            // Calculate TDAT size (Windows-1252 strings)
            int tdatSize = 0;
            foreach (var entry in table)
            {
                var bytes = Windows1252.GetBytes(entry.Value);
                tdatSize += bytes.Length + 1; // String + null terminator
            }
            tableSize += tdatSize;

#if NETFRAMEWORK || NETSTANDARD2_0
            tableOffsets.Add(new TableOffset(table, currentOffset));
#else
            tableOffsets.Add((table, currentOffset));
#endif
            currentOffset += tableSize;
        }

        // Write tables
        foreach (var tableOffset in tableOffsets)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            WriteSaIvTable(tableOffset.Table, tableOffset.Offset == 4 + 4 + 4 + headerSize);
#else
            WriteSaIvTable(tableOffset.table, tableOffset.offset == 4 + 4 + 4 + headerSize);
#endif
        }
    }

    private void WriteSaIvTable(GxtTable table, bool isFirstTable)
    {
        // Write table name if not MAIN and not first table
        if (!table.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase) && !isFirstTable)
        {
            var nameBytes = new byte[8];
            var name = Encoding.ASCII.GetBytes(table.Name);
            Array.Copy(name, 0, nameBytes, 0, Math.Min(name.Length, 8));
            _writer.Write(nameBytes);
        }

        // Write TKEY marker
        _writer.Write(Encoding.ASCII.GetBytes("TKEY"));

        // Calculate TKEY size (8 bytes per entry)
        int tkeySize = table.Count * 8;
        _writer.Write(tkeySize);

        // Write entries and collect string data
#if NETFRAMEWORK || NETSTANDARD2_0
        var entries = new List<EntryHashValueOffset>();
#else
        var entries = new List<(uint keyHash, string value, int offset)>();
#endif
        int currentOffset = 0;

        foreach (var entry in table.OrderBy(e => e.Key))
        {
            _writer.Write(currentOffset);
            _writer.Write(entry.Key);

            // Calculate string length (Windows-1252: 1 byte per char + null terminator)
            var bytes = Windows1252.GetBytes(entry.Value);
            int stringLength = bytes.Length + 1;
#if NETFRAMEWORK || NETSTANDARD2_0
            entries.Add(new EntryHashValueOffset(entry.Key, entry.Value, currentOffset));
#else
            entries.Add((entry.Key, entry.Value, currentOffset));
#endif
            currentOffset += stringLength;
        }

        // Write TDAT marker
        _writer.Write(Encoding.ASCII.GetBytes("TDAT"));
        _writer.Write(currentOffset);

        // Write strings
        foreach (var entry in entries)
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            var bytes = Windows1252.GetBytes(entry.Value);
#else
            var bytes = Windows1252.GetBytes(entry.value);
#endif
            _writer.Write(bytes);
            _writer.Write((byte)0); // Null terminator
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsStream)
                _writer.Dispose();
            _disposed = true;
        }
    }
}

