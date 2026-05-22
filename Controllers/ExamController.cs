using aoe.DTOs;
using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/exam")]
    [Authorize(Roles = "student")]
    public class ExamController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public ExamController(AoeDbContext context)
        {
            _context = context;
        }


        private int GetStudentId()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                ) ?? "0"
            );
        }


        // LIST ASSIGNMENTS OF CLASS
        [HttpGet("assignments/{classId}")]
        public IActionResult Assignments(int classId)
        {
            var assignments =
                from ac in _context.AssignmentClasses
                join a in _context.Assignments
                on ac.AssignmentId equals a.Id
                where ac.ClassId == classId
                select a;

            return Ok(assignments);
        }

        [HttpGet("state/{assignmentId}")]
        [Authorize(Roles = "student")]
        public IActionResult GetExamState(int assignmentId)
        {
            // ===== LẤY STUDENT ID =====
            var studentId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"
            );

            // ===== CHECK STUDENT THUỘC CLASS CÓ ASSIGNMENT =====
            bool hasAccess =
            (
                from ac in _context.AssignmentClasses
                join cs in _context.ClassStudents
                    on ac.ClassId equals cs.ClassId
                where ac.AssignmentId == assignmentId
                && cs.StudentId == studentId
                select ac
            ).Any();

            if (!hasAccess)
                return Unauthorized();

            // ===== LẤY ASSIGNMENT =====
            var assignment =
                _context.Assignments
                .Where(a => a.Id == assignmentId)
                .Select(a => new
                {
                    a.OpenTime,
                    a.CloseTime,
                    a.ShowResult,
                    a.ShowExplanation
                })
                .FirstOrDefault();

            if (assignment == null)
                return NotFound();

            // ===== CHECK SUBMITTED =====
            bool submitted =
                _context.Results.Any(r =>
                    r.AssignmentId == assignmentId &&
                    r.StudentId == studentId
                );

            // ===== RETURN =====
            return Ok(new
            {
                openTime = assignment.OpenTime,
                closeTime = assignment.CloseTime,
                submitted,
                showResult = assignment.ShowResult,
                showExplanation = assignment.ShowExplanation
            });
        }

        // START EXAM
        [HttpGet("start/{assignmentId}")]
        public IActionResult StartExam(int assignmentId)
        {
            var assignment =
                _context.Assignments
                .FirstOrDefault(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound();

            if (assignment.OpenTime != null &&
                DateTime.Now < assignment.OpenTime)
                return BadRequest("Not open yet");

            if (assignment.CloseTime != null &&
                DateTime.Now > assignment.CloseTime)
                return BadRequest("Closed");

            var questions =
        _context.Questions
        .Where(q => q.AssignmentId == assignmentId)
        .Select(q => new
        {
            q.Id,
            q.Type,
            q.Content,

            Options =
                q.Type == "single_choice"
                ? _context.QuestionOptions
                    .Where(o => o.QuestionId == q.Id)
                    .OrderBy(o => o.Label)
                    .Select(o => new
                    {
                        label = o.Label.ToString(),
                        content = o.Content
                    })
                    .ToList()
                : null
        })
        .ToList();

            return Ok(questions);
        }


        // SUBMIT EXAM
        [HttpPost("submit")]
        public IActionResult SubmitExam(SubmitExamDTO dto)
        {
            var studentId = GetStudentId();

            double totalScore = 0;

            var debug = new List<object>();

            foreach (var ans in dto.Answers)
            {
                var question =
                    _context.Questions.Find(ans.QuestionId);

                if (question == null)
                    continue;

                string dbAnswer =
                    question.CorrectAnswer?
                        .Trim()
                        .ToUpper() ?? "";

                string studentAnswer =
                    ans.Answer?
                        .Trim()
                        .ToUpper() ?? "";

                bool correct =
                    dbAnswer == studentAnswer;

                if (correct)
                    totalScore += question.Score;

                debug.Add(new
                {
                    questionId = question.Id,

                    dbAnswer,

                    studentAnswer,

                    correct,

                    score = question.Score
                });

                var existing =
                    _context.StudentAnswers.FirstOrDefault(x =>
                        x.StudentId == studentId &&
                        x.AssignmentId == dto.AssignmentId &&
                        x.QuestionId == ans.QuestionId
                    );

                if (existing != null)
                {
                    existing.Answer = ans.Answer;
                    existing.IsCorrect = correct;
                }
                else
                {
                    _context.StudentAnswers.Add(
                        new StudentAnswer
                        {
                            StudentId = studentId,
                            AssignmentId = dto.AssignmentId,
                            QuestionId = ans.QuestionId,
                            Answer = ans.Answer,
                            IsCorrect = correct
                        }
                    );
                }
            }

            _context.Results.Add(
                new Result
                {
                    StudentId = studentId,
                    AssignmentId = dto.AssignmentId,
                    Score = totalScore
                }
            );

            _context.SaveChanges();

            return Ok(new
            {
                score = totalScore,
                debug
            });
        }

        // VIEW RESULT
        [HttpGet("result/{assignmentId}")]
        public IActionResult Result(
            int assignmentId)
        {
            var studentId =
                GetStudentId();

            var result =
                _context.Results.FirstOrDefault(
                    x =>
                        x.StudentId ==
                        studentId &&
                        x.AssignmentId ==
                        assignmentId
                );

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        // HISTORY
        [HttpGet("history/{assignmentId}")]
        public IActionResult History(int assignmentId)
        {
            var studentId = GetStudentId();

            var history =
                _context.Results
                .Where(x =>
                    x.StudentId == studentId &&
                    x.AssignmentId == assignmentId
                )
                .OrderByDescending(x => x.SubmittedAt ?? DateTime.MinValue) // mới nhất lên đầu
                .Select(x => new
                {
                    score = x.Score,
                    time = x.SubmittedAt
                })
                .ToList();

            return Ok(history);
        }

        [HttpGet("review/{assignmentId}")]
        public IActionResult Review(int assignmentId)
        {
            var studentId = GetStudentId();

            var questions =
                _context.Questions
                .Where(q => q.AssignmentId == assignmentId)
                .Select(q => new
                {
                    q.Id,
                    q.Content,
                    q.Type,
                    q.CorrectAnswer,
                    q.Explanation,

                    StudentAnswer =
                        _context.StudentAnswers
                        .Where(a =>
                            a.StudentId == studentId &&
                            a.QuestionId == q.Id)
                        .Select(a => a.Answer)
                        .FirstOrDefault(),

                    IsCorrect =
                        _context.StudentAnswers
                        .Where(a =>
                            a.StudentId == studentId &&
                            a.QuestionId == q.Id)
                        .Select(a => a.IsCorrect)
                        .FirstOrDefault(),

                    Options =
                        q.QuestionOptions
                        .Select(o => new
                        {
                            o.Label,
                            o.Content
                        })
                        .ToList()
                })
                .ToList();

            return Ok(questions);
        }
    }
}
