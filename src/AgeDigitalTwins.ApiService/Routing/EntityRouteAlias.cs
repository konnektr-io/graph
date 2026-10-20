namespace AgeDigitalTwins.ApiService.Routing;

/// <summary>
/// Vocabulary-neutral alias for the Azure Digital Twins profile.
/// <para>
/// <c>/entities/*</c> is served by the exact same handlers as <c>/digitaltwins/*</c>.
/// This is deliberately implemented as a path rewrite rather than a second endpoint
/// table: a duplicated table drifts the moment someone adds an endpoint to only one
/// side, whereas a rewrite cannot. Request bodies, response bodies, status codes,
/// auth policies, rate limits and caching are all the canonical pipeline's.
/// </para>
/// <para>
/// The ADT profile (<c>/digitaltwins/*</c>, <c>FROM DIGITALTWINS</c>, <c>$dtId</c>,
/// <c>$metadata.$model</c>) is unchanged and remains the compatibility contract.
/// </para>
/// </summary>
public static class EntityRouteAlias
{
    /// <summary>Canonical ADT-compatible route prefix.</summary>
    public const string CanonicalPrefix = "/digitaltwins";

    /// <summary>Vocabulary-neutral alias prefix.</summary>
    public const string AliasPrefix = "/entities";

    /// <summary>
    /// Rewrites <c>/entities</c> and <c>/entities/…</c> onto <see cref="CanonicalPrefix"/>.
    /// </summary>
    /// <param name="path">Incoming request path.</param>
    /// <param name="rewritten">Rewritten path (unchanged when the alias does not match).</param>
    /// <returns><c>true</c> when the path was rewritten.</returns>
    public static bool TryRewrite(PathString path, out PathString rewritten)
    {
        if (
            path.StartsWithSegments(
                AliasPrefix,
                StringComparison.OrdinalIgnoreCase,
                out PathString remainder
            )
        )
        {
            rewritten = new PathString(CanonicalPrefix + remainder.Value);
            return true;
        }

        rewritten = path;
        return false;
    }
}
