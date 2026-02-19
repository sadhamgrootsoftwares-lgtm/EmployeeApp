using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using EmployeeApp.API.Models;
using EmployeeApp.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EmployeeApp.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists != null)
                    return BadRequest(new AuthResponse { IsSuccess = false, Message = "User already exists!" });

                ApplicationUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new AuthResponse { IsSuccess = false, Message = $"User creation failed: {errors}" });
                }

                if (!await _roleManager.RoleExistsAsync(model.Role))
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));

                await _userManager.AddToRoleAsync(user, model.Role);

                return Ok(new AuthResponse { IsSuccess = true, Message = "User created successfully!" });

            }
            catch (Exception)
            {

                throw;
            }

          
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new Dictionary<string, object>
                {
                    { JwtRegisteredClaimNames.Sub, user.Email! },
                    { JwtRegisteredClaimNames.Email, user.Email! },
                    { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() },
                    { "FullName", user.FullName }
                };

                foreach (var userRole in userRoles)
                {
                    claims.Add(ClaimTypes.Role, userRole);
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var descriptor = new SecurityTokenDescriptor
                {
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    Claims = claims,
                    Expires = DateTime.Now.AddHours(3),
                    SigningCredentials = credentials
                };

                var handler = new JsonWebTokenHandler();
                var token = handler.CreateToken(descriptor);

                return Ok(new AuthResponse
                {
                    IsSuccess = true,
                    Token = token,
                    FullName = user.FullName,
                    Email = user.Email!,
                    Role = userRoles.FirstOrDefault() ?? "User"
                });
            }
            return Unauthorized(new AuthResponse { IsSuccess = false, Message = "Invalid email or password" });
        }
    }
}
