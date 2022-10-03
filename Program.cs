using System.Text;
using Authentication.Data;
using Authentication.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//add db context with SQL
builder.Services.AddDbContext<AppDBContext>(options=>{
 options.UseSqlServer(builder.Configuration.GetConnectionString("DeafultConnectionstring"));
});

var tokenValidationParam=new TokenValidationParameters
    {
        ValidateIssuerSigningKey=true,
        IssuerSigningKey =new SymmetricSecurityKey(Encoding.ASCII.GetBytes( builder.Configuration["Jwt:SecreteKey"])),
        ValidateIssuer=true,
        ValidIssuer=builder.Configuration["Jwt:Issuer"],
        ValidateAudience=true,
        ValidAudience=builder.Configuration["Jwt:Audience"],
        ValidateLifetime=true,
        ClockSkew=TimeSpan.Zero       
    };
builder.Services.AddSingleton(tokenValidationParam);

//add identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

//add authentication
builder.Services.AddAuthentication(option=>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
//Add jwt bearer.
.AddJwtBearer(options=>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = tokenValidationParam;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//seed db with roles
await AppDbInitializer.SeedRolesToDb(app);
app.Run();
