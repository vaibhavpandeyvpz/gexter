using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gexter.Desktop.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace Gexter.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private GxtFile? _loadedFile;
    private string? _currentFilePath;

    [ObservableProperty]
    private string _windowTitle = "Gexter by VPZ";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private GxtVersion _fileVersion;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _tableSearchText = string.Empty;

    [ObservableProperty]
    private GxtTableModel? _selectedTable;

    [ObservableProperty]
    private GxtEntryModel? _selectedEntry;

    [ObservableProperty]
    private bool _caseSensitive;

    [ObservableProperty]
    private string _gameName = string.Empty;

    [ObservableProperty]
    private System.Windows.Media.Brush _gameColor = System.Windows.Media.Brushes.Gray;

    [ObservableProperty]
    private int _totalTables;

    [ObservableProperty]
    private int _totalEntries;

    public ObservableCollection<GxtTableModel> Tables { get; } = new();
    public ObservableCollection<GxtTableModel> FilteredTables { get; } = new();
    public ObservableCollection<GxtEntryModel> FilteredEntries { get; } = new();

    [ObservableProperty]
    private bool _canEditEntry;

    [ObservableProperty]
    private bool _hasSelectedTable;

    partial void OnSelectedTableChanged(GxtTableModel? value)
    {
        HasSelectedTable = value != null;
        ApplyEntryFilter();
        // Notify commands that depend on SelectedTable
        AddEntryCommand.NotifyCanExecuteChanged();
        RenameEntryCommand.NotifyCanExecuteChanged();
        DuplicateEntryCommand.NotifyCanExecuteChanged();
        DeleteEntryCommand.NotifyCanExecuteChanged();
        RenameTableCommand.NotifyCanExecuteChanged();
        RemoveTableCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedEntryChanged(GxtEntryModel? value)
    {
        CanEditEntry = value != null;
        // Notify commands that depend on SelectedEntry
        RenameEntryCommand.NotifyCanExecuteChanged();
        DuplicateEntryCommand.NotifyCanExecuteChanged();
        DeleteEntryCommand.NotifyCanExecuteChanged();
        CopyKeyToClipboardCommand.NotifyCanExecuteChanged();
        CopyValueToClipboardCommand.NotifyCanExecuteChanged();
        CopyHashToClipboardCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyEntryFilter();
    }

    partial void OnTableSearchTextChanged(string value)
    {
        ApplyTableFilter();
    }

    partial void OnCaseSensitiveChanged(bool value) => ApplyEntryFilter();

    [RelayCommand]
    private void OpenFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "GXT Files (*.gxt)|*.gxt|All Files (*.*)|*.*",
            Title = "Open GXT File"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadFile(dialog.FileName);
        }
    }

    [RelayCommand]
    private void SaveFile()
    {
        if (!IsFileLoaded || _loadedFile == null || string.IsNullOrEmpty(_currentFilePath)) return;

        try
        {
            SyncModelsToFile();
            _loadedFile.Save(_currentFilePath);

            // Reset modified state after successful save
            foreach (var table in Tables)
            {
                table.ResetModified();
            }

            StatusMessage = $"Saved to {Path.GetFileName(_currentFilePath)}";
        }
        catch (Exception ex)
        {
            MessageDialog.Show($"Error saving file: {ex.Message}", "Save Error",
                MessageDialogType.Error, MessageDialogButtons.Ok);
        }
    }

    [RelayCommand]
    private void SaveFileAs()
    {
        if (!IsFileLoaded || _loadedFile == null) return;

        var dialog = new SaveFileDialog
        {
            Filter = "GXT Files (*.gxt)|*.gxt|All Files (*.*)|*.*",
            Title = "Save GXT File As",
            FileName = _currentFilePath != null ? Path.GetFileName(_currentFilePath) : "untitled.gxt"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                SyncModelsToFile();
                _loadedFile.Save(dialog.FileName);
                _currentFilePath = dialog.FileName;

                // Reset modified state after successful save
                foreach (var table in Tables)
                {
                    table.ResetModified();
                }

                WindowTitle = $"Gexter by VPZ - {Path.GetFileName(dialog.FileName)}";
                StatusMessage = $"Saved to {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageDialog.Show($"Error saving file: {ex.Message}", "Save Error",
                    MessageDialogType.Error, MessageDialogButtons.Ok);
            }
        }
    }

    [RelayCommand]
    private void CloseFile()
    {
        Tables.Clear();
        FilteredTables.Clear();
        FilteredEntries.Clear();
        _loadedFile = null;
        _currentFilePath = null;
        IsFileLoaded = false;
        TotalTables = 0;
        TotalEntries = 0;
        SelectedTable = null;
        SelectedEntry = null;
        GameName = string.Empty;
        GameColor = System.Windows.Media.Brushes.Gray;
        WindowTitle = "Gexter by VPZ";
        StatusMessage = "Ready";
    }

    [RelayCommand]
    private void ExportToCsv()
    {
        if (!IsFileLoaded || _loadedFile == null) return;

        var dialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            Title = "Export to CSV",
            FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + ".csv"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                ExportTablesToCsv(dialog.FileName);
                StatusMessage = $"Exported to {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageDialog.Show($"Error exporting: {ex.Message}", "Export Error",
                    MessageDialogType.Error, MessageDialogButtons.Ok);
            }
        }
    }

    [RelayCommand]
    private void ExportToJson()
    {
        if (!IsFileLoaded || _loadedFile == null) return;

        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Export to JSON",
            FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + ".json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                ExportTablesToJson(dialog.FileName);
                StatusMessage = $"Exported to {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageDialog.Show($"Error exporting: {ex.Message}", "Export Error",
                    MessageDialogType.Error, MessageDialogButtons.Ok);
            }
        }
    }

    #region Table Management

    [RelayCommand]
    private void AddTable()
    {
        var (confirmed, name) = InputDialog.Show("Enter table name:", "Add Table", "NEW_TABLE");
        if (!confirmed || string.IsNullOrWhiteSpace(name)) return;

        name = name.ToUpperInvariant();
        if (name.Length > 8) name = name[..8];

        if (Tables.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageDialog.Show($"Table '{name}' already exists.", "Add Table",
                MessageDialogType.Warning, MessageDialogButtons.Ok);
            return;
        }

        var newTable = new GxtTableModel { Name = name, IsModified = true };
        Tables.Add(newTable);
        ApplyTableFilter();
        SelectedTable = newTable;
        TotalTables = Tables.Count;
        StatusMessage = $"Added table '{name}'";
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTable))]
    private void RenameTable()
    {
        if (SelectedTable == null) return;

        var (confirmed, newName) = InputDialog.Show("Enter new table name:", "Rename Table", SelectedTable.Name);
        if (!confirmed || string.IsNullOrWhiteSpace(newName)) return;

        newName = newName.ToUpperInvariant();
        if (newName.Length > 8) newName = newName[..8];

        if (newName != SelectedTable.Name && Tables.Any(t => t.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageDialog.Show($"Table '{newName}' already exists.", "Rename Table",
                MessageDialogType.Warning, MessageDialogButtons.Ok);
            return;
        }

        var oldName = SelectedTable.Name;
        SelectedTable.Name = newName;
        SelectedTable.IsModified = true;
        ApplyTableFilter();
        StatusMessage = $"Renamed table '{oldName}' to '{newName}'";
    }

    [RelayCommand(CanExecute = nameof(HasSelectedTable))]
    private void RemoveTable()
    {
        if (SelectedTable == null) return;

        var result = MessageDialog.Show($"Remove table '{SelectedTable.Name}' and all its entries?",
            "Confirm Remove", MessageDialogType.Question, MessageDialogButtons.YesNo);

        if (result == MessageDialogResult.Yes)
        {
            var name = SelectedTable.Name;
            Tables.Remove(SelectedTable);
            ApplyTableFilter();
            TotalTables = Tables.Count;
            StatusMessage = $"Removed table '{name}'";
        }
    }

    #endregion

    #region Entry Management

    [RelayCommand(CanExecute = nameof(HasSelectedTable))]
    private void AddEntry()
    {
        if (SelectedTable == null) return;

        var (confirmed, keyName) = InputDialog.Show("Enter key name:", "Add Entry", "NEW_KEY");
        if (!confirmed || string.IsNullOrWhiteSpace(keyName)) return;

        keyName = keyName.ToUpperInvariant();

        var newEntry = new GxtEntryModel
        {
            KeyName = keyName,
            KeyHash = Crc32.Compute(keyName),
            Value = "",
            IsNew = true,
            IsModified = true
        };

        SelectedTable.Entries.Add(newEntry);
        SelectedTable.RefreshEntryCount();
        SelectedTable.IsModified = true;
        TotalEntries++;
        ApplyEntryFilter();
        SelectedEntry = newEntry;
        StatusMessage = $"Added entry '{keyName}'";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void RenameEntry()
    {
        if (SelectedTable == null || SelectedEntry == null) return;

        var currentKey = !string.IsNullOrEmpty(SelectedEntry.KeyName) ? SelectedEntry.KeyName : SelectedEntry.KeyHashHex;
        var (confirmed, newName) = InputDialog.Show("Enter new key name:", "Rename Entry", currentKey);
        if (!confirmed || string.IsNullOrWhiteSpace(newName)) return;

        newName = newName.ToUpperInvariant();
        SelectedEntry.KeyName = newName;
        SelectedEntry.KeyHash = Crc32.Compute(newName);
        SelectedEntry.IsModified = true;
        SelectedTable.IsModified = true;
        StatusMessage = $"Renamed entry to '{newName}'";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void DeleteEntry()
    {
        if (SelectedTable == null || SelectedEntry == null) return;

        var result = MessageDialog.Show($"Delete entry '{SelectedEntry.DisplayKey}'?",
            "Confirm Delete", MessageDialogType.Question, MessageDialogButtons.YesNo);

        if (result == MessageDialogResult.Yes)
        {
            SelectedTable.Entries.Remove(SelectedEntry);
            FilteredEntries.Remove(SelectedEntry);
            SelectedTable.RefreshEntryCount();
            SelectedTable.IsModified = true;
            TotalEntries--;
            StatusMessage = "Entry deleted";
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void DuplicateEntry()
    {
        if (SelectedTable == null || SelectedEntry == null) return;

        var newKeyName = SelectedEntry.KeyName + "_COPY";
        var duplicate = new GxtEntryModel
        {
            KeyName = newKeyName,
            KeyHash = Crc32.Compute(newKeyName),
            Value = SelectedEntry.Value,
            IsNew = true,
            IsModified = true
        };

        SelectedTable.Entries.Add(duplicate);
        SelectedTable.RefreshEntryCount();
        SelectedTable.IsModified = true;
        TotalEntries++;
        ApplyEntryFilter();
        SelectedEntry = duplicate;
        StatusMessage = "Entry duplicated";
    }

    #endregion

    #region Clipboard

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyKeyToClipboard()
    {
        if (SelectedEntry == null) return;
        Clipboard.SetText(SelectedEntry.DisplayKey);
        StatusMessage = "Key copied to clipboard";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyValueToClipboard()
    {
        if (SelectedEntry == null) return;
        Clipboard.SetText(SelectedEntry.Value);
        StatusMessage = "Value copied to clipboard";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyHashToClipboard()
    {
        if (SelectedEntry == null) return;
        Clipboard.SetText(SelectedEntry.KeyHashHex);
        StatusMessage = "Hash copied to clipboard";
    }

    #endregion

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void ClearTableSearch()
    {
        TableSearchText = string.Empty;
    }

    [RelayCommand]
    private void SelectAllTables()
    {
        SelectedTable = null;
        ApplyEntryFilter();
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var aboutWindow = new AboutWindow
        {
            Owner = Application.Current.MainWindow
        };
        aboutWindow.ShowDialog();
    }

    public void LoadFile(string filePath)
    {
        try
        {
            StatusMessage = "Loading...";
            Tables.Clear();
            FilteredTables.Clear();
            FilteredEntries.Clear();

            _loadedFile = GxtLoader.Load(filePath, keepKeyNames: true);
            _currentFilePath = filePath;
            FileVersion = _loadedFile.Version;

            int totalEntries = 0;

            foreach (var table in _loadedFile.Tables)
            {
                var tableModel = new GxtTableModel { Name = table.Name };

                foreach (var entry in table)
                {
                    var keyName = table.GetKeyName(entry.Key) ?? string.Empty;
                    var entryModel = new GxtEntryModel
                    {
                        KeyHash = entry.Key,
                        KeyName = keyName,
                        Value = entry.Value
                    };
                    tableModel.Entries.Add(entryModel);
                    totalEntries++;
                }

                tableModel.RefreshEntryCount();
                tableModel.MarkAsLoaded(); // Mark all entries as loaded to reset modified state
                Tables.Add(tableModel);
            }

            TotalTables = Tables.Count;
            TotalEntries = totalEntries;
            IsFileLoaded = true;

            // Set game name and color based on file version
            UpdateGameInfo();

            ApplyTableFilter();

            // Select first table
            if (FilteredTables.Count > 0)
            {
                SelectedTable = FilteredTables[0];
            }

            WindowTitle = $"Gexter by VPZ - {Path.GetFileName(filePath)}";
            StatusMessage = $"Loaded {TotalTables} tables, {TotalEntries} entries";
        }
        catch (Exception ex)
        {
            MessageDialog.Show($"Error loading file: {ex.Message}", "Load Error",
                MessageDialogType.Error, MessageDialogButtons.Ok);
            StatusMessage = "Error loading file";
        }
    }

    private void ApplyTableFilter()
    {
        FilteredTables.Clear();

        IEnumerable<GxtTableModel> tables = Tables;

        if (!string.IsNullOrWhiteSpace(TableSearchText))
        {
            var search = TableSearchText.ToLowerInvariant();
            tables = tables.Where(t => t.Name.ToLowerInvariant().Contains(search));
        }

        // Sort by name
        tables = tables.OrderBy(t => t.Name);

        foreach (var table in tables)
        {
            FilteredTables.Add(table);
        }
    }

    private void ApplyEntryFilter()
    {
        FilteredEntries.Clear();

        IEnumerable<GxtEntryModel> entries;

        if (SelectedTable != null)
        {
            entries = SelectedTable.Entries;
        }
        else
        {
            // Show all entries from all tables
            entries = Tables.SelectMany(t => t.Entries);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchTerm = CaseSensitive ? SearchText : SearchText.ToLowerInvariant();

            entries = entries.Where(e =>
            {
                var key = CaseSensitive ? e.DisplayKey : e.DisplayKey.ToLowerInvariant();
                var value = CaseSensitive ? e.Value : e.Value.ToLowerInvariant();
                return key.Contains(searchTerm) || value.Contains(searchTerm);
            });
        }

        // Sort by key name
        entries = entries.OrderBy(e => e.DisplayKey);

        foreach (var entry in entries)
        {
            FilteredEntries.Add(entry);
        }

        OnPropertyChanged(nameof(FilteredEntries));
    }

    private void ExportTablesToCsv(string filePath)
    {
        using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        writer.WriteLine("Table,KeyHash,KeyName,Value");

        foreach (var table in Tables)
        {
            foreach (var entry in table.Entries)
            {
                var value = entry.Value.Replace("\"", "\"\"");
                writer.WriteLine($"\"{table.Name}\",\"{entry.KeyHashHex}\",\"{entry.KeyName}\",\"{value}\"");
            }
        }
    }

    private void ExportTablesToJson(string filePath)
    {
        using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        writer.WriteLine("{");
        writer.WriteLine($"  \"version\": \"{FileVersion}\",");
        writer.WriteLine("  \"tables\": [");

        for (int t = 0; t < Tables.Count; t++)
        {
            var table = Tables[t];
            writer.WriteLine("    {");
            writer.WriteLine($"      \"name\": \"{EscapeJson(table.Name)}\",");
            writer.WriteLine("      \"entries\": [");

            for (int e = 0; e < table.Entries.Count; e++)
            {
                var entry = table.Entries[e];
                var comma = e < table.Entries.Count - 1 ? "," : "";
                writer.WriteLine($"        {{ \"hash\": \"{entry.KeyHashHex}\", \"key\": \"{EscapeJson(entry.KeyName)}\", \"value\": \"{EscapeJson(entry.Value)}\" }}{comma}");
            }

            writer.WriteLine("      ]");
            var tableComma = t < Tables.Count - 1 ? "," : "";
            writer.WriteLine($"    }}{tableComma}");
        }

        writer.WriteLine("  ]");
        writer.WriteLine("}");
    }

    private static string EscapeJson(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private void SyncModelsToFile()
    {
        if (_loadedFile == null) return;

        // Rebuild tables from UI models
        var newTables = new List<GxtTable>();

        foreach (var tableModel in Tables)
        {
            // Get encoding from original table or use default
            var originalTable = _loadedFile[tableModel.Name];
            var encoding = originalTable?.InternalEncoding ??
                (FileVersion == GxtVersion.ViceCityIII
                    ? System.Text.Encoding.Unicode
                    : System.Text.Encoding.GetEncoding(1252));

            // Create new table with same name and encoding
            var newTable = new GxtTable(tableModel.Name, encoding, keepKeyNames: true);

            // Copy all entries from UI model
            foreach (var entryModel in tableModel.Entries)
            {
                if (!string.IsNullOrEmpty(entryModel.KeyName))
                {
                    // Use key name if available (for VC/III)
                    newTable.SetValue(entryModel.KeyName, entryModel.Value);
                }
                else
                {
                    // Use hash directly (for SA/IV)
                    newTable.SetValue(entryModel.KeyHash, entryModel.Value);
                }
            }

            newTables.Add(newTable);
        }

        // Rebuild GxtFile with updated tables
        _loadedFile = new GxtFile(FileVersion, newTables);
    }

    private void UpdateGameInfo()
    {
        // Determine game name and color based on file version
        // ViceCityIII = GTA III and GTA VC (single table = III, multiple = VC)
        // SanAndreasIV = GTA SA and GTA IV (determined by file content)

        if (FileVersion == GxtVersion.ViceCityIII)
        {
            // GTA III has single MAIN table, GTA VC has multiple tables
            if (TotalTables == 1 && Tables.FirstOrDefault()?.Name == "MAIN")
            {
                GameName = "GTA:III";
                GameColor = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#5FA3D8"));
            }
            else
            {
                GameName = "GTA:VC";
                GameColor = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E75AA1"));
            }
        }
        else if (FileVersion == GxtVersion.SanAndreasIV)
        {
            // SA typically has many more entries than IV
            // This is a heuristic - could be improved
            if (TotalEntries > 5000)
            {
                GameName = "GTA:SA";
                GameColor = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F5A623"));
            }
            else
            {
                GameName = "GTA:IV";
                GameColor = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9EBF4E"));
            }
        }
        else
        {
            GameName = FileVersion.ToString();
            GameColor = System.Windows.Media.Brushes.Gray;
        }
    }
}
