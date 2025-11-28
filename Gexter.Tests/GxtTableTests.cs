using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gexter.Tests;

[TestFixture]
public class GxtTableTests
{
    [Test]
    public void Constructor_ShouldSetProperties()
    {
        var table = new GxtTable("TEST", Encoding.UTF8, true);

        Assert.That(table.Name, Is.EqualTo("TEST"));
        Assert.That(table.InternalEncoding, Is.EqualTo(Encoding.UTF8));
        Assert.That(table.KeepKeyNames, Is.True);
        Assert.That(table.Count, Is.EqualTo(0));
    }

    [Test]
    public void SetValue_ByKeyHash_ShouldStoreValue()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        const uint keyHash = 0x12345678;
        const string value = "Test Value";

        table.SetValue(keyHash, value);

        Assert.That(table.Count, Is.EqualTo(1));
        Assert.That(table.GetValue(keyHash), Is.EqualTo(value));
        Assert.That(table[keyHash], Is.EqualTo(value));
    }

    [Test]
    public void SetValue_ByKeyName_ShouldStoreValue()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        const string key = "MYKEY";
        const string value = "Test Value";

        table.SetValue(key, value);

        Assert.That(table.Count, Is.EqualTo(1));
        Assert.That(table.GetValue(key), Is.EqualTo(value));
        Assert.That(table[key], Is.EqualTo(value));
    }

    [Test]
    public void SetValue_ByKeyName_ShouldUseCrc32Hash()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        const string key = "MYKEY";
        const string value = "Test Value";

        table.SetValue(key, value);
        var expectedHash = Crc32.Compute(key);

        Assert.That(table.GetValue(expectedHash), Is.EqualTo(value));
    }

    [Test]
    public void SetValue_OverwriteExisting_ShouldUpdateValue()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        const string key = "MYKEY";

        table.SetValue(key, "Original");
        table.SetValue(key, "Updated");

        Assert.That(table.Count, Is.EqualTo(1));
        Assert.That(table[key], Is.EqualTo("Updated"));
    }

    [Test]
    public void TryGetValue_ExistingKey_ShouldReturnTrue()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("KEY", "Value");

        var result = table.TryGetValue("KEY", out var value);

        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo("Value"));
    }

    [Test]
    public void TryGetValue_NonExistingKey_ShouldReturnFalse()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);

        var result = table.TryGetValue("NONEXISTENT", out var value);

        Assert.That(result, Is.False);
        Assert.That(value, Is.Null);
    }

    [Test]
    public void GetValue_NonExistingKey_ShouldReturnNull()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);

        Assert.That(table.GetValue("NONEXISTENT"), Is.Null);
        Assert.That(table["NONEXISTENT"], Is.Null);
    }

    [Test]
    public void ContainsKey_ExistingKey_ShouldReturnTrue()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("MYKEY", "Value");

        Assert.That(table.ContainsKey("MYKEY"), Is.True);
        Assert.That(table.ContainsKey(Crc32.Compute("MYKEY")), Is.True);
    }

    [Test]
    public void ContainsKey_NonExistingKey_ShouldReturnFalse()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);

        Assert.That(table.ContainsKey("NONEXISTENT"), Is.False);
    }

    [Test]
    public void Remove_ExistingKey_ShouldRemoveAndReturnTrue()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("MYKEY", "Value");

        var result = table.Remove("MYKEY");

        Assert.That(result, Is.True);
        Assert.That(table.Count, Is.EqualTo(0));
        Assert.That(table.ContainsKey("MYKEY"), Is.False);
    }

    [Test]
    public void Remove_NonExistingKey_ShouldReturnFalse()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);

        var result = table.Remove("NONEXISTENT");

        Assert.That(result, Is.False);
    }

    [Test]
    public void Clear_ShouldRemoveAllEntries()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("KEY1", "Value1");
        table.SetValue("KEY2", "Value2");
        table.SetValue("KEY3", "Value3");

        table.Clear();

        Assert.That(table.Count, Is.EqualTo(0));
    }

    [Test]
    public void KeyNames_WithKeepKeyNames_ShouldStoreOriginalNames()
    {
        var table = new GxtTable("TEST", Encoding.UTF8, keepKeyNames: true);
        table.SetValue("MYKEY", "Value");

        var keyName = table.GetKeyName(Crc32.Compute("MYKEY"));

        Assert.That(keyName, Is.EqualTo("MYKEY"));
    }

    [Test]
    public void KeyNames_WithoutKeepKeyNames_ShouldReturnNull()
    {
        var table = new GxtTable("TEST", Encoding.UTF8, keepKeyNames: false);
        table.SetValue("MYKEY", "Value");

        var keyName = table.GetKeyName(Crc32.Compute("MYKEY"));

        Assert.That(keyName, Is.Null);
    }

    [Test]
    public void Enumeration_ShouldYieldAllEntries()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("KEY1", "Value1");
        table.SetValue("KEY2", "Value2");
        table.SetValue("KEY3", "Value3");

        var entries = table.ToList();

        Assert.That(entries.Count, Is.EqualTo(3));
    }

    [Test]
    public void Keys_ShouldReturnAllKeyHashes()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("KEY1", "Value1");
        table.SetValue("KEY2", "Value2");

        var keys = table.Keys.ToList();

        Assert.That(keys.Count, Is.EqualTo(2));
        Assert.That(keys, Contains.Item(Crc32.Compute("KEY1")));
        Assert.That(keys, Contains.Item(Crc32.Compute("KEY2")));
    }

    [Test]
    public void Values_ShouldReturnAllValues()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table.SetValue("KEY1", "Value1");
        table.SetValue("KEY2", "Value2");

        var values = table.Values.ToList();

        Assert.That(values.Count, Is.EqualTo(2));
        Assert.That(values, Contains.Item("Value1"));
        Assert.That(values, Contains.Item("Value2"));
    }

    [Test]
    public void ToString_ShouldIncludeNameAndCount()
    {
        var table = new GxtTable("MAIN", Encoding.UTF8);
        table.SetValue("KEY1", "Value1");
        table.SetValue("KEY2", "Value2");

        var str = table.ToString();

        Assert.That(str, Does.Contain("MAIN"));
        Assert.That(str, Does.Contain("2"));
    }

    [Test]
    public void Indexer_SetNull_ShouldNotAddEntry()
    {
        var table = new GxtTable("TEST", Encoding.UTF8);
        table[0x12345678u] = null;

        Assert.That(table.Count, Is.EqualTo(0));
    }
}

