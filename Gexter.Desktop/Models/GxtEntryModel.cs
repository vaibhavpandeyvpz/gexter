using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Gexter.Desktop.Models;

/// <summary>
/// Represents a single GXT entry for display and editing.
/// </summary>
public partial class GxtEntryModel : ObservableObject
{
    private bool _isLoading = true;
    private string _originalValue = string.Empty;

    [ObservableProperty]
    private uint _keyHash;

    [ObservableProperty]
    private string _keyName = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isNew;

    /// <summary>
    /// Event raised when the entry is modified.
    /// </summary>
    public event EventHandler? Modified;

    /// <summary>
    /// Gets the display key (name if available, otherwise hash as hex).
    /// </summary>
    public string DisplayKey => !string.IsNullOrEmpty(KeyName) ? KeyName : $"0x{KeyHash:X8}";

    /// <summary>
    /// Gets the key hash as a hex string.
    /// </summary>
    public string KeyHashHex => $"0x{KeyHash:X8}";

    /// <summary>
    /// Marks the entry as loaded (no longer in initial loading state).
    /// </summary>
    public void MarkAsLoaded()
    {
        _originalValue = Value;
        _isLoading = false;
        IsModified = false;
    }

    /// <summary>
    /// Resets the modified state after saving.
    /// </summary>
    public void ResetModified()
    {
        _originalValue = Value;
        IsModified = false;
        IsNew = false;
    }

    partial void OnKeyNameChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayKey));
    }

    partial void OnKeyHashChanged(uint value)
    {
        OnPropertyChanged(nameof(DisplayKey));
        OnPropertyChanged(nameof(KeyHashHex));
    }

    partial void OnValueChanged(string value)
    {
        if (!_isLoading)
        {
            IsModified = value != _originalValue;
            Modified?.Invoke(this, EventArgs.Empty);
        }
    }
}

