using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MinimumApiExample.Tests;

internal sealed class TestApplication : WebApplicationFactory<Program>
{
    public HttpClient CreateAuthorizedClient(string id)
    {
        return CreateDefaultClient(new AddAuthenticationHandler(request => AttachToken(id, request)));
    }

    private void AttachToken(string id, HttpRequestMessage request)
    {
        var token = CreateToken(id);
        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
    }

    private string CreateToken(string id)
    {
        var configuration = Services.GetRequiredService<IConfiguration>();
        var bearerSection = configuration.GetSection("Authentication:Schemes:Bearer");
        var section = bearerSection.GetSection("SigningKeys:0");
        var issuer = section["Issuer"];
        var signingKeyBase64 = section["Value"];

        Assert.NotNull(issuer);
        Assert.NotNull(signingKeyBase64);

        var signingKeyBytes = Convert.FromBase64String(signingKeyBase64);

        var audiences = bearerSection.GetSection("ValidAudiences").GetChildren().Select(s =>
        {
            var audience = s.Value;
            Assert.NotNull(audience);
            return audience;
        }).ToList();

        var jwtIssuer = new JwtIssuer(issuer, signingKeyBytes);

        var token = jwtIssuer.Create(new JwtCreatorOptions(
            JwtBearerDefaults.AuthenticationScheme,
            Name: id,
            Audiences: audiences,
            Issuer: jwtIssuer.Issuer,
            NotBefore: DateTime.UtcNow,
            ExpiresOn: DateTime.UtcNow.AddDays(1),
            Roles: new List<string>(),
            Scopes: new List<string>(),
            Claims: new Dictionary<string, string>()));

        return JwtIssuer.WriteToken(token);
    }
    
    private sealed class AddAuthenticationHandler : DelegatingHandler
    {
        private readonly Action<HttpRequestMessage> _onRequest;

        public AddAuthenticationHandler(Action<HttpRequestMessage> onRequest)
        {
            _onRequest = onRequest;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _onRequest(request);
            return base.SendAsync(request, cancellationToken);
        }
    }
}