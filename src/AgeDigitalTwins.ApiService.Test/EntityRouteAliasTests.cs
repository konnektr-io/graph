using AgeDigitalTwins.ApiService.Routing;
using Microsoft.AspNetCore.Http;

namespace AgeDigitalTwins.ApiService.Test;

/// <summary>
/// Unit coverage for the /entities route alias. The alias must rewrite only the
/// <c>/entities</c> segment and must not touch anything else — a false positive here
/// would silently remap unrelated routes.
/// </summary>
public class EntityRouteAliasTests
{
    [Theory]
    [InlineData("/entities", "/digitaltwins")]
    [InlineData("/entities/", "/digitaltwins/")]
    [InlineData("/entities/crater1", "/digitaltwins/crater1")]
    [InlineData("/entities/crater1/relationships", "/digitaltwins/crater1/relationships")]
    [InlineData("/entities/crater1/telemetry", "/digitaltwins/crater1/telemetry")]
    [InlineData("/entities/crater1/components/thermostat", "/digitaltwins/crater1/components/thermostat")]
    [InlineData("/ENTITIES/crater1", "/digitaltwins/crater1")]
    public void Rewrites_alias_prefix_onto_canonical_prefix(string requestPath, string expected)
    {
        bool rewritten = EntityRouteAlias.TryRewrite(
            new PathString(requestPath),
            out PathString result
        );

        Assert.True(rewritten);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("/digitaltwins/crater1")]
    [InlineData("/models")]
    [InlineData("/query")]
    [InlineData("/relationships")]
    [InlineData("/entitiesfoo")]
    [InlineData("/entity")]
    [InlineData("/")]
    public void Leaves_non_alias_paths_untouched(string requestPath)
    {
        bool rewritten = EntityRouteAlias.TryRewrite(
            new PathString(requestPath),
            out PathString result
        );

        Assert.False(rewritten);
        Assert.Equal(requestPath, result.Value);
    }
}
