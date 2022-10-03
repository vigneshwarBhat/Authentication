using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.Data;
using Authentication.Data.Models;
using Authentication.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController:ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParam;
        public AuthenticationController(UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, AppDBContext context,  IConfiguration configuration, TokenValidationParameters tokenValidationParameters)
        {
            _configuration=configuration;
            _userManager=userManager;
            _roleManager=roleManager;
            _context=context;
            _tokenValidationParam=tokenValidationParameters;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register request)
        {
            if(!ModelState.IsValid)
                return BadRequest("Please provide all the required fields");

            var userExists= await _userManager.FindByEmailAsync(request.EmailAddress);
            if(userExists !=null)
                return BadRequest("User already exists");

            var user = new ApplicationUser
            {
              FirstName=request.FirstName,
              LastName=request.LastName,
              Email=request.EmailAddress,
              UserName=request.UserName,
              SecurityStamp= Guid.NewGuid().ToString()              
            };

           var result = await _userManager.CreateAsync(user, request.Password);
           if(result.Succeeded)
           {
                //add user role
                switch(request.Role)
                {
                    case "Manager":
                                    await _userManager.AddToRoleAsync(user, "Manager");
                                    break;
                    case "Student":
                                    await _userManager.AddToRoleAsync(user, "Student");
                                    break;
                    default: break;
                }
                return Ok("User created.");
           }
            return BadRequest();           

        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignIn loginRequest)
        {
            if(!ModelState.IsValid)
                return BadRequest("Please provide all required fields");

            var userExists= await _userManager.FindByEmailAsync(loginRequest.EmailAddress);
            if(userExists !=null && await _userManager.CheckPasswordAsync(userExists, loginRequest.Password))
            {
                var tokenValue= await GenerateJwtToken(userExists);
                return Ok(tokenValue);
            }

            return Unauthorized();
        }


        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest )
        {
            if(!ModelState.IsValid)
                return BadRequest("Please provide all required fields");

            var result=await VerifyAndgenerateToken(tokenRequest);
            return Ok(result);
        }

        private async Task<AuthResult> VerifyAndgenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler=new JwtSecurityTokenHandler();
            var storedToken=await _context.RefreshTokens.FirstOrDefaultAsync(x=>x.Token==tokenRequest.RefreshToken);
            var dbUser=await _userManager.FindByIdAsync(storedToken?.UserId);
            try
            {
                var tokenCheckResult=jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParam, out var isValidToken);
                return await GenerateJwtToken(dbUser, storedToken);
            }
            catch(SecurityTokenExpiredException)
            {
                if (storedToken?.DateExpire < DateTime.UtcNow)
                    return await GenerateJwtToken(dbUser);
                else
                    return await GenerateJwtToken(dbUser, storedToken);
            }
        }

        private async Task<AuthResult> GenerateJwtToken(ApplicationUser user, RefreshToken? rToken =null)
        {
            var claims=new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            //add user role claims
            var userRoles =await _userManager.GetRolesAsync(user);
            foreach(var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var signingKey=new SymmetricSecurityKey(Encoding.ASCII.GetBytes( _configuration["Jwt:SecreteKey"]));
            var token=new JwtSecurityToken
            (
                issuer:_configuration["Jwt:Issuer"], 
                audience:_configuration["Jwt:Audience"],
                expires:DateTime.UtcNow.AddMinutes(1),
                claims:claims,
                signingCredentials:new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );
            var jwtToken= new JwtSecurityTokenHandler().WriteToken(token);
            if(rToken != null)
            {
                return new AuthResult
                {
                    Token=jwtToken,
                    ExpiresAt=token.ValidTo,
                    RefreshToken=rToken.Token
                }; 
            }
            var refreshToken=new RefreshToken
            {
                JwtId=token.Id,
                IsRevoked=false,
                UserId=user.Id,
                DateAdded=DateTime.UtcNow,
                DateExpire=DateTime.UtcNow.AddMonths(6),
                Token=Guid .NewGuid().ToString()+ "-" + Guid.NewGuid().ToString()
                
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return new AuthResult
            {
                Token=jwtToken,
                ExpiresAt=token.ValidTo,
                RefreshToken=refreshToken.Token
            };
        }
    }
}