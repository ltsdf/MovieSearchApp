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

public class OmdbClientTests
{
    [Fact]
    public async Task Search_Builds_Correct_Url_And_Maps_Items()
    {
        var handler = new StubHandler(req =>
        {
            var uri = req.RequestUri!.ToString();
            Assert.Contains("apikey=TEST_KEY", uri);
            Assert.Contains("&s=Batman", uri);
            Assert.Contains("&page=1", uri);
            var payload = new
            {
                Search = new[]
                {
                    new { Title = "Batman Begins", Year = "2005", imdbID = "tt0372784", Type = "movie", Poster = "https://example/poster.jpg" }
                },
                totalResults = "1",
                Response = "True"
            };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload))
            };
        });

        var http = new HttpClient(handler);
        var client = new OmdbClient(http, new OmdbOptions { ApiKey = "TEST_KEY", BaseUrl = "https://example" });

        var paged = await client.SearchAsync("Batman", 1, CancellationToken.None);
        Assert.Single(paged.Items);
        Assert.Equal("Batman Begins", paged.Items[0].Title);
        Assert.Equal("tt0372784", paged.Items[0].ImdbId);
        Assert.Equal("https://example/poster.jpg", paged.Items[0].PosterUrl);
        Assert.Equal(1, paged.TotalCount);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}