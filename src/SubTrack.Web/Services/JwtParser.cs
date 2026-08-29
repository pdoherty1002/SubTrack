using System.Security.Claims;
using System.Text.Json;

namespace SubTrack.Web.Services;

/// <summary>
/// Reads the claims out of a JWT's payload on the client, purely to build a
/// <see cref="ClaimsPrincipal"/> for Blazor's authorization system. This never
/// validates the signature — the API is the only party that needs to trust the token.
/// </summary>
public static class JwtParser
{
    public static IReadOnlyList<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
            return [];

        var payloadJson = Base64UrlDecode(parts[1]);
        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson)
            ?? [];

        return payload.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())).ToList();
    }

    public static DateTimeOffset? GetExpiry(IReadOnlyList<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        return long.TryParse(exp, out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
    }

    private static string Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
