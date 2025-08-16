namespace DataRetrievalService.Application.Interfaces;

public interface IAuthService
{
    Task<(string token, IEnumerable<string> roles)?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
