using Microsoft.AspNetCore.Authentication;

namespace ECL.Security;

public sealed class TestAuthOptions : AuthenticationSchemeOptions
{
    public bool Enabled { get; set; } = false;
    public string HeaderName { get; set; } = "X-Test-Auth";
    public string HeaderValue { get; set; } = "admin";
}

