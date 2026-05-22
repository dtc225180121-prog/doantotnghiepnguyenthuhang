using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/submission")]
    [Authorize]
    public class SubmissionController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public SubmissionController(AoeDbContext context)
        {
            _context = context;
        }

        [HttpGet("my")]
        public IActionResult MySubmissions()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"
            );
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "student")
            {
                var submissions = from r in _context.Results
                                 join a in _context.Assignments on r.AssignmentId equals a.Id
                                 where r.StudentId == userId
                                 orderby r.SubmittedAt descending
                                 select new
                                 {
                                     r.Id,
                                     r.AssignmentId,
                                     AssignmentTitle = a.Name,
                                     r.Score,
                                     r.SubmittedAt
                                 };

                return Ok(submissions.ToList());
            }
            else if (role == "teacher")
            {
                var submissions = from r in _context.Results
                                 join a in _context.Assignments on r.AssignmentId equals a.Id
                                 join u in _context.Users on r.StudentId equals u.Id
                                 where a.TeacherId == userId
                                 orderby r.SubmittedAt descending
                                 select new
                                 {
                                     r.Id,
                                     r.AssignmentId,
                                     StudentName = u.Name,
                                     AssignmentTitle = a.Name,
                                     r.Score,
                                     r.SubmittedAt
                                 };

                return Ok(submissions.ToList());
            }

            return BadRequest("Invalid role");
        }
    }
}