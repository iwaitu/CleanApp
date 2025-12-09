using CleanApp.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApp.Infrastructure
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {

            var userManager = serviceProvider.GetService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "Admin";
            string email = "admin@clean.app";
            string password = "place_your_password_here";

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole();
                role.Name = roleName;
                var result = await roleManager.CreateAsync(role);
            }


            if (userManager.Users.All(u => u.UserName != email))
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded && !string.IsNullOrEmpty(user.Id))
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
    }
}
