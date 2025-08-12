namespace DataRetrievalService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<(string Token, IEnumerable<string> Roles)?> AuthenticateAsync(
            string email, string password, CancellationToken ct = default);
    }
}
