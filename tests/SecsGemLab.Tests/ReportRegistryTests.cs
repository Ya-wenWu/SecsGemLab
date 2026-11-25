using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class ReportRegistryTests
{
    private static ReportRegistry Create(int maxReports = 256) =>
        new([1u, 2u, 3u], [100u, 200u], maxReports);

    private static ReportDefinition Report(uint id, params uint[] vids) => new(id, vids);

    private static EventLink Link(uint eventId, params uint[] reportIds) => new(eventId, reportIds);

    [Fact]
    public void DefineLinkEnable_ProducesTheReportsForAnEvent()
    {
        var registry = Create();

        Assert.Equal(DefineReportAck.Accepted, registry.Define([Report(10, 1, 2), Report(11, 3)]));
        Assert.Equal(LinkReportAck.Accepted, registry.Link([Link(100, 10, 11)]));
        Assert.Equal(EnableEventAck.Accepted, registry.SetEnabled(true, [100]));

        var reports = registry.GetReportsFor(100);

        Assert.Equal(2, reports.Count);
        Assert.Equal(10u, reports[0].ReportId);
        Assert.Equal(new uint[] { 1, 2 }, reports[0].VariableIds.ToArray());
        Assert.Equal(11u, reports[1].ReportId);
    }

    [Fact]
    public void DisabledOrUnlinkedEvents_CarryNoReports()
    {
        var registry = Create();
        _ = registry.Define([Report(10, 1)]);
        _ = registry.Link([Link(100, 10)]);

        Assert.Empty(registry.GetReportsFor(100));

        _ = registry.SetEnabled(true, [200]);
        Assert.Empty(registry.GetReportsFor(200));
    }

    [Fact]
    public void Define_RejectsUnknownVariable_AndChangesNothing()
    {
        var registry = Create();

        var ack = registry.Define([Report(10, 1), Report(11, 99)]);

        Assert.Equal(DefineReportAck.VariableNotFound, ack);
        Assert.Equal(0, registry.ReportCount);
    }

    [Fact]
    public void Define_RejectsRedefinitionWithoutDelete()
    {
        var registry = Create();
        _ = registry.Define([Report(10, 1)]);

        Assert.Equal(DefineReportAck.AlreadyDefined, registry.Define([Report(10, 2)]));
    }

    [Fact]
    public void Define_RejectsDuplicateIdsInOneRequest()
    {
        var registry = Create();

        Assert.Equal(DefineReportAck.InvalidFormat, registry.Define([Report(10, 1), Report(10, 2)]));
    }

    [Fact]
    public void Define_EnforcesTheReportLimit()
    {
        var registry = Create(maxReports: 1);

        Assert.Equal(DefineReportAck.OutOfSpace, registry.Define([Report(10, 1), Report(11, 2)]));
        Assert.Equal(0, registry.ReportCount);
    }

    [Fact]
    public void Define_WithEmptyList_ClearsReportsAndLinks()
    {
        var registry = Create();
        _ = registry.Define([Report(10, 1)]);
        _ = registry.Link([Link(100, 10)]);
        _ = registry.SetEnabled(true, [100]);

        Assert.Equal(DefineReportAck.Accepted, registry.Define([]));

        Assert.Equal(0, registry.ReportCount);
        Assert.Empty(registry.GetReportsFor(100));
    }

    [Fact]
    public void Define_WithEmptyVariableList_DeletesThatReportAndItsLinks()
    {
        var registry = Create();
        _ = registry.Define([Report(10, 1), Report(11, 2)]);
        _ = registry.Link([Link(100, 10, 11)]);
        _ = registry.SetEnabled(true, [100]);

        _ = registry.Define([Report(10)]);

        var reports = registry.GetReportsFor(100);
        Assert.Single(reports);
        Assert.Equal(11u, reports[0].ReportId);
    }

    [Fact]
    public void Link_Rejects_UnknownEvent_UnknownReport_AndRelinkWithoutUnlink()
    {
        var registry = Create();
        _ = registry.Define([Report(10, 1)]);

        Assert.Equal(LinkReportAck.EventNotFound, registry.Link([Link(999, 10)]));
        Assert.Equal(LinkReportAck.ReportNotFound, registry.Link([Link(100, 77)]));
        Assert.Equal(LinkReportAck.InvalidFormat, registry.Link([Link(100, 10, 10)]));

        Assert.Equal(LinkReportAck.Accepted, registry.Link([Link(100, 10)]));
        Assert.Equal(LinkReportAck.AlreadyDefined, registry.Link([Link(100, 10)]));
    }

    [Fact]
    public void Link_WithEmptyReportList_Unlinks()
    {
        var registry = Create();
        _ = registry.Define([Report(10, 1)]);
        _ = registry.Link([Link(100, 10)]);
        _ = registry.SetEnabled(true, [100]);

        Assert.Equal(LinkReportAck.Accepted, registry.Link([Link(100)]));

        Assert.Empty(registry.GetReportsFor(100));
    }

    [Fact]
    public void SetEnabled_WithEmptyList_AppliesToEveryEvent()
    {
        var registry = Create();

        _ = registry.SetEnabled(true, []);
        Assert.True(registry.IsEnabled(100));
        Assert.True(registry.IsEnabled(200));

        _ = registry.SetEnabled(false, []);
        Assert.False(registry.IsEnabled(100));
    }

    [Fact]
    public void SetEnabled_RejectsUnknownEvent_AndChangesNothing()
    {
        var registry = Create();

        Assert.Equal(EnableEventAck.EventNotFound, registry.SetEnabled(true, [100, 999]));
        Assert.False(registry.IsEnabled(100));
    }
}
