using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gexter;

/// <summary>
/// Represents a loaded GXT file containing multiple tables.
/// </summary>
public class GxtFile : IEnumerable<GxtTable>
{
    private readonly Dictionary<string, GxtTable> _tablesByName;
    private readonly List<GxtTable> _tables;

    /// <summary>
    /// Gets the GXT version of this file.
    /// </summary>
    public GxtVersion Version { get; }

    /// <summary>
    /// Gets the number of tables in this file.
    /// </summary>
    public int TableCount => _tables.Count;

    /// <summary>
    /// Gets all tables in this file.
    /// </summary>
    public IReadOnlyList<GxtTable> Tables => _tables;

    /// <summary>
    /// Gets the table names in this file.
    /// </summary>
    public IEnumerable<string> TableNames => _tablesByName.Keys;

    /// <summary>
    /// Creates a new GXT file from loaded tables.
    /// </summary>
    /// <param name="version">The GXT version.</param>
    /// <param name="tables">The loaded tables.</param>
    public GxtFile(GxtVersion version, IEnumerable<GxtTable> tables)
    {
        Version = version;
        _tables = tables.ToList();
        _tablesByName = new Dictionary<string, GxtTable>(StringComparer.OrdinalIgnoreCase);

        foreach (var table in _tables)
        {
            _tablesByName[table.Name] = table;
        }
    }

    /// <summary>
    /// Gets a table by name.
    /// </summary>
    /// <param name="tableName">The table name (case-insensitive).</param>
    /// <returns>The table, or null if not found.</returns>
    public GxtTable this[string tableName]
    {
        get
        {
            if (_tablesByName.TryGetValue(tableName, out var table))
                return table;
            return null;
        }
    }

    /// <summary>
    /// Gets a table by index.
    /// </summary>
    /// <param name="index">The table index.</param>
    /// <returns>The table.</returns>
    public GxtTable this[int index] => _tables[index];

    /// <summary>
    /// Tries to get a table by name.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="table">The table if found.</param>
    /// <returns>True if the table was found, false otherwise.</returns>
    public bool TryGetTable(string tableName, out GxtTable table)
    {
        return _tablesByName.TryGetValue(tableName, out table);
    }

    /// <summary>
    /// Checks if a table with the specified name exists.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <returns>True if the table exists, false otherwise.</returns>
    public bool ContainsTable(string tableName)
    {
        return _tablesByName.ContainsKey(tableName);
    }

    /// <summary>
    /// Gets the total number of entries across all tables.
    /// </summary>
    /// <returns>The total entry count.</returns>
    public int GetTotalEntryCount()
    {
        return _tables.Sum(t => t.Count);
    }

    /// <summary>
    /// Searches for a value across all tables.
    /// </summary>
    /// <param name="keyHash">The key hash to search for.</param>
    /// <returns>The value if found, or null.</returns>
    public string FindValue(uint keyHash)
    {
        foreach (var table in _tables)
        {
            if (table.TryGetValue(keyHash, out var value))
                return value;
        }
        return null;
    }

    /// <summary>
    /// Searches for a value across all tables by key name.
    /// </summary>
    /// <param name="key">The key name to search for.</param>
    /// <returns>The value if found, or null.</returns>
    public string FindValue(string key)
    {
        return FindValue(Crc32.Compute(key));
    }

    public IEnumerator<GxtTable> GetEnumerator() => _tables.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Saves the GXT file to a file.
    /// </summary>
    /// <param name="filePath">The path to save the file to.</param>
    public void Save(string filePath)
    {
        using var writer = new GxtWriter(filePath);
        writer.Write(this);
    }

    /// <summary>
    /// Saves the GXT file to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="leaveOpen">Whether to leave the stream open after writing.</param>
    public void Save(Stream stream, bool leaveOpen = false)
    {
        using var writer = new GxtWriter(stream, leaveOpen);
        writer.Write(this);
    }

    public override string ToString() => $"GxtFile[{Version}] ({TableCount} tables, {GetTotalEntryCount()} entries)";
}

