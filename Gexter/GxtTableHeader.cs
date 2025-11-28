namespace Gexter;

/// <summary>
/// Represents a GXT table header containing the table name and data offset.
/// </summary>
public readonly struct GxtTableHeader
{
    /// <summary>
    /// The table name (up to 8 characters).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The offset to the table data within the file.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Creates a new table header.
    /// </summary>
    /// <param name="name">The table name.</param>
    /// <param name="offset">The data offset.</param>
    public GxtTableHeader(string name, int offset)
    {
        Name = name;
        Offset = offset;
    }

    public override string ToString() => $"{Name} @ 0x{Offset:X}";
}

