using aoe.DTOs.Auth;
using aoe.Helpers;
using aoe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AoeDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(
            AoeDbContext context,
            IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request");

                if (string.IsNullOrWhiteSpace(dto.Name) ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Phone) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.Role))
                {
                    return BadRequest("Please fill all fields");
                }

                dto.Name = dto.Name.Trim();
                dto.Email = dto.Email.Trim().ToLower();
                dto.Phone = dto.Phone.Trim();
                dto.Password = dto.Password.Trim();
                dto.Role = dto.Role.Trim().ToLower();

                if (!ValidationHelper.ValidEmail(dto.Email))
                    return BadRequest("Invalid email");

                if (!ValidationHelper.ValidPhone(dto.Phone))
                    return BadRequest("Invalid phone");

                if (!ValidationHelper.ValidName(dto.Name))
                    return BadRequest("Invalid name");

                if (!ValidationHelper.ValidPassword(dto.Password))
                    return BadRequest("Invalid password");

                if (!ValidationHelper.ValidRole(dto.Role))
                    return BadRequest("Invalid role");

                if (_context.Users.Any(x =>
                    x.Email == dto.Email ||
                    x.Phone == dto.Phone))
                    return BadRequest("User exists");

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Password = dto.Password,
                    Role = dto.Role
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return Ok(new { message = "Registered successfully" });
            }
            catch (Exception ex) when (IsDatabaseUnavailable(ex))
            {
                Console.WriteLine("REGISTER DB UNAVAILABLE: " + ex);
                return StatusCode(503, new { error = "Database is unavailable" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("REGISTER ERROR: " + ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request");

                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest("Please fill all fields");
                }

                dto.Email = dto.Email.Trim().ToLower();
                dto.Password = dto.Password.Trim();

                var user = _context.Users
                    .FirstOrDefault(x => x.Email == dto.Email);

                if (user == null)
                    return Unauthorized(new
                    {
                        message = "Email not found"
                    });

                if (user.Password != dto.Password)
                    return Unauthorized(new
                    {
                        message = "Wrong password"
                    });

                var token = GenerateToken(user);

                return Ok(new
                {
                    token,
                    role = user.Role.Trim().ToLower()
                });
            }
            catch (Exception ex) when (IsDatabaseUnavailable(ex))
            {
                Console.WriteLine("LOGIN DB UNAVAILABLE: " + ex);
                return StatusCode(503, new { error = "Database is unavailable" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOGIN ERROR: " + ex);
                return StatusCode(500, new { error = ex.Message });
            }

        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.Role
                )
            };

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _config["Jwt:Key"] ?? "secret_key"
                    )
                );

            var creds =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token =
                new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires:
                        DateTime.Now.AddMinutes(
                            Convert.ToDouble(
                                _config["Jwt:ExpireMinutes"] ?? "120"
                            )
                        ),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        private static bool IsDatabaseUnavailable(Exception ex)
        {
            if (ex is Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException)
                return true;

            if (ex is Npgsql.NpgsqlException)
                return true;

            if (ex.InnerException != null)
                return IsDatabaseUnavailable(ex.InnerException);

            return false;
        }
    }
}
