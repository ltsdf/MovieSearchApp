using MovieSearchApp.App.Models;
using MovieSearchApp.App.Services;
using MovieSearchApp.App.Services.SearchHistory;
using MovieSearchApp.App.State;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OmdbApp.Tests;
    
public class MovieSearchStateTests
{
    [Fact]
    public async Task Initialize_Loads_Recent_From_History()
    {
        var (state, historyMock, _) = Create();
        historyMock.Setup(h => h.GetAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<string> { "A", "B" });

        await state.InitializeAsync();
        Assert.Equal(new[] { "A", "B" }, state.Recent);
    }

    [Fact]
    public async Task Search_Populates_Results_And_Updates_History()
    {
        var (state, historyMock, clientMock) = Create();
        state.Query = "Batman";
        clientMock.Setup(c => c.SearchAsync("Batman", 1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PagedResult<MovieSearchItem> { Items = new List<MovieSearchItem> { new("Batman Begins", "2005", "tt0372784", "movie", "url") }, Page = 1, PageSize = 10, TotalCount = 1 });
        // Setup both AddAsync and GetAsync because mocks are strict
        historyMock.Setup(h => h.AddAsync("Batman", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        historyMock.Setup(h => h.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "Batman" });

        await state.SearchAsync();
        Assert.Single(state.Results);
        Assert.Equal("Batman Begins", state.Results[0].Title);
        historyMock.Verify(h => h.AddAsync("Batman", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(new[] { "Batman" }, state.Recent);
    }

    [Fact]
    public async Task SelectMovie_Loads_Details()
    {
        var (state, _, clientMock) = Create();
        var item = new MovieSearchItem("Batman Begins", "2005", "tt0372784", "movie", "url");
        clientMock.Setup(c => c.GetDetailsAsync("tt0372784", It.IsAny<CancellationToken>()))
        .ReturnsAsync(new MovieDetailsDto(
        Title: "Batman Begins",
        Year: "2005",
        Rated: "PG-13",
        Released: "",
        Runtime: "",
        Genre: "",
        Director: "",
        Writer: "",
        Actors: "",
        Plot: "",
        Language: "",
        Country: "",
        Awards: "",
        PosterUrl: "",
        ImdbRating: "",
        ImdbVotes: "",
        ImdbId: "tt0372784",
        Type: "movie",
        Metascore: "",
        Ratings: new List<RatingDto>()));

        await state.SelectMovieAsync(item);
        Assert.NotNull(state.SelectedDetails);
        Assert.Equal("tt0372784", state.SelectedDetails!.ImdbId);
    }

    private static (IMovieSearchState state, Mock<ISearchHistoryService> history, Mock<IOmdbClient> client) Create()
    {
        var history = new Mock<ISearchHistoryService>(MockBehavior.Strict);
        var client = new Mock<IOmdbClient>(MockBehavior.Strict);
        var state = new MovieSearchState(client.Object, history.Object);
        return (state, history, client);
    }
}
