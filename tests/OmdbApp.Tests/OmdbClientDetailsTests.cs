using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MovieSearchApp.App.Options;
using MovieSearchApp.App.Services;
using Xunit;

namespace OmdbApp.Tests;

public class OmdbClientDetailsTests
{
    [Fact]
    public async Task GetDetails_Returns_Dto_When_Response_True()
    {
        var handler = new StubHandler(req =>
        {
            var uri = req.RequestUri!.ToString();
            Assert.Contains("apikey=TEST_KEY", uri);
            Assert.Contains("&i=tt0372784", uri);
            var payload = new
            {
                Title = "Batman Begins",
                Year = "2005",
                Rated = "PG-13",
                Released = "15 Jun 2005",
                Runtime = "140 min",
                Genre = "Action, Crime",
                Director = "Christopher Nolan",
                Writer = "Nolan",
                Actors = "Bale, Caine",
                Plot = "Origin story.",
                Language = "English",
                Country = "USA",
                Awards = "Nominated",
                Poster = "https://example/poster.jpg",
                Ratings = new[] { new { Source = "Internet", Value = "8/10" } },
                Metascore = "70",
                imdbRating = "8.2",
                imdbVotes = "1000000",
                imdbID = "tt0372784",
                Type = "movie",
                Response = "True"
            };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload))
            };
        });
        var http = new HttpClient(handler);
        var client = new OmdbClient(http, new OmdbOptions { ApiKey = "TEST_KEY", BaseUrl = "https://example" });
        var dto = await client.GetDetailsAsync("tt0372784", CancellationToken.None);
        Assert.NotNull(dto);
        Assert.Equal("Batman Begins", dto!.Title);
        Assert.Equal("tt0372784", dto.ImdbId);
        Assert.Single(dto.Ratings);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}
