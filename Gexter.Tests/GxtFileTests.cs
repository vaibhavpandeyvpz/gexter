using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gexter.Tests;

[TestFixture]
public class GxtFileTests
{
    [Test]
    public void Constructor_ShouldSetVersion()
    {
        var tables = new List<GxtTable>
        {
            new("MAIN", Encoding.UTF8)
        };

        var gxtFile = new GxtFile(GxtVersion.SanAndreasIV, tables);

        Assert.That(gxtFile.Version, Is.EqualTo(GxtVersion.SanAndreasIV));
    }

    [Test]
    public void TableCount_ShouldReturnCorrectCount()
    {
        var tables = new List<GxtTable>
        {
            new("MAIN", Encoding.UTF8),
            new("HELP", Encoding.UTF8),
            new("MISC", Encoding.UTF8)
        };

        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile.TableCount, Is.EqualTo(3));
    }

    [Test]
    public void IndexerByName_ShouldReturnTable()
    {
        var mainTable = new GxtTable("MAIN", Encoding.UTF8);
        var tables = new List<GxtTable> { mainTable };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        var result = gxtFile["MAIN"];

        Assert.That(result, Is.SameAs(mainTable));
    }

    [Test]
    public void IndexerByName_CaseInsensitive_ShouldWork()
    {
        var mainTable = new GxtTable("MAIN", Encoding.UTF8);
        var tables = new List<GxtTable> { mainTable };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile["main"], Is.SameAs(mainTable));
        Assert.That(gxtFile["Main"], Is.SameAs(mainTable));
        Assert.That(gxtFile["MAIN"], Is.SameAs(mainTable));
    }

    [Test]
    public void IndexerByName_NonExistent_ShouldReturnNull()
    {
        var tables = new List<GxtTable> { new("MAIN", Encoding.UTF8) };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile["NONEXISTENT"], Is.Null);
    }

    [Test]
    public void IndexerByIndex_ShouldReturnTable()
    {
        var mainTable = new GxtTable("MAIN", Encoding.UTF8);
        var helpTable = new GxtTable("HELP", Encoding.UTF8);
        var tables = new List<GxtTable> { mainTable, helpTable };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile[0], Is.SameAs(mainTable));
        Assert.That(gxtFile[1], Is.SameAs(helpTable));
    }

    [Test]
    public void TryGetTable_ExistingTable_ShouldReturnTrue()
    {
        var mainTable = new GxtTable("MAIN", Encoding.UTF8);
        var tables = new List<GxtTable> { mainTable };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        var result = gxtFile.TryGetTable("MAIN", out var table);

        Assert.That(result, Is.True);
        Assert.That(table, Is.SameAs(mainTable));
    }

    [Test]
    public void TryGetTable_NonExistent_ShouldReturnFalse()
    {
        var tables = new List<GxtTable> { new("MAIN", Encoding.UTF8) };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        var result = gxtFile.TryGetTable("NONEXISTENT", out var table);

        Assert.That(result, Is.False);
        Assert.That(table, Is.Null);
    }

    [Test]
    public void ContainsTable_ShouldWork()
    {
        var tables = new List<GxtTable> { new("MAIN", Encoding.UTF8) };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile.ContainsTable("MAIN"), Is.True);
        Assert.That(gxtFile.ContainsTable("NONEXISTENT"), Is.False);
    }

    [Test]
    public void GetTotalEntryCount_ShouldSumAllEntries()
    {
        var table1 = new GxtTable("MAIN", Encoding.UTF8);
        table1.SetValue("KEY1", "Value1");
        table1.SetValue("KEY2", "Value2");

        var table2 = new GxtTable("HELP", Encoding.UTF8);
        table2.SetValue("KEY3", "Value3");

        var tables = new List<GxtTable> { table1, table2 };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile.GetTotalEntryCount(), Is.EqualTo(3));
    }

    [Test]
    public void FindValue_ShouldSearchAllTables()
    {
        var table1 = new GxtTable("MAIN", Encoding.UTF8);
        table1.SetValue("KEY1", "Value1");

        var table2 = new GxtTable("HELP", Encoding.UTF8);
        table2.SetValue("KEY2", "Value2");

        var tables = new List<GxtTable> { table1, table2 };
        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);

        Assert.That(gxtFile.FindValue("KEY1"), Is.EqualTo("Value1"));
        Assert.That(gxtFile.FindValue("KEY2"), Is.EqualTo("Value2"));
        Assert.That(gxtFile.FindValue("NONEXISTENT"), Is.Null);
    }

    [Test]
    public void TableNames_ShouldReturnAllNames()
    {
        var tables = new List<GxtTable>
        {
            new("MAIN", Encoding.UTF8),
            new("HELP", Encoding.UTF8),
            new("MISC", Encoding.UTF8)
        };

        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);
        var names = gxtFile.TableNames.ToList();

        Assert.That(names.Count, Is.EqualTo(3));
        Assert.That(names, Does.Contain("MAIN"));
        Assert.That(names, Does.Contain("HELP"));
        Assert.That(names, Does.Contain("MISC"));
    }

    [Test]
    public void Enumeration_ShouldYieldAllTables()
    {
        var tables = new List<GxtTable>
        {
            new("MAIN", Encoding.UTF8),
            new("HELP", Encoding.UTF8)
        };

        var gxtFile = new GxtFile(GxtVersion.ViceCityIII, tables);
        var enumerated = gxtFile.ToList();

        Assert.That(enumerated.Count, Is.EqualTo(2));
    }

    [Test]
    public void ToString_ShouldIncludeVersionAndCounts()
    {
        var table = new GxtTable("MAIN", Encoding.UTF8);
        table.SetValue("KEY1", "Value1");

        var gxtFile = new GxtFile(GxtVersion.SanAndreasIV, new List<GxtTable> { table });
        var str = gxtFile.ToString();

        Assert.That(str, Does.Contain("SanAndreasIV"));
        Assert.That(str, Does.Contain("1"));
    }
}

