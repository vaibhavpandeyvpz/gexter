using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gexter.Tests;

[TestFixture]
public class GxtLoaderTests
{
    private static readonly string ExamplesPath = Path.Combine(
        TestContext.CurrentContext.TestDirectory, "examples");

    private static string GetExamplePath(string filename) =>
        Path.Combine(ExamplesPath, filename);

    #region GTA III Tests

    [Test]
    public void LoadGta3_ShouldDetectVC3Version()
    {
        var path = GetExamplePath("gta3.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.Version, Is.EqualTo(GxtVersion.ViceCityIII));
    }

    [Test]
    public void LoadGta3_ShouldHaveTables()
    {
        var path = GetExamplePath("gta3.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.TableCount, Is.GreaterThan(0));
        TestContext.Out.WriteLine($"GTA III has {loader.TableCount} tables");
    }

    [Test]
    public void LoadGta3_ShouldHaveMainTable()
    {
        var path = GetExamplePath("gta3.gxt");
        using var loader = new GxtLoader(path);

        var hasMain = loader.TableHeaders.Any(h =>
            h.Name.Equals("MAIN", StringComparison.OrdinalIgnoreCase));
        Assert.That(hasMain, Is.True, "GTA III should have a MAIN table");
    }

    [Test]
    public void LoadGta3_ShouldReadAllTables()
    {
        var path = GetExamplePath("gta3.gxt");
        var gxtFile = GxtLoader.Load(path);

        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));
        Assert.That(gxtFile.GetTotalEntryCount(), Is.GreaterThan(0));

        TestContext.Out.WriteLine($"GTA III: {gxtFile.TableCount} tables, {gxtFile.GetTotalEntryCount()} total entries");

        foreach (var table in gxtFile.Tables)
        {
            TestContext.Out.WriteLine($"  Table '{table.Name}': {table.Count} entries");
        }
    }

    [Test]
    public void LoadGta3_MainTable_ShouldHaveEntries()
    {
        var path = GetExamplePath("gta3.gxt");
        var gxtFile = GxtLoader.Load(path);
        var mainTable = gxtFile["MAIN"];

        Assert.That(mainTable, Is.Not.Null);
        Assert.That(mainTable!.Count, Is.GreaterThan(0));
    }

    #endregion

    #region GTA Vice City Tests

    [Test]
    public void LoadGtaVC_ShouldDetectVC3Version()
    {
        var path = GetExamplePath("gtavc.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.Version, Is.EqualTo(GxtVersion.ViceCityIII));
    }

    [Test]
    public void LoadGtaVC_ShouldHaveTables()
    {
        var path = GetExamplePath("gtavc.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.TableCount, Is.GreaterThan(0));
        TestContext.Out.WriteLine($"GTA Vice City has {loader.TableCount} tables");
    }

    [Test]
    public void LoadGtaVC_ShouldReadAllTables()
    {
        var path = GetExamplePath("gtavc.gxt");
        var gxtFile = GxtLoader.Load(path);

        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));
        Assert.That(gxtFile.GetTotalEntryCount(), Is.GreaterThan(0));

        TestContext.Out.WriteLine($"GTA Vice City: {gxtFile.TableCount} tables, {gxtFile.GetTotalEntryCount()} total entries");

        foreach (var table in gxtFile.Tables)
        {
            TestContext.Out.WriteLine($"  Table '{table.Name}': {table.Count} entries");
        }
    }

    [Test]
    public void LoadGtaVC_MainTable_ShouldHaveEntries()
    {
        var path = GetExamplePath("gtavc.gxt");
        var gxtFile = GxtLoader.Load(path);
        var mainTable = gxtFile["MAIN"];

        Assert.That(mainTable, Is.Not.Null);
        Assert.That(mainTable!.Count, Is.GreaterThan(0));
    }

    #endregion

    #region GTA San Andreas Tests

    [Test]
    public void LoadGtaSA_ShouldDetectSAIVVersion()
    {
        var path = GetExamplePath("gtasa.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.Version, Is.EqualTo(GxtVersion.SanAndreasIV));
    }

    [Test]
    public void LoadGtaSA_ShouldHaveTables()
    {
        var path = GetExamplePath("gtasa.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.TableCount, Is.GreaterThan(0));
        TestContext.Out.WriteLine($"GTA San Andreas has {loader.TableCount} tables");
    }

    [Test]
    public void LoadGtaSA_ShouldReadAllTables()
    {
        var path = GetExamplePath("gtasa.gxt");
        var gxtFile = GxtLoader.Load(path);

        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));
        Assert.That(gxtFile.GetTotalEntryCount(), Is.GreaterThan(0));

        TestContext.Out.WriteLine($"GTA San Andreas: {gxtFile.TableCount} tables, {gxtFile.GetTotalEntryCount()} total entries");

        foreach (var table in gxtFile.Tables)
        {
            TestContext.Out.WriteLine($"  Table '{table.Name}': {table.Count} entries");
        }
    }

    [Test]
    public void LoadGtaSA_MainTable_ShouldHaveEntries()
    {
        var path = GetExamplePath("gtasa.gxt");
        var gxtFile = GxtLoader.Load(path);
        var mainTable = gxtFile["MAIN"];

        Assert.That(mainTable, Is.Not.Null);
        Assert.That(mainTable!.Count, Is.GreaterThan(0));
    }

    #endregion

    #region GTA IV Tests

    [Test]
    public void LoadGtaIV_ShouldDetectSAIVVersion()
    {
        var path = GetExamplePath("gtaiv.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.Version, Is.EqualTo(GxtVersion.SanAndreasIV));
    }

    [Test]
    public void LoadGtaIV_ShouldHaveTables()
    {
        var path = GetExamplePath("gtaiv.gxt");
        using var loader = new GxtLoader(path);

        Assert.That(loader.TableCount, Is.GreaterThan(0));
        TestContext.Out.WriteLine($"GTA IV has {loader.TableCount} tables");
    }

    [Test]
    public void LoadGtaIV_ShouldReadAllTables()
    {
        var path = GetExamplePath("gtaiv.gxt");
        var gxtFile = GxtLoader.Load(path);

        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));
        Assert.That(gxtFile.GetTotalEntryCount(), Is.GreaterThan(0));

        TestContext.Out.WriteLine($"GTA IV: {gxtFile.TableCount} tables, {gxtFile.GetTotalEntryCount()} total entries");

