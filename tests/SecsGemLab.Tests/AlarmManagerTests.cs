using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class AlarmManagerTests
{
    private static AlarmManager Create()
    {
        var manager = new AlarmManager();
        manager.Register(new AlarmDefinition(1, "Over temperature", 5));
        manager.Register(new AlarmDefinition(2, "Door open", 2), enabled: false);
        return manager;
    }

    [Fact]
    public void Set_ReportsWithTheSetBit()
    {
        var manager = Create();

        var report = manager.Set(1);

        Assert.NotNull(report);
        Assert.Equal((byte)(5 | 0x80), report.AlarmCode);
        Assert.Equal(1u, report.Id);
        Assert.Equal("Over temperature", report.Text);
        Assert.True(manager.IsSet(1));
    }

    [Fact]
    public void Clear_ReportsWithoutTheSetBit()
    {
        var manager = Create();
        _ = manager.Set(1);

        var report = manager.Clear(1);

        Assert.NotNull(report);
        Assert.Equal((byte)5, report.AlarmCode);
        Assert.False(manager.IsSet(1));
    }

    [Fact]
    public void RepeatedSetOrClear_ReportsNothing()
    {
        var manager = Create();

        Assert.Null(manager.Clear(1));
        _ = manager.Set(1);
        Assert.Null(manager.Set(1));
    }

    [Fact]
    public void DisabledAlarm_TracksStateButReportsNothing()
    {
        var manager = Create();

        Assert.Null(manager.Set(2));
        Assert.True(manager.IsSet(2));
    }

    [Fact]
    public void SetEnabled_TogglesReporting_AndRejectsUnknownAlid()
    {
        var manager = Create();

        Assert.Equal(AlarmManager.Accepted, manager.SetEnabled(2, enabled: true));
        Assert.NotNull(manager.Set(2));

        Assert.Equal(AlarmManager.Error, manager.SetEnabled(99, enabled: true));
    }

    [Fact]
    public void Lists_ShowAllAndEnabledAlarms()
    {
        var manager = Create();
        _ = manager.Set(1);

        var all = manager.ListAll();
        var enabled = manager.ListEnabled();

        Assert.Equal(2, all.Count);
        Assert.True(all[0].IsSet);
        Assert.False(all[1].IsEnabled);
        Assert.Single(enabled);
        Assert.Equal(1u, enabled[0].Definition.Id);
    }

    [Fact]
    public void UnknownAlarm_ThrowsOnSetAndClear()
    {
        var manager = Create();

        Assert.Throws<KeyNotFoundException>(() => manager.Set(99));
        Assert.Throws<KeyNotFoundException>(() => manager.Clear(99));
    }

    [Fact]
    public void Register_RejectsDuplicatesAndOversizedCategory()
    {
        var manager = Create();

        Assert.Throws<ArgumentException>(() => manager.Register(new AlarmDefinition(1, "Dup", 1)));
        Assert.Throws<ArgumentException>(() => manager.Register(new AlarmDefinition(3, "Big", 0x80)));
    }
}
