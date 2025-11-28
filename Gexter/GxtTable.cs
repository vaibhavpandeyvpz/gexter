using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Gexter;

/// <summary>
/// Represents a GXT table containing text entries.
/// Entries are keyed by either a CRC32 hash (SA/IV) or a string key (VC/III).
/// </summary>
public class GxtTable : IEnumerable<KeyValuePair<uint, string>>
{
    private readonly Dictionary<uint, string> _entries = new();
    private readonly Dictionary<uint, string> _keyNames;

    /// <summary>
    /// Gets the table name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the internal encoding used for this table.
    /// </summary>
    public Encoding InternalEncoding { get; }

    /// <summary>
    /// Gets whether original key names are preserved.
    /// </summary>
    public bool KeepKeyNames { get; }

    /// <summary>
    /// Gets the number of entries in this table.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets the key hashes in this table.
    /// </summary>
    public IEnumerable<uint> Keys => _entries.Keys;

    /// <summary>
    /// Gets the values in this table.
    /// </summary>
    public IEnumerable<string> Values => _entries.Values;

    /// <summary>
    /// Creates a new GXT table.
    /// </summary>
    /// <param name="name">The table name.</param>
    /// <param name="internalEncoding">The encoding used for values.</param>
    /// <param name="keepKeyNames">Whether to preserve original key names.</param>
    public GxtTable(string name, Encoding internalEncoding, bool keepKeyNames = false)
    {
        Name = name;
        InternalEncoding = internalEncoding;
        KeepKeyNames = keepKeyNames;
        _keyNames = keepKeyNames ? new Dictionary<uint, string>() : new Dictionary<uint, string>();
    }

    /// <summary>
    /// Gets or sets an entry value by its key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <returns>The entry value, or null if not found.</returns>
    public string this[uint keyHash]
    {
        get
        {
            if (TryGetValue(keyHash, out var value))
                return value;
            return null;
        }
        set
        {
            if (value != null)
                SetValue(keyHash, value);
        }
    }

    /// <summary>
    /// Gets or sets an entry value by its key name.
    /// </summary>
    /// <param name="key">The key name.</param>
    /// <returns>The entry value, or null if not found.</returns>
    public string this[string key]
    {
        get => this[Crc32.Compute(key)];
        set => SetValue(key, value ?? string.Empty);
    }

    /// <summary>
    /// Gets a value by its key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <returns>The entry value, or null if not found.</returns>
    public string GetValue(uint keyHash)
    {
        if (_entries.TryGetValue(keyHash, out var value))
            return value;
        return null;
    }

    /// <summary>
    /// Gets a value by its key name.
    /// </summary>
    /// <param name="key">The key name.</param>
    /// <returns>The entry value, or null if not found.</returns>
    public string GetValue(string key)
    {
        return GetValue(Crc32.Compute(key));
    }

    /// <summary>
    /// Tries to get a value by its key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <param name="value">The entry value if found.</param>
    /// <returns>True if the entry was found, false otherwise.</returns>
    public bool TryGetValue(uint keyHash, out string value)
    {
        return _entries.TryGetValue(keyHash, out value);
    }

    /// <summary>
    /// Tries to get a value by its key name.
    /// </summary>
    /// <param name="key">The key name.</param>
    /// <param name="value">The entry value if found.</param>
    /// <returns>True if the entry was found, false otherwise.</returns>
    public bool TryGetValue(string key, out string value)
    {
        return TryGetValue(Crc32.Compute(key), out value);
    }

    /// <summary>
    /// Sets an entry value by its key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <param name="value">The entry value.</param>
    public void SetValue(uint keyHash, string value)
    {
        _entries[keyHash] = value;
    }

    /// <summary>
    /// Sets an entry value by its key name.
    /// </summary>
    /// <param name="key">The key name.</param>
    /// <param name="value">The entry value.</param>
    public void SetValue(string key, string value)
    {
        var keyHash = Crc32.Compute(key);
        _entries[keyHash] = value;

        if (KeepKeyNames)
            _keyNames[keyHash] = key;
    }

    /// <summary>
    /// Gets the original key name for a key hash, if available.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <returns>The original key name, or null if not available.</returns>
    public string GetKeyName(uint keyHash)
    {
        if (KeepKeyNames && _keyNames.TryGetValue(keyHash, out var name))
            return name;
        return null;
    }

    /// <summary>
    /// Sets the original key name for a key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <param name="name">The original key name.</param>
    public void SetKeyName(uint keyHash, string name)
    {
        if (KeepKeyNames)
            _keyNames[keyHash] = name;
    }

    /// <summary>
    /// Checks if an entry exists with the specified key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <returns>True if the entry exists, false otherwise.</returns>
    public bool ContainsKey(uint keyHash) => _entries.ContainsKey(keyHash);

    /// <summary>
    /// Checks if an entry exists with the specified key name.
    /// </summary>
    /// <param name="key">The key name.</param>
    /// <returns>True if the entry exists, false otherwise.</returns>
    public bool ContainsKey(string key) => ContainsKey(Crc32.Compute(key));

    /// <summary>
    /// Removes an entry by its key hash.
    /// </summary>
    /// <param name="keyHash">The CRC32 hash of the key.</param>
    /// <returns>True if the entry was removed, false if it didn't exist.</returns>
    public bool Remove(uint keyHash)
    {
        if (KeepKeyNames)
            _keyNames.Remove(keyHash);
        return _entries.Remove(keyHash);
    }

    /// <summary>
    /// Removes an entry by its key name.
    /// </summary>
    /// <param name="key">The key name.</param>
    /// <returns>True if the entry was removed, false if it didn't exist.</returns>
    public bool Remove(string key) => Remove(Crc32.Compute(key));

    /// <summary>
    /// Removes all entries from the table.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
        if (KeepKeyNames)
            _keyNames.Clear();
    }

    public IEnumerator<KeyValuePair<uint, string>> GetEnumerator() => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => $"GxtTable[{Name}] ({Count} entries)";
}

