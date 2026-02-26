namespace Analitycs.Domain.Interfaces;

public interface IKeycloakTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}

