using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OmdbApp.Tests;

public class SearchHistoryServiceTests
{
    [Fact]
    public async Task Adds_Unique_And_Trims_To_5()
    {
        var svc = new InMemorySearchHistoryService();
        foreach (var q in new[] { "A", "B", "C", "D", "E", "F" })
            await svc.AddAsync(q, CancellationToken.None);

        var items = await svc.GetAsync(CancellationToken.None);
        Assert.Equal(new[] { "F", "E", "D", "C", "B" }, items);
    }

    [Fact]
    public async Task Duplicate_Moves_To_Front()
    {
        var svc = new InMemorySearchHistoryService();
        await svc.AddAsync("Star", CancellationToken.None);
        await svc.AddAsync("Matrix", CancellationToken.None);
        await svc.AddAsync("Star", CancellationToken.None);

        var items = await svc.GetAsync(CancellationToken.None);
        Assert.Equal(new[] { "Star", "Matrix" }, items);
    }
}