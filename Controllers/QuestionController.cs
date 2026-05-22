using aoe.DTOs.Question;
using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/question")]
    [Authorize(Roles = "teacher")]
    public class QuestionController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public QuestionController(AoeDbContext context)
        {
            _context = context;
        }

        // ===== CORE: RECALCULATE SCORE =====
        private void RecalculateScores(int assignmentId)
        {
            var questions = _context.Questions
                .Where(q => q.AssignmentId == assignmentId)
                .ToList();

            if (questions.Count == 0) return;

            var scorePerQuestion =
                Math.Round(100.0 / questions.Count, 2);

            foreach (var q in questions)
            {
                q.Score = scorePerQuestion;
            }

            _context.SaveChanges();
        }

        // ===== CREATE =====
        [HttpPost("create")]
        public IActionResult Create(CreateQuestionDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload");

            if (dto.Type != "single_choice"
                && dto.Type != "fill_blank")
                return BadRequest("Invalid question type");

            var assignment =
                _context.Assignments
                .FirstOrDefault(x => x.Id == dto.AssignmentId);

            if (assignment == null)
                return NotFound("Assignment not found");
            var currentQuestionCount = _context.Questions.Count(q => q.AssignmentId == dto.AssignmentId);

            if (currentQuestionCount >= assignment.QuestionCount)
            {
                return BadRequest(
                    $"Maximum {assignment.QuestionCount} questions reached"
                );
            };

            var teacherId =
                int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (assignment.TeacherId != teacherId)
                return Unauthorized("Not your assignment");

            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Content required");

            if (dto.Type == "fill_blank")
            {
                if (!System.Text.RegularExpressions.Regex
                    .IsMatch(dto.Content, @"^[A-Za-z ]+$"))
                    return BadRequest("Content only letters + spaces");

                if (!System.Text.RegularExpressions.Regex
                    .IsMatch(dto.CorrectAnswer, @"^[A-Za-z ]+$"))
                    return BadRequest("Answer only letters + spaces");
            }

            if (dto.Type == "single_choice")
            {
                if (!new[] { "A", "B", "C", "D" }
                    .Contains(dto.CorrectAnswer))
                    return BadRequest("Answer must be A B C or D");
            }

            var question = new Question
            {
                AssignmentId = dto.AssignmentId,
                Type = dto.Type,
                Content = dto.Content.Trim(),
                CorrectAnswer = dto.CorrectAnswer.Trim(),
                Explanation = dto.Explanation?.Trim(),
                Score = 0 // sẽ được set lại ngay sau
            };

            _context.Questions.Add(question);
            _context.SaveChanges();

            // 🔥 UPDATE SCORE
            RecalculateScores(dto.AssignmentId);

            return Ok(question);
        }

        // ===== ADD OPTIONS =====
        [HttpPost("add-options")]
        public IActionResult AddOptions(AddOptionsDTO dto)
        {
            var question = _context.Questions.Find(dto.QuestionId);

            if (question == null)
                return NotFound();

            if (question.Type != "single_choice")
                return BadRequest("Only single_choice");

            var options = new List<QuestionOption>
            {
                new QuestionOption { QuestionId = dto.QuestionId, Label = 'A', Content = dto.A },
                new QuestionOption { QuestionId = dto.QuestionId, Label = 'B', Content = dto.B },
                new QuestionOption { QuestionId = dto.QuestionId, Label = 'C', Content = dto.C },
                new QuestionOption { QuestionId = dto.QuestionId, Label = 'D', Content = dto.D }
            };

            _context.QuestionOptions.AddRange(options);
            _context.SaveChanges();

            return Ok("Options added");
        }

        // ===== GET BY ASSIGNMENT =====
        [HttpGet("by-assignment/{assignmentId}")]
        public IActionResult ByAssignment(int assignmentId)
        {
            var questions =
                _context.Questions
                .Where(q => q.AssignmentId == assignmentId)
                .Select(q => new
                {
                    q.Id,
                    q.Type,
                    q.Content,
                    q.CorrectAnswer,
                    q.Explanation,
                    q.Score, // 🔥 NEW

                    Options =
                        q.Type == "single_choice"
                        ? _context.QuestionOptions
                            .Where(o => o.QuestionId == q.Id)
                            .Select(o => new
                            {
                                o.Label,
                                o.Content
                            })
                            .ToList()
                        : null
                });

            return Ok(questions);
        }

        // ===== UPDATE =====
        [HttpPut("update/{id}")]
        public IActionResult Update(int id, CreateQuestionDTO dto)
        {
            var question = _context.Questions.Find(id);

            if (question == null)
                return NotFound();

            question.Content = dto.Content;
            question.CorrectAnswer = dto.CorrectAnswer;
            question.Explanation = dto.Explanation;

            _context.SaveChanges();

            // 🔥 đảm bảo consistency
            RecalculateScores(question.AssignmentId);

            return Ok("Updated");
        }

        [HttpPut("update-options/{questionId}")]
        public IActionResult UpdateOptions(int questionId, AddOptionsDTO dto)
        {
            var options = _context.QuestionOptions
                .Where(o => o.QuestionId == questionId)
                .ToList();

            if (!options.Any())
                return NotFound();

            foreach (var o in options)
            {
                if (o.Label == 'A') o.Content = dto.A;
                if (o.Label == 'B') o.Content = dto.B;
                if (o.Label == 'C') o.Content = dto.C;
                if (o.Label == 'D') o.Content = dto.D;
            }

            _context.SaveChanges();

            return Ok();
        }

        // ===== DELETE =====
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var question = _context.Questions.Find(id);

            if (question == null)
                return NotFound();

            var assignmentId = question.AssignmentId;

            _context.Questions.Remove(question);
            _context.SaveChanges();

            // 🔥 UPDATE SCORE
            RecalculateScores(assignmentId);

            return Ok("Deleted");
        }
    }
}