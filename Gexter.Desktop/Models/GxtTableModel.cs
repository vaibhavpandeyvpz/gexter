using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Gexter.Desktop.Models;

/// <summary>
/// Represents a GXT table for display in the UI.
/// </summary>
public partial class GxtTableModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isModified;

    public ObservableCollection<GxtEntryModel> Entries { get; }

    public GxtTableModel()
    {
        Entries = new ObservableCollection<GxtEntryModel>();
        Entries.CollectionChanged += OnEntriesCollectionChanged;
    }

    /// <summary>
    /// Gets the entry count for display.
    /// </summary>
    public int EntryCount => Entries.Count;

    public void RefreshEntryCount()
    {
        OnPropertyChanged(nameof(EntryCount));
    }

    /// <summary>
    /// Marks all entries as loaded and resets modified state.
    /// </summary>
    public void MarkAsLoaded()
    {
        foreach (var entry in Entries)
        {
            entry.MarkAsLoaded();
        }
        IsModified = false;
    }

    /// <summary>
    /// Resets the modified state after saving.
    /// </summary>
    public void ResetModified()
    {
        foreach (var entry in Entries)
        {
            entry.ResetModified();
        }
        IsModified = false;
    }

    /// <summary>
    /// Checks if any entry is modified and updates the table's modified state.
    /// </summary>
    public void UpdateModifiedState()
    {
        IsModified = Entries.Any(e => e.IsModified || e.IsNew);
    }

    private void OnEntriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (GxtEntryModel entry in e.NewItems)
            {
                entry.Modified += OnEntryModified;
            }
        }

        if (e.OldItems != null)
        {
            foreach (GxtEntryModel entry in e.OldItems)
            {
                entry.Modified -= OnEntryModified;
            }
        }
    }

    private void OnEntryModified(object? sender, System.EventArgs e)
    {
        UpdateModifiedState();
    }
}

