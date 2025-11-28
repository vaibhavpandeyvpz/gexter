namespace Gexter.Tests;

[TestFixture]
public class Crc32Tests
{
    [Test]
    public void Compute_EmptyString_ShouldReturnZero()
    {
        Assert.That(Crc32.Compute(""), Is.EqualTo(0u));
        Assert.That(Crc32.Compute((string)null!), Is.EqualTo(0u));
    }

    [Test]
    public void Compute_SameInputs_ShouldReturnSameHash()
    {
        var hash1 = Crc32.Compute("MAIN");
        var hash2 = Crc32.Compute("MAIN");

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void Compute_CaseInsensitive_ShouldReturnSameHash()
    {
        var hashUpper = Crc32.Compute("MAIN");
        var hashLower = Crc32.Compute("main");
        var hashMixed = Crc32.Compute("MaIn");

        Assert.That(hashUpper, Is.EqualTo(hashLower));
        Assert.That(hashUpper, Is.EqualTo(hashMixed));
    }

    [Test]
    public void Compute_DifferentInputs_ShouldReturnDifferentHashes()
    {
        var hash1 = Crc32.Compute("MAIN");
        var hash2 = Crc32.Compute("TEST");

        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void Compute_KnownGxtKeys_ShouldProduceConsistentHashes()
    {
        // These are common GXT key names used in GTA games
        var keys = new[] { "MAIN", "INTRO", "MISGEN", "OFR1", "START", "HELP" };

        foreach (var key in keys)
        {
            var hash = Crc32.Compute(key);
            TestContext.Out.WriteLine($"CRC32(\"{key}\") = 0x{hash:X8}");
            Assert.That(hash, Is.Not.EqualTo(0u), $"Hash for '{key}' should not be zero");
        }
    }

    [Test]
    public void ComputeBytes_ShouldWorkCorrectly()
    {
        var data = new byte[] { 0x4D, 0x41, 0x49, 0x4E }; // "MAIN" in ASCII
        var hash = Crc32.Compute(data);

        Assert.That(hash, Is.Not.EqualTo(0u));
    }

    [Test]
    public void Compute_ConsistencyCheck()
    {
        // Test that repeated computations give consistent results
        const string testKey = "MISGEN";

        var hashes = new uint[100];
        for (int i = 0; i < 100; i++)
        {
            hashes[i] = Crc32.Compute(testKey);
        }

        Assert.That(hashes.Distinct().Count(), Is.EqualTo(1),
            "All hash computations should return the same value");
    }
}

