using System;

namespace Gexter;

/// <summary>
/// Exception thrown when an error occurs while reading or processing GXT files.
/// </summary>
public class GxtException : Exception
{
    /// <summary>
    /// Creates a new GXT exception with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public GxtException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new GXT exception with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public GxtException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

