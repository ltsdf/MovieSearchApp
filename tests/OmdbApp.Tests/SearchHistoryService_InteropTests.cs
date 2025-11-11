using Microsoft.JSInterop;
using MovieSearchApp.App.Services.SearchHistory;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OmdbApp.Tests;

public class SearchHistoryService_InteropTests
{
    [Fact]
    public async Task Add_Get_Clear_Roundtrip_Uses_LocalStorage_And_Maintains_Semantics()
    {
        var js = new Mock<IJSRuntime>(MockBehavior.Strict);
        string? storageJson = null;

        // getItem
        js.Setup(j => j.InvokeAsync<string?>(
                It.Is<string>(id => id == "localStorage.getItem"),
                It.IsAny<CancellationToken>(),
                It.IsAny<object?[]>()))
          .Returns(() => new ValueTask<string?>(storageJson));

        // setItem -> set up underlying InvokeAsync<object?> because InvokeVoidAsync is an extension method
        js.Setup(j => j.InvokeAsync<object?>(
                It.Is<string>(id => id == "localStorage.setItem"),
                It.IsAny<CancellationToken>(),
                It.IsAny<object?[]>()))
          .Callback<string, CancellationToken, object?[]>((_, __, args) =>
          {
              // args[0] = key, args[1] = json
              storageJson = args.Length >= 2 ? args[1] as string : null;
          })
          .Returns(new ValueTask<object?>(result: null));

        // removeItem -> set up underlying InvokeAsync<object?>
        js.Setup(j => j.InvokeAsync<object?>(
                It.Is<string>(id => id == "localStorage.removeItem"),
                It.IsAny<CancellationToken>(),
                It.IsAny<object?[]>()))
          .Callback<string, CancellationToken, object?[]>((_, __, __args) => storageJson = null)
          .Returns(new ValueTask<object?>(result: null));

        var svc = new SearchHistoryService(js.Object);

        await svc.AddAsync("A");
        await svc.AddAsync("B");
        var items = await svc.GetAsync();
        Assert.Equal(new[] { "B", "A" }, items);

        await svc.ClearAsync();
        var cleared = await svc.GetAsync();
        Assert.Empty(cleared);
    }
}