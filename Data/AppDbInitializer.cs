using Microsoft.AspNetCore.Identity;

namespace Authentication.Data
{
    public class AppDbInitializer
    {
        public static async Task SeedRolesToDb(IApplicationBuilder applicationBuilder)
        {
            using(var scope = applicationBuilder.ApplicationServices.CreateAsyncScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                if(!await roleManager.RoleExistsAsync("Manager"))
                    await roleManager.CreateAsync(new IdentityRole("Manager"));
                if(!await roleManager.RoleExistsAsync("Student"))
                    await roleManager.CreateAsync(new IdentityRole("Student"));

            }
        }
        
    }
}