using Microsoft.AspNetCore.Identity;

namespace CleanApp.Domain
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
