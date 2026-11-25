using SecsGemLab.Gem;

namespace SecsGemLab.Tests;

public class RecipeStoreTests
{
    private static readonly byte[] Body = [1, 2, 3, 4];

    private static RecipeStore Create() => new(maxTotalBytes: 100, maxRecipeBytes: 50);

    [Fact]
    public void StoreThenGet_ReturnsACopy()
    {
        var store = Create();

        Assert.Equal(ProcessProgramAck.Accepted, store.Store("RCP1", Body));

        var first = store.Get("RCP1");
        Assert.NotNull(first);
        Assert.Equal(Body, first);
        first[0] = 99;
        Assert.Equal(Body, store.Get("RCP1"));
    }

    [Fact]
    public void Inquire_GrantsAnewRecipe_AndRefusesBadOnes()
    {
        var store = Create();
        _ = store.Store("RCP1", Body);

        Assert.Equal(ProcessProgramGrant.Ok, store.Inquire("RCP2", 10));
        Assert.Equal(ProcessProgramGrant.AlreadyHave, store.Inquire("RCP1", 10));
        Assert.Equal(ProcessProgramGrant.InvalidPpid, store.Inquire("", 10));
        Assert.Equal(ProcessProgramGrant.InvalidPpid, store.Inquire(new string('A', RecipeStore.MaxPpidLength + 1), 10));
        Assert.Equal(ProcessProgramGrant.NoSpace, store.Inquire("RCP2", 51));
        Assert.Equal(ProcessProgramGrant.NoSpace, store.Inquire("RCP2", 0));
    }

    [Fact]
    public void Inquire_ChecksTheTotalCapacity()
    {
        var store = Create();
        _ = store.Store("A", new byte[50]);
        _ = store.Store("B", new byte[40]);

        Assert.Equal(ProcessProgramGrant.NoSpace, store.Inquire("C", 11));
        Assert.Equal(ProcessProgramGrant.Ok, store.Inquire("C", 10));
    }

    [Fact]
    public void Inquire_ReportsBusy()
    {
        var store = Create();
        store.IsBusy = true;

        Assert.Equal(ProcessProgramGrant.Busy, store.Inquire("RCP1", 10));
        Assert.Equal(ProcessProgramAck.PermissionNotGranted, store.Store("RCP1", Body));
    }

    [Fact]
    public void Store_ChecksLengthAndCapacity()
    {
        var store = Create();

        Assert.Equal(ProcessProgramAck.LengthError, store.Store("A", []));
        Assert.Equal(ProcessProgramAck.LengthError, store.Store("A", new byte[51]));

        _ = store.Store("A", new byte[50]);
        _ = store.Store("B", new byte[50]);
        Assert.Equal(ProcessProgramAck.MatrixOverflow, store.Store("C", new byte[1]));
    }

    [Fact]
    public void Store_ReplacesAnExistingRecipe_AndAdjustsTheTotal()
    {
        var store = Create();
        _ = store.Store("A", new byte[40]);

        Assert.Equal(ProcessProgramAck.Accepted, store.Store("A", new byte[10]));

        Assert.Equal(10, store.TotalBytes);
        Assert.Equal(new[] { "A" }, store.ListIds().ToArray());
    }

    [Fact]
    public void Delete_RemovesNamedRecipes()
    {
        var store = Create();
        _ = store.Store("A", Body);
        _ = store.Store("B", Body);

        Assert.Equal(ProcessProgramAck.Accepted, store.Delete(["A"]));

        Assert.Null(store.Get("A"));
        Assert.Equal(new[] { "B" }, store.ListIds().ToArray());
        Assert.Equal(4, store.TotalBytes);
    }

    [Fact]
    public void Delete_IsAllOrNothing_WhenOneIsMissing()
    {
        var store = Create();
        _ = store.Store("A", Body);

        Assert.Equal(ProcessProgramAck.PpidNotFound, store.Delete(["A", "MISSING"]));

        Assert.NotNull(store.Get("A"));
    }

    [Fact]
    public void Delete_WithEmptyList_RemovesEverything()
    {
        var store = Create();
        _ = store.Store("A", Body);
        _ = store.Store("B", Body);

        Assert.Equal(ProcessProgramAck.Accepted, store.Delete([]));

        Assert.Empty(store.ListIds());
        Assert.Equal(0, store.TotalBytes);
    }

    [Fact]
    public void Delete_WithDuplicatePpids_DoesNotDoubleCount()
    {
        var store = Create();
        _ = store.Store("A", Body);

        Assert.Equal(ProcessProgramAck.Accepted, store.Delete(["A", "A"]));

        Assert.Equal(0, store.TotalBytes);
    }

    [Fact]
    public void ListIds_IsSorted()
    {
        var store = Create();
        _ = store.Store("B", Body);
        _ = store.Store("A", Body);

        Assert.Equal(new[] { "A", "B" }, store.ListIds().ToArray());
    }
}
