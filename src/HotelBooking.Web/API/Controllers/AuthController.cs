using HotelBooking.Application.DTO.Custom;
using HotelBooking.Domain.Models;
using HotelBooking.Web.API.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelBooking.Web.API.Controllers
{
    public class AuthController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._roleManager = roleManager;
            this._configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDTO.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid email or password");
                }
                if (!user.IsActive)
                {
                    return Unauthorized("Your account has been disabled. Please contact an administrator.");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized("Invalid email or password");
                }

                var token = await GenerateJwtToken(user);

                user.LastLoginAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]))
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
        [HttpPatch("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
                if (existingUser != null)
                {
                    return Conflict($"Email '{registerDTO.Email}' is already in use");
                }

                var user = new ApplicationUser
                {
                    UserName = registerDTO.Email,
                    Email = registerDTO.Email,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    PhoneNumber = registerDTO.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true,
                    Address = "",
                    Title = "",
                    Department = "", 
                    Position = ""
                };

                var result = await _userManager.CreateAsync(user, registerDTO.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors.Select(e => e.Description));
                }

                await _userManager.AddToRoleAsync(user, "Guest");

                var token = await GenerateJwtToken(user);

                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]))
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                if (!user.IsActive)
                {
                    return Unauthorized("Your account has been disabled. Please contact an administrator.");
                }

                var token = await GenerateJwtToken(user);

                return Ok(new
                {
                    token = token,
                    expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]))
                });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                var roleEntity = await _roleManager.FindByNameAsync(role);
                if (roleEntity != null)
                {
                    if (roleEntity.CanManageBookings)
                        claims.Add(new Claim("Permission", "ManageBookings"));

                    if (roleEntity.CanManageRooms)
                        claims.Add(new Claim("Permission", "ManageRooms"));

                    if (roleEntity.CanManageUsers)
                        claims.Add(new Claim("Permission", "ManageUsers"));

                    if (roleEntity.CanAccessReports)
                        claims.Add(new Claim("Permission", "AccessReports"));

                    if (roleEntity.CanManagePayments)
                        claims.Add(new Claim("Permission", "ManagePayments"));
                }
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]));

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenDescriptor);
        }
    }
}
