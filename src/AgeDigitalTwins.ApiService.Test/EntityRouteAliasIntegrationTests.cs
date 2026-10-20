using System.Net;
using System.Text;
using System.Text.Json;
using Aspire.Hosting;

namespace AgeDigitalTwins.ApiService.Test;

/// <summary>
/// End-to-end proof that the /entities alias is served by the same handlers as
/// /digitaltwins: identical bodies, identical status codes, and writes through the
/// alias are visible on the canonical path (they share one store, not two).
/// </summary>
[Trait("Category", "Integration")]
public class EntityRouteAliasIntegrationTests : IAsyncLifetime
{
    private TestingAspireAppHost? _app;
    private HttpClient? _httpClient;

    public async Task InitializeAsync()
    {
        _app = new TestingAspireAppHost();
        await _app.StartAsync();
        _httpClient = _app.CreateHttpClient("apiservice");

        string[] sModels =
        [
            SampleData.DtdlRoom,
            SampleData.DtdlTemperatureSensor,
            SampleData.DtdlCelestialBody,
            SampleData.DtdlPlanet,
            SampleData.DtdlCrater,
        ];
        List<JsonElement> jModels = sModels
            .Select(m => JsonDocument.Parse(m))
            .Select(j => j.RootElement)
            .ToList();

        var modelResponse = await _httpClient.PostAsync(
            "/models",
            new StringContent(JsonSerializer.Serialize(jModels), Encoding.UTF8, "application/json")
        );
        string modelResponseContent = await modelResponse.Content.ReadAsStringAsync();
        Assert.True(
            modelResponse.IsSuccessStatusCode,
            $"model import failed: {modelResponse.StatusCode} {modelResponseContent}"
        );
    }

    public async Task DisposeAsync()
    {
        if (_httpClient != null)
        {
            await _httpClient.DeleteAsync("/graph/delete");
        }

        _httpClient?.Dispose();
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task Get_through_alias_returns_identical_body_and_status()
    {
        var create = await _httpClient!.PutAsync(
            "/digitaltwins/crater1",
            new StringContent(SampleData.TwinCrater, Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var canonical = await _httpClient.GetAsync("/digitaltwins/crater1");
        var aliased = await _httpClient.GetAsync("/entities/crater1");

        Assert.Equal(HttpStatusCode.OK, aliased.StatusCode);
        Assert.Equal(canonical.StatusCode, aliased.StatusCode);
        Assert.Equal(
            await canonical.Content.ReadAsStringAsync(),
            await aliased.Content.ReadAsStringAsync()
        );
    }

    [Fact]
    public async Task Write_through_alias_is_visible_on_canonical_path()
    {
        var viaAlias = await _httpClient!.PutAsync(
            "/entities/crater1",
            new StringContent(SampleData.TwinCrater, Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.OK, viaAlias.StatusCode);

        var canonical = await _httpClient.GetAsync("/digitaltwins/crater1");
        Assert.Equal(HttpStatusCode.OK, canonical.StatusCode);

        JsonDocument twin = JsonDocument.Parse(await canonical.Content.ReadAsStringAsync());
        Assert.Equal("crater1", twin.RootElement.GetProperty("$dtId").GetString());
    }

    [Fact]
    public async Task Unknown_twin_returns_the_same_response_on_both_paths()
    {
        var canonical = await _httpClient!.GetAsync("/digitaltwins/does-not-exist");
        var aliased = await _httpClient.GetAsync("/entities/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, aliased.StatusCode);
        Assert.Equal(canonical.StatusCode, aliased.StatusCode);
        Assert.Equal(
            await canonical.Content.ReadAsStringAsync(),
            await aliased.Content.ReadAsStringAsync()
        );
    }
}
