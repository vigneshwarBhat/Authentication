using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Authentication.Data.Models;

namespace Authentication.Data;

public class AppDBContext:IdentityDbContext<ApplicationUser>
{
    public AppDBContext(DbContextOptions<AppDBContext> options):base(options)
    {
        
    }

    public DbSet<RefreshToken> RefreshTokens {get;set;}
}
