using System.Net;
using System.Net.Http.Json;
using EzItam.Web.Api;

namespace EzItam.Tests;

public sealed class WebApiClientTests
{
    [Fact]
    public async Task GetAssetByTag_surfaces_not_found_status_and_api_error_message()
    {
        using var http = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent.Create(new ApiError { Message = "자산을 찾을 수 없습니다." })
        });
        var client = new ItamApiClient(http);

        var exception = await Assert.ThrowsAsync<ApiException>(() => client.GetAssetByTagAsync("missing"));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("자산을 찾을 수 없습니다.", exception.Message);
    }

    [Fact]
    public async Task UpdateInventory_accepts_no_content_success_response()
    {
        using var http = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new ItamApiClient(http);

        await client.UpdateInventoryAsync(12, new InventoryDto { Id = 12 });
    }

    [Fact]
    public async Task CreateInventory_deserializes_created_resource_response()
    {
        using var http = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new InventoryDto { Id = 12, Name = "연간 자산 조사" })
        });
        var client = new ItamApiClient(http);

        var created = await client.CreateInventoryAsync(new InventoryDto { Name = "연간 자산 조사" });

        Assert.Equal(12, created.Id);
        Assert.Equal("연간 자산 조사", created.Name);
    }

    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
        => new(new StubHandler(respond)) { BaseAddress = new Uri("http://localhost/") };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }
}
