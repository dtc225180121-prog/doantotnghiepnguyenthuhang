using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/result")]
    [Authorize(Roles = "teacher")]
    public class ResultController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public ResultController(AoeDbContext context)
        {
            _context = context;
        }

        private int GetTeacherId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        private bool OwnsAssignment(int assignmentId)
        {
            return _context.Assignments.Any(a =>
                a.Id == assignmentId);
        }

        // ================= RESULTS =================
        [HttpGet("assignment/{assignmentId}")]
        public IActionResult AssignmentResults(int assignmentId)
        {
            if (!OwnsAssignment(assignmentId))
                return Unauthorized();

            var results =
                from r in _context.Results
                join u in _context.Users
                on r.StudentId equals u.Id
                where r.AssignmentId == assignmentId
                orderby r.SubmittedAt descending
                select new
                {
                    StudentId = u.Id, // 🔥 FIX
                    u.Name,
                    u.Email,
                    u.Phone,
                    r.Score,
                    r.SubmittedAt
                };

            return Ok(results.ToList());
        }

        // ================= EXPORT =================
        [HttpGet("export/{assignmentId}")]
        public IActionResult ExportCSV(int assignmentId)
        {
            if (!OwnsAssignment(assignmentId))
                return Unauthorized();

            var results =
                from r in _context.Results
                join u in _context.Users
                on r.StudentId equals u.Id
                where r.AssignmentId == assignmentId
                select new
                {
                    u.Name,
                    u.Email,
                    u.Phone,
                    r.Score,
                    r.SubmittedAt
                };

            var csv = new StringBuilder();
            csv.AppendLine("Name,Email,Phone,Score,SubmittedAt");

            foreach (var r in results)
            {
                csv.AppendLine(
                    $"\"{r.Name}\",\"{r.Email}\",\"{r.Phone}\",{r.Score},{r.SubmittedAt}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "assignment_result.csv"
            );
        }

        // ================= REVIEW =================
        [HttpGet("review/{assignmentId}/{studentId}")]
        public IActionResult Review(int assignmentId, int studentId)
        {
            if (!OwnsAssignment(assignmentId))
                return Unauthorized();

            // 🔥 LOAD ALL OPTIONS TRƯỚC (FIX N+1)
            var review = _context.Questions
    .Where(q =>
        _context.StudentAnswers.Any(sa =>
            sa.QuestionId == q.Id &&
            sa.StudentId == studentId &&
            sa.AssignmentId == assignmentId
        )
    )
    .Select(q => new
    {
        q.Id,
        q.Content,
        q.Type,
        q.CorrectAnswer,

        Answer = _context.StudentAnswers
            .Where(sa =>
                sa.QuestionId == q.Id &&
                sa.StudentId == studentId &&
                sa.AssignmentId == assignmentId
            )
            .Select(sa => sa.Answer)
            .FirstOrDefault(),

        IsCorrect = _context.StudentAnswers
            .Where(sa =>
                sa.QuestionId == q.Id &&
                sa.StudentId == studentId &&
                sa.AssignmentId == assignmentId
            )
            .Select(sa => sa.IsCorrect)
            .FirstOrDefault(),

        q.Explanation,

        Options = q.QuestionOptions
            .Select(o => o.Content)
            .ToList()
    })
    .ToList();

            return Ok(review);
        }
    }
}