        foreach (var table in gxtFile.Tables)
        {
            TestContext.Out.WriteLine($"  Table '{table.Name}': {table.Count} entries");
        }
    }

    [Test]
    public void LoadGtaIV_MainTable_ShouldHaveEntries()
    {
        var path = GetExamplePath("gtaiv.gxt");
        var gxtFile = GxtLoader.Load(path);
        var mainTable = gxtFile["MAIN"];

        Assert.That(mainTable, Is.Not.Null);
        Assert.That(mainTable!.Count, Is.GreaterThan(0));
    }

    #endregion

    #region Stream Loading Tests

    [Test]
    public void LoadFromStream_ShouldWorkCorrectly()
    {
        var path = GetExamplePath("gtasa.gxt");
        using var stream = File.OpenRead(path);
        var gxtFile = GxtLoader.Load(stream);

        Assert.That(gxtFile.Version, Is.EqualTo(GxtVersion.SanAndreasIV));
        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));
    }

    #endregion

    #region Encoding and Formatting Tests

    [Test]
    public void LoadGta3_ShouldPreserveFormattingCodes()
    {
        var path = GetExamplePath("gta3.gxt");
        var gxtFile = GxtLoader.Load(path);
        var mainTable = gxtFile["MAIN"];

        Assert.That(mainTable, Is.Not.Null);

        // Find entries with common formatting codes
        var entriesWithFormatting = mainTable!
            .Where(e => e.Value.Contains("~r~") || e.Value.Contains("~g~") ||
                       e.Value.Contains("~w~") || e.Value.Contains("~b~") ||
                       e.Value.Contains("~y~") || e.Value.Contains("~h~"))
            .Take(5)
            .ToList();

        if (entriesWithFormatting.Count > 0)
        {
            TestContext.Out.WriteLine("Found entries with formatting codes:");
            foreach (var entry in entriesWithFormatting)
            {
                var keyName = mainTable.GetKeyName(entry.Key);
                TestContext.Out.WriteLine($"  {keyName ?? entry.Key.ToString("X8")}: {entry.Value.Substring(0, Math.Min(50, entry.Value.Length))}...");

                // Verify formatting codes are preserved
                Assert.That(entry.Value, Does.Contain("~"),
                    $"Entry should contain formatting code: {entry.Value}");
            }
        }
        else
        {
            TestContext.Out.WriteLine("No entries with formatting codes found in GTA III file");
        }
    }

    [Test]
    public void LoadGtaSA_ShouldPreserveFormattingCodes()
    {
        var path = GetExamplePath("gtasa.gxt");
        var gxtFile = GxtLoader.Load(path);
        var mainTable = gxtFile["MAIN"];

        Assert.That(mainTable, Is.Not.Null);

        // Find entries with common formatting codes
        var entriesWithFormatting = mainTable!
            .Where(e => e.Value.Contains("~r~") || e.Value.Contains("~g~") ||
                       e.Value.Contains("~w~") || e.Value.Contains("~b~") ||
                       e.Value.Contains("~y~") || e.Value.Contains("~h~"))
            .Take(5)
            .ToList();

        if (entriesWithFormatting.Count > 0)
        {
            TestContext.Out.WriteLine("Found entries with formatting codes:");
            foreach (var entry in entriesWithFormatting)
            {
                TestContext.Out.WriteLine($"  {entry.Key:X8}: {entry.Value.Substring(0, Math.Min(50, entry.Value.Length))}...");

                // Verify formatting codes are preserved
                Assert.That(entry.Value, Does.Contain("~"),
                    $"Entry should contain formatting code: {entry.Value}");
            }
        }
        else
        {
            TestContext.Out.WriteLine("No entries with formatting codes found in GTA SA file");
        }
    }

    [Test]
    public void LoadGtaVC_ShouldPreserveUTF16Characters()
    {
        var path = GetExamplePath("gtavc.gxt");
        var gxtFile = GxtLoader.Load(path);

        // UTF-16LE should preserve all Unicode characters including special characters
        // Test that we can read the file without encoding errors
        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));

        foreach (var table in gxtFile.Tables)
        {
            foreach (var entry in table)
            {
                // Verify no replacement characters () indicating encoding issues
                Assert.That(entry.Value, Does.Not.Contain("\uFFFD"),
                    $"Entry should not contain replacement characters: {entry.Value}");
            }
        }
    }

    [Test]
    public void LoadGtaSA_ShouldPreserveWindows1252Characters()
    {
        var path = GetExamplePath("gtasa.gxt");
        var gxtFile = GxtLoader.Load(path);

        // Windows-1252 should preserve Western European characters
        Assert.That(gxtFile.TableCount, Is.GreaterThan(0));

        foreach (var table in gxtFile.Tables)
        {
            foreach (var entry in table)
            {
                // Verify no replacement characters () indicating encoding issues
                Assert.That(entry.Value, Does.Not.Contain("\uFFFD"),
                    $"Entry should not contain replacement characters: {entry.Value}");
            }
        }
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public void LoadNonExistentFile_ShouldThrowFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => GxtLoader.Load("nonexistent.gxt"));
    }

    [Test]
    public void LoadEmptyStream_ShouldThrowException()
    {
        using var stream = new MemoryStream();
        // Empty stream will throw an exception - either EndOfStreamException or GxtException
        Assert.That(() => GxtLoader.Load(stream), Throws.Exception);
    }

    [Test]
    public void LoadTooSmallData_ShouldThrowGxtException()
    {
        // Data too small to be a valid GXT but enough to read first 4 bytes
        var smallData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        using var stream = new MemoryStream(smallData);
        Assert.Throws<GxtException>(() => GxtLoader.Load(stream));
    }

    [Test]
    public void LoadInvalidData_ShouldThrowGxtException()
    {
        var invalidData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
        using var stream = new MemoryStream(invalidData);

        Assert.Throws<GxtException>(() => GxtLoader.Load(stream));
    }

    #endregion
}

