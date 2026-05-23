using Microsoft.AspNetCore.Mvc;

namespace aoe.Controllers
{
    using aoe.DTOs.Assignment;
    using aoe.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [ApiController]
    [Route("api/assignment")]
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public AssignmentController(AoeDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        private bool OwnsAssignment(int assignmentId)
        {
            var teacherId = GetUserId();

            return _context.Assignments.Any(a =>
                a.Id == assignmentId &&
                a.TeacherId == teacherId);
        }

        private bool OwnsClass(int classId)
        {
            var teacherId = GetUserId();

            return _context.Classes.Any(c =>
                c.Id == classId &&
                c.TeacherId == teacherId);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var assignment = _context.Assignments.FirstOrDefault(x => x.Id == id);

            if (assignment == null)
                return NotFound("Assignment not found");

            return Ok(new
            {
                assignment.Id,
                assignment.Name,
                assignment.QuestionType,
                assignment.QuestionCount,
                assignment.OpenTime,
                assignment.CloseTime,
                assignment.ShowResult,
                assignment.ShowExplanation,
                assignment.TeacherId
            });
        }

        // CREATE
        [HttpPost("create")]
        [Authorize(Roles = "teacher")]
        public IActionResult Create(CreateAssignmentDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Assignment name required");

            if (dto.Name.Length > 20)
                return BadRequest("Assignment name max 20 characters");

            if (dto.QuestionType != "single_choice"
                && dto.QuestionType != "fill_blank")
                return BadRequest("Invalid question type");

            if (dto.QuestionCount <= 0)
                return BadRequest("Question count must be > 0");

            // 🔥 FIX
            var teacherId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var assignment =
                new Assignment
                {
                    Name = dto.Name.Trim(),
                    QuestionType = dto.QuestionType,
                    QuestionCount = dto.QuestionCount,
                    OpenTime = dto.OpenTime,
                    CloseTime = dto.CloseTime,
                    ShowResult = dto.ShowResult,
                    ShowExplanation = dto.ShowExplanation,
                    TeacherId = teacherId
                };

            _context.Assignments.Add(assignment);

            _context.SaveChanges();

            return Ok(assignment);
        }


        // LIST
        [HttpGet("my-assignments")]
        [Authorize(Roles = "teacher")]
        public IActionResult MyAssignments(
            string? keyword)
        {
            var teacherId = GetUserId();

            var query = _context.Assignments
                .Where(x => x.TeacherId == teacherId);

            if (!string.IsNullOrEmpty(keyword))
            {
                query =
                    query.Where(x =>
                        x.Name.Contains(keyword));
            }

            return Ok(query.ToList());
        }

        // ALIAS FOR MY-ASSIGNMENTS
        [HttpGet("my")]
        [Authorize(Roles = "teacher")]
        public IActionResult MyAssignmentsAlias(string? keyword)
        {
            return MyAssignments(keyword);
        }

        // LIST FOR STUDENT
        [HttpGet("student")]
        [Authorize(Roles = "student")]
        public IActionResult StudentAssignments()
        {
            var studentId = GetUserId();

            var assignmentIds =
                from cs in _context.ClassStudents
                join ac in _context.AssignmentClasses
                    on cs.ClassId equals ac.ClassId
                where cs.StudentId == studentId
                select ac.AssignmentId;

            var assignments =
                _context.Assignments
                .Where(a => assignmentIds.Distinct().Contains(a.Id))
                .OrderByDescending(a => a.OpenTime ?? DateTime.MinValue)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.QuestionType,
                    a.QuestionCount,
                    a.OpenTime,
                    a.CloseTime,
                    a.ShowResult,
                    a.ShowExplanation,
                    Submitted = _context.Results.Any(r =>
                        r.AssignmentId == a.Id &&
                        r.StudentId == studentId)
                });

            return Ok(assignments.ToList());
        }


        // UPDATE
        [HttpPut("update/{id}")]
        [Authorize(Roles = "teacher")]
        public IActionResult Update(
            int id,
            CreateAssignmentDTO dto)
        {
            var assignment =
                _context.Assignments.Find(id);

            if (assignment == null)
                return NotFound();

            assignment.Name = dto.Name;
            assignment.QuestionType =
                dto.QuestionType;
            assignment.QuestionCount =
                dto.QuestionCount;
            assignment.OpenTime =
                dto.OpenTime;
            assignment.CloseTime =
                dto.CloseTime;
            assignment.ShowResult =
                dto.ShowResult;
            assignment.ShowExplanation =
                dto.ShowExplanation;

            _context.SaveChanges();

            return Ok("Updated");
        }


        // DELETE
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "teacher")]
        public IActionResult Delete(int id)
        {
            var assignment =
                _context.Assignments.Find(id);

            if (assignment == null)
                return NotFound();

            _context.Assignments.Remove(
                assignment);

            _context.SaveChanges();

            return Ok("Deleted");
        }


        // ASSIGN → CLASS
        [HttpPost("assign-to-class")]
        [Authorize(Roles = "teacher")]
        public IActionResult AssignToClass(
AssignToClassDTO dto)
        {
            if (!OwnsAssignment(dto.AssignmentId))
                return Unauthorized("Assignment not found or not yours");


            if (!OwnsClass(dto.ClassId))
                return Unauthorized("Class not found or not yours");


            var exists =
            _context.AssignmentClasses.Any(x =>
            x.AssignmentId == dto.AssignmentId &&
            x.ClassId == dto.ClassId
            );

            if (exists)
                return Ok("Already assigned");


            _context.AssignmentClasses.Add(
            new AssignmentClass
            {
                AssignmentId = dto.AssignmentId,
                ClassId = dto.ClassId
            });

            _context.SaveChanges();

            return Ok("Assigned");
        }


        // LIST CLASSES OF ASSIGNMENT
        [HttpGet("classes/{assignmentId}")]
        [Authorize(Roles = "teacher")]
        public IActionResult Classes(
            int assignmentId)
        {
            if (!OwnsAssignment(assignmentId))
                return Unauthorized();

            var classes =
                from ac in _context.AssignmentClasses
                join c in _context.Classes
                on ac.ClassId equals c.Id
                where ac.AssignmentId ==
                      assignmentId
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
