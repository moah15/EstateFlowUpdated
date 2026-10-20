using System.Net.Http;
using System.Net.Http.Headers;

namespace EstateFlow.Desktop.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IAuthState _authState;

    public AuthTokenHandler(IAuthState authState)
    {
        _authState = authState;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_authState.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

        return base.SendAsync(request, cancellationToken);
    }
}