# Gexter

<div align="center">

**A powerful tool for viewing and editing GXT (game text) files from Grand Theft Auto games**

![Screenshot](assets/screenshot.png)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![NuGet](https://img.shields.io/badge/NuGet-Gexter-orange.svg)](https://www.nuget.org/packages/Gexter)

[Features](#-features) • [Supported Games](#-supported-games) • [Installation](#-installation) • [Usage](#-usage) • [Library](#-library) • [Contributing](#-contributing) • [License](#-license)

</div>

---

## ✨ Features

### Desktop Application
- **Modern Dark UI** - Beautiful flat design with custom window chrome
- **Multi-Game Support** - Works with GTA III, Vice City, San Andreas, and GTA IV
- **Full CRUD Operations** - Add, edit, rename, duplicate, and delete tables and entries
- **Advanced Search & Filter** - Search entries and filter tables with case-sensitive option
- **Multi-line Value Editor** - Dedicated editor for editing long text values with word wrap
- **Real-time Modification Tracking** - Visual indicators show unsaved changes on tables and entries
- **Export Functionality** - Export to CSV or JSON formats
- **Keyboard Shortcuts** - Efficient workflow with Ctrl+O, Ctrl+S, Ctrl+F, and more
- **Drag & Drop Support** - Simply drag GXT files onto the window to open them
- **Formatting Code Preservation** - Correctly handles and preserves game formatting codes like `~r~`, `~g~`, `~b~`, etc.
- **Multi-lingual Support** - Proper encoding handling for UTF-16LE (III/VC) and Windows-1252 (SA/IV)

### Library
- **Cross-Platform** - Supports .NET Framework 4.5.2+, .NET Standard 2.0, .NET Core 3.1+, and .NET 5.0, 7.0-10.0
- **Type-Safe API** - Clean, intuitive API for reading and manipulating GXT files
- **Read & Write Support** - Full support for reading and writing GXT files in all supported formats
- **Memory Efficient** - Stream-based reading and writing for large files
- **Key Name Preservation** - Maintains original key names for VC/III formats
- **Comprehensive Error Handling** - Detailed exceptions for debugging

## 🎮 Supported Games

| Game | Format | Encoding | Status |
|------|--------|----------|--------|
| **GTA III** | Single-table (TKEY) | UTF-16LE | ✅ Fully Supported |
| **GTA: Vice City** | Multi-table (TABL) | UTF-16LE | ✅ Fully Supported |
| **GTA: San Andreas** | Multi-table (TABL) | Windows-1252 | ✅ Fully Supported |
| **GTA IV** | Multi-table (TABL) | Windows-1252 | ✅ Fully Supported |

## 📦 Installation

### Desktop Application

1. Download the latest release from the [Releases](https://github.com/vaibhavpandeyvpz/gexter/releases) page
2. Extract the ZIP file
3. Run `Gexter.Desktop.exe`

**Requirements:**
- Windows 7 or later
- No additional dependencies (self-contained)

### Library (NuGet)

```bash
dotnet add package Gexter
```

Or via Package Manager:
```
Install-Package Gexter
```

## 🚀 Usage

### Desktop Application

1. **Open a GXT file:**
   - Click `File > Open` or press `Ctrl+O`
   - Or drag and drop a `.gxt` file onto the window

2. **Navigate:**
   - Select a table from the left panel
   - View entries in the middle panel
   - Edit values in the bottom panel

3. **Search:**
   - Use the search box in the entries section
   - Toggle case-sensitive search as needed

4. **Edit:**
   - Click on an entry to edit its value
   - Use the action buttons or context menu to add/rename/delete entries
   - Modified entries are marked with an orange dot

5. **Save:**
   - Click `File > Save` or press `Ctrl+S`
   - Use `File > Save As...` to save to a new location

6. **Export:**
   - Use `Export > Export to CSV...` or `Export > Export to JSON...`

### Library

```csharp
using Gexter;

// Load a GXT file
var gxtFile = GxtLoader.Load("american.gxt");

// Access tables
var mainTable = gxtFile["MAIN"];
if (mainTable != null)
{
    // Get value by hash
    var value = mainTable.GetValue(0x89BD20D0);
    
    // Get value by key name (VC/III)
    var value2 = mainTable.GetValue("MAIN");
    
    // Set a value
    mainTable.SetValue("MY_KEY", "My custom text");
    
    // Iterate entries
    foreach (var entry in mainTable)
    {
        Console.WriteLine($"Hash: {entry.Key:X8}, Value: {entry.Value}");
    }
}

// Search across all tables
var foundValue = gxtFile.FindValue("SOME_KEY");

// Save the modified file
gxtFile.Save("modified.gxt");
```

## 📚 Library

### Key Classes

- **`GxtLoader`** - Main class for loading GXT files from streams or files
- **`GxtFile`** - Container for all tables in a GXT file with save functionality
- **`GxtWriter`** - Class for writing GXT files to streams or files
- **`GxtTable`** - Represents a single table with key-value pairs
- **`GxtVersion`** - Enum for GXT format versions
- **`Crc32`** - Utility for computing CRC32 hashes

### Example: Reading a GXT File

```csharp
using var loader = GxtLoader.FromFile("american.gxt");
var gxtFile = loader.Load();

Console.WriteLine($"Version: {gxtFile.Version}");
Console.WriteLine($"Tables: {gxtFile.TableCount}");

foreach (var table in gxtFile)
{
    Console.WriteLine($"Table: {table.Name} ({table.Count} entries)");
}
```

### Example: Creating and Saving a New GXT File

```csharp
var tables = new List<GxtTable>
{
    new GxtTable("MAIN", Encoding.Unicode, keepKeyNames: true)
};

tables[0].SetValue("HELLO", "Hello, World!");
tables[0].SetValue("GOODBYE", "Goodbye!");

var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

// Save to file
gxtFile.Save("output.gxt");

// Or save to stream
using var stream = File.Create("output.gxt");
gxtFile.Save(stream);
```

### Example: Modifying and Saving an Existing GXT File

```csharp
// Load a GXT file
var gxtFile = GxtLoader.Load("american.gxt");

// Modify entries
var mainTable = gxtFile["MAIN"];
if (mainTable != null)
{
    mainTable.SetValue("NEW_KEY", "New value");
    mainTable.SetValue(0x89BD20D0, "Updated value");
}

// Save the modified file
gxtFile.Save("modified.gxt");
```

## 🛠️ Development

### Building from Source

```bash
# Clone the repository
git clone https://github.com/vaibhavpandeyvpz/gexter.git
cd gexter

# Restore dependencies
dotnet restore

# Build the library
dotnet build Gexter

# Build the desktop app
dotnet build Gexter.Desktop

# Run tests
dotnet test Gexter.Tests
```

### Project Structure

```
gexter/
├── Gexter/              # Core library
├── Gexter.Desktop/      # WPF desktop application
├── Gexter.Tests/        # NUnit test suite
└── examples/            # Sample GXT files for testing
```

## 🧪 Testing

The project includes comprehensive unit tests covering:
- CRC32 hash computation
- GXT file loading for all supported formats
- GXT file writing for all supported formats
- Round-trip testing (load, save, reload) to verify data integrity
- Encoding handling (UTF-16LE and Windows-1252)
- Formatting code preservation
- Error handling

Run tests with:
```bash
dotnet test
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Vaibhav Pandey (VPZ)**

- Website: [vaibhavpandey.com](https://vaibhavpandey.com)
- Email: contact@vaibhavpandey.com
- GitHub: [@vaibhavpandeyvpz](https://github.com/vaibhavpandeyvpz)
- YouTube: [Channel](https://www.youtube.com/channel/UC5uV1PRvtnNj9P8VfqO93Pw)

## 🙏 Acknowledgments

- **libgtaformats** - This project is a C# port (or based on) of the C++ library [libgtaformats](https://github.com/alemariusnexus/gtatools/tree/master/src/libgtaformats/src/gtaformats/gxt) by David "Alemarius Nexus" Lerch
- Grand Theft Auto series by Rockstar Games
- FontAwesome for icons
- Community contributors and testers

## 📞 Support

If you encounter any issues or have questions:
- Open an issue on [GitHub](https://github.com/vaibhavpandeyvpz/gexter/issues)
- Contact: contact@vaibhavpandey.com

---

<div align="center">

Made with ❤️ for the GTA modding community

⭐ Star this repo if you find it useful!

</div>

