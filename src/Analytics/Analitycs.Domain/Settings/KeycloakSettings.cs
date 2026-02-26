namespace Analitycs.Domain.Settings;

public class KeycloakSettings
{
    public string TokenUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GrantType { get; set; } = "client_credentials";
}

