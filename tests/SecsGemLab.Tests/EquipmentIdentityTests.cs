using SecsGemLab.Handshake;

namespace SecsGemLab.Tests;

public class EquipmentIdentityTests
{
    [Fact]
    public void Constructor_AcceptsValuesAtTheMaximumLength()
    {
        var value = new string('A', EquipmentIdentity.MaxLength);

        var identity = new EquipmentIdentity(value, value);

        Assert.Equal(value, identity.Mdln);
        Assert.Equal(value, identity.SoftRev);
    }

    [Fact]
    public void Constructor_RejectsValuesLongerThanTheMaximum()
    {
        var tooLong = new string('A', EquipmentIdentity.MaxLength + 1);

        Assert.ThrowsAny<ArgumentException>(() => _ = new EquipmentIdentity(tooLong, "1.0"));
        Assert.ThrowsAny<ArgumentException>(() => _ = new EquipmentIdentity("MDLN", tooLong));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankValues(string blank)
    {
        Assert.ThrowsAny<ArgumentException>(() => _ = new EquipmentIdentity(blank, "1.0"));
        Assert.ThrowsAny<ArgumentException>(() => _ = new EquipmentIdentity("MDLN", blank));
    }

    [Fact]
    public void Constructor_RejectsNonAsciiValues()
    {
        Assert.ThrowsAny<ArgumentException>(() => _ = new EquipmentIdentity("設備", "1.0"));
    }

    [Fact]
    public void Identities_WithSameValues_AreEqual()
    {
        Assert.Equal(new EquipmentIdentity("MDLN", "1.0"), new EquipmentIdentity("MDLN", "1.0"));
        Assert.NotEqual(new EquipmentIdentity("MDLN", "1.0"), new EquipmentIdentity("MDLN", "2.0"));
    }
}
