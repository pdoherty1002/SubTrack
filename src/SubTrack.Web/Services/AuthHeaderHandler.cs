using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace SubTrack.Web.Services;

/// <summary>
/// Attaches the current JWT as a Bearer token to every outgoing request on the API's
/// typed <see cref="HttpClient"/>. A 401 response means the token is missing or expired
/// server-side too, so it's cleared and the user is sent back to the login page.
/// </summary>
public class AuthHeaderHandler(TokenStore tokenStore, NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(tokenStore.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.Token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await tokenStore.ClearAsync();
            navigation.NavigateTo("/login", forceLoad: false);
        }

        return response;
    }
}
