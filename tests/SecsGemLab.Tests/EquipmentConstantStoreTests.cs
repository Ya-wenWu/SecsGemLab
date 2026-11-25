using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class EquipmentConstantStoreTests
{
    private static EquipmentConstantStore Create()
    {
        var store = new EquipmentConstantStore();
        store.Register(new EquipmentConstant(1, "Temperature", "C", 0, 300, 25));
        store.Register(new EquipmentConstant(2, "Pressure", "Pa", 0, 1000, 100));
        return store;
    }

    private static KeyValuePair<uint, double> Change(uint id, double value) => new(id, value);

    [Fact]
    public void Get_ReturnsDefaults_AndNullForUnknown()
    {
        var store = Create();

        Assert.Equal(25.0, store.Get(1));
        Assert.Null(store.Get(99));
    }

    [Fact]
    public void Set_AppliesAllChanges()
    {
        var store = Create();

        var ack = store.Set([Change(1, 150), Change(2, 500)]);

        Assert.Equal(EquipmentConstantAck.Accepted, ack);
        Assert.Equal(150.0, store.Get(1));
        Assert.Equal(500.0, store.Get(2));
    }

    [Fact]
    public void Set_IsAllOrNothing_WhenOneIsOutOfRange()
    {
        var store = Create();

        var ack = store.Set([Change(1, 150), Change(2, 5000)]);

        Assert.Equal(EquipmentConstantAck.OutOfRange, ack);
        Assert.Equal(25.0, store.Get(1));
    }

    [Fact]
    public void Set_IsAllOrNothing_WhenOneIsUnknown()
    {
        var store = Create();

        Assert.Equal(EquipmentConstantAck.NotFound, store.Set([Change(1, 150), Change(99, 1)]));
        Assert.Equal(25.0, store.Get(1));
    }

    [Fact]
    public void Set_RejectsNaN()
    {
        var store = Create();

        Assert.Equal(EquipmentConstantAck.OutOfRange, store.Set([Change(1, double.NaN)]));
    }

    [Fact]
    public void Set_IsRefusedWhileBusy()
    {
        var store = Create();
        store.IsBusy = true;

        Assert.Equal(EquipmentConstantAck.Busy, store.Set([Change(1, 150)]));
        Assert.Equal(25.0, store.Get(1));
    }

    [Fact]
    public void Register_RejectsDuplicatesAndBadDefaults()
    {
        var store = Create();

        Assert.Throws<ArgumentException>(() => store.Register(new EquipmentConstant(1, "Dup", "C", 0, 1, 0)));
        Assert.Throws<ArgumentException>(() => store.Register(new EquipmentConstant(3, "Bad", "C", 0, 1, 5)));
        Assert.Throws<ArgumentException>(() => store.Register(new EquipmentConstant(4, "Bad", "C", 5, 1, 3)));
    }

    [Fact]
    public void Definitions_AreOrderedByEcid()
    {
        var store = new EquipmentConstantStore();
        store.Register(new EquipmentConstant(9, "B", "", 0, 1, 0));
        store.Register(new EquipmentConstant(2, "A", "", 0, 1, 0));

        Assert.Equal(new uint[] { 2, 9 }, store.Definitions.Select(d => d.Id).ToArray());
    }
}
