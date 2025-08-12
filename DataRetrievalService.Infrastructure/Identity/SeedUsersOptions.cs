namespace DataRetrievalService.Infrastructure.Identity
{
    /// <summary>
    /// Bound from configuration (SeedUsers section). Used only in Development to seed demo users.
    /// </summary>
    public class SeedUsersOptions
    {
        public string? AdminEmail { get; set; }
        public string? AdminPassword { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
    }
}
