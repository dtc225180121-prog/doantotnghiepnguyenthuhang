using aoe.DTOs.Class;
using aoe.DTOs.Student;
using aoe.Helpers;
using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "student")]
    public class StudentController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public StudentController(AoeDbContext context)
        {
            _context = context;
        }

        // JOIN CLASS
        [HttpPost("join-class")]
        public IActionResult JoinClass(
            JoinClassDTO dto)
        {
            var studentId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    ) ?? "0"
                );

            var classObj =
                _context.Classes.FirstOrDefault(
                    x => x.ClassCode == dto.ClassCode
                );

            if (classObj == null)
                return NotFound("Class not found");

            bool exists =
                _context.ClassStudents.Any(
                    x =>
                        x.ClassId == classObj.Id &&
                        x.StudentId == studentId
                );

            if (exists)
                return BadRequest(
                    "Already joined"
                );

            _context.ClassStudents.Add(
                new ClassStudent
                {
                    ClassId = classObj.Id,
                    StudentId = studentId
                }
            );

            _context.SaveChanges();

            return Ok("Joined class");
        }


        // LIST MY CLASSES
        [HttpGet("my-classes")]
        public IActionResult MyClasses()
        {
            var studentId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    ) ?? "0"
                );

            var classes =
                from cs in _context.ClassStudents
                join c in _context.Classes
                on cs.ClassId equals c.Id
                where cs.StudentId == studentId
                select new
                {
                    c.Id,
                    c.Name,
                    c.ClassCode
                };

            return Ok(classes.ToList());
        }
        [HttpGet("my")]
        [Authorize(Roles = "student")]
        public IActionResult GetMyClasses()
        {
            var studentId =
            int.Parse(
            User.FindFirstValue(
            ClaimTypes.NameIdentifier
            ) ?? "0"
            );

            var classes =
            from cs in _context.ClassStudents
            join c in _context.Classes
            on cs.ClassId equals c.Id
            where cs.StudentId == studentId
            select new
            {
                c.Id,
                c.Name,
                c.ClassCode
            };

            return Ok(classes.ToList());
        }
    }
}
