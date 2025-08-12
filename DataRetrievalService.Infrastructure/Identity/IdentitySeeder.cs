using DataRetrievalService.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace DataRetrievalService.Infrastructure.Identity
{
    public class IdentitySeeder
    {
        public static async Task SeedAsync(
        RoleManager<IdentityRole> roles,
        UserManager<IdentityUser> users,
        SeedUsersOptions options)
        {
            foreach (UserRole userRole in Enum.GetValues<UserRole>())
            {
                var roleName = userRole.ToString();
                if (!await roles.RoleExistsAsync($"{nameof(roleName)}"))
                {
                    await roles.CreateAsync(new IdentityRole(roleName));
                }
            }
                
            if (!string.IsNullOrWhiteSpace(options.AdminEmail) &&
                !string.IsNullOrWhiteSpace(options.AdminPassword))
            {
                var admin = await users.FindByEmailAsync(options.AdminEmail);
                if (admin is null)
                {
                    admin = new IdentityUser { UserName = options.AdminEmail, Email = options.AdminEmail, EmailConfirmed = true };
                    await users.CreateAsync(admin, options.AdminPassword);
                    await users.AddToRoleAsync(admin, "Admin");
                }
            }

            if (!string.IsNullOrWhiteSpace(options.UserEmail) &&
                !string.IsNullOrWhiteSpace(options.UserPassword))
            {
                var user = await users.FindByEmailAsync(options.UserEmail);
                if (user is null)
                {
                    user = new IdentityUser { UserName = options.UserEmail, Email = options.UserEmail, EmailConfirmed = true };
                    await users.CreateAsync(user, options.UserPassword);
                    await users.AddToRoleAsync(user, "User");
                }
            }
        }
    }
}
