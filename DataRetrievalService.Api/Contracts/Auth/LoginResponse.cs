namespace DataRetrievalService.Api.Contracts.Auth;

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}
