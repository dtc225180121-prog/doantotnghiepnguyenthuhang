using aoe.DTOs.User;
using aoe.Helpers;
using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public UserController(AoeDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    ) ?? "0"
                );

            var user =
                _context.Users.Find(userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Name,
                user.Email,
                user.Phone,
                user.Role
            });
        }

        [HttpPut("update-profile")]
        public IActionResult UpdateProfile(
    UpdateProfileDTO dto)
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    ) ?? "0"
                );

            var user =
                _context.Users.Find(userId);

            if (user == null)
                return NotFound();

            if (!ValidationHelper.ValidName(dto.Name))
                return BadRequest("Invalid name");

            if (!ValidationHelper.ValidPhone(dto.Phone))
                return BadRequest("Invalid phone");

            user.Name = dto.Name;
            user.Phone = dto.Phone;

            _context.SaveChanges();

            return Ok("Profile updated");
        }

        [HttpPut("change-password")]
        public IActionResult ChangePassword(
    ChangePasswordDTO dto)
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    ) ?? "0"
                );

            var user =
                _context.Users.Find(userId);

            if (user == null)
                return NotFound();

            if (user.Password != dto.OldPassword)
                return BadRequest("Wrong password");

            if (!ValidationHelper.ValidPassword(
                dto.NewPassword))
                return BadRequest("Invalid password");

            user.Password = dto.NewPassword;

            _context.SaveChanges();

            return Ok("Password changed");
        }
    }
}
