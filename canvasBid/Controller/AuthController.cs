using canvasBid.Dtos;
using canvasBid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace canvasBid.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                name = dto.fullName
            };

            if (dto.Role == "Artist")
                user.Status = AccountStatus.Pending;
            else
                user.Status = AccountStatus.Approved;

            var result = await _userManager.CreateAsync(user, dto.password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok("User Created");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return Unauthorized("Invalid Email or Password");

            var check = await _userManager.CheckPasswordAsync(user, dto.password);

            if (!check)
                return Unauthorized("Invalid Email or Password");

            
            if (user.Status == AccountStatus.Pending)
                return Unauthorized("Your account is waiting for admin approval");

            if (user.Status == AccountStatus.Rejected)
                return Unauthorized("Your account has been rejected");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id)
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminData()
        {
            return Ok("Hello Admin");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("accept/{id}")]
        public async Task<IActionResult> AcceptArtist(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.Status = AccountStatus.Approved;

            await _userManager.UpdateAsync(user);

            return Ok("Artist Approved");
        }



        [Authorize(Roles = "Admin")]
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectArtist(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.Status = AccountStatus.Rejected;

            await _userManager.UpdateAsync(user);

            return Ok("Artist Rejected");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending-artists")]
        public IActionResult GetPendingArtists()
        {
            var artists = _userManager.Users
                .Where(u => u.Status == AccountStatus.Pending)
                .ToList();

            return Ok(artists);
        }
    }
}