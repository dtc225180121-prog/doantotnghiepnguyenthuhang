using aoe.DTOs.Class;
using aoe.Helpers;
using aoe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace aoe.Controllers
{
    [ApiController]
    [Route("api/class")]
    [Authorize(Roles = "teacher")]
    public class ClassController : ControllerBase
    {
        private readonly AoeDbContext _context;

        public ClassController(AoeDbContext context)
        {
            _context = context;
        }


        private int GetTeacherId()
        {
            return int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                ) ?? "0"
            );
        }


        private bool OwnsClass(int classId)
        {
            var teacherId = GetTeacherId();

            return _context.Classes.Any(
                x =>
                    x.Id == classId &&
                    x.TeacherId == teacherId
            );
        }


        // CREATE CLASS
        [HttpPost("create")]
        public IActionResult CreateClass(
            CreateClassDTO dto)
        {
            if (dto.Name.Length > 20)
                return BadRequest("Invalid class name");

            var teacherId = GetTeacherId();

            string code;

            do
            {
                code =
                    ClassCodeGenerator.Generate();
            }
            while (
                _context.Classes.Any(
                    x => x.ClassCode == code
                )
            );

            var newClass =
                new Class
                {
                    Name = dto.Name,
                    ClassCode = code,
                    TeacherId = teacherId
                };

            _context.Classes.Add(newClass);

            _context.SaveChanges();

            return Ok(newClass);
        }


        // DELETE CLASS
        [HttpDelete("delete/{classId}")]
        public IActionResult DeleteClass(int classId)
        {
            if (!OwnsClass(classId))
                return Unauthorized();

            var cls =
                _context.Classes.Find(classId);

            if (cls == null)
                return NotFound();

            _context.Classes.Remove(cls);

            _context.SaveChanges();

            return Ok("Class deleted");
        }


        // LIST MY CLASSES
        [HttpGet("my-classes")]
        public IActionResult MyClasses(
            string? keyword)
        {
            var teacherId = GetTeacherId();

            var query =
                _context.Classes
                .Where(x =>
                    x.TeacherId == teacherId
                );

            if (!string.IsNullOrEmpty(keyword))
            {
                query =
                    query.Where(x =>
                        x.Name.Contains(keyword)
                    );
            }

            return Ok(query.ToList());
        }

        [HttpGet("my")]
        public IActionResult MyClassesAlias(string? keyword)
        {
            return MyClasses(keyword);
        }

        [HttpGet("all-students")]
        public IActionResult AllStudents()
        {
            var teacherId = GetTeacherId();

            var studentIds = _context.Classes
                .Where(c => c.TeacherId == teacherId)
                .Join(_context.ClassStudents,
                      c => c.Id,
                      cs => cs.ClassId,
                      (c, cs) => cs.StudentId)
                .Distinct()
                .ToList();

            return Ok(studentIds);
        }


        // LIST STUDENTS IN CLASS
        [HttpGet("students/{classId}")]
        public IActionResult Students(int classId)
        {
            if (!OwnsClass(classId))
                return Unauthorized();

            var students =
                from cs in _context.ClassStudents
                join u in _context.Users
                on cs.StudentId equals u.Id
                where cs.ClassId == classId
                select new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Phone
                };

            return Ok(students.ToList());
        }


        // ADD STUDENT BY EMAIL
        [HttpPost("add-student")]
        public IActionResult AddStudent(
            AddStudentDTO dto)
        {
            if (!OwnsClass(dto.ClassId))
                return Unauthorized();

            var student =
                _context.Users.FirstOrDefault(
                    x =>
                        x.Email == dto.Email &&
                        x.Role == "student"
                );

            if (student == null)
                return NotFound("Student not found");

            bool exists =
                _context.ClassStudents.Any(
                    x =>
                        x.ClassId == dto.ClassId &&
                        x.StudentId == student.Id
                );

            if (exists)
                return BadRequest("Already exists");

            _context.ClassStudents.Add(
                new ClassStudent
                {
                    ClassId = dto.ClassId,
                    StudentId = student.Id
                }
            );

            _context.SaveChanges();

            return Ok("Added");
        }


        // REMOVE STUDENT
        [HttpDelete("remove-student/{studentId}/{classId}")]
        public IActionResult RemoveStudent(
            int studentId,
            int classId)
        {
            if (!OwnsClass(classId))
                return Unauthorized();

            var record =
                _context.ClassStudents.FirstOrDefault(
                    x =>
                        x.StudentId == studentId &&
                        x.ClassId == classId
                );

            if (record == null)
                return NotFound();

            _context.ClassStudents.Remove(record);

            _context.SaveChanges();

            return Ok("Removed");
        }


        // UPDATE STUDENT INFO
        [HttpPut("update-student")]
        public IActionResult UpdateStudent(
            UpdateStudentDTO dto)
        {
            var student =
                _context.Users.FirstOrDefault(
                    x =>
                        x.Id == dto.StudentId &&
                        x.Role == "student"
                );

            if (student == null)
                return NotFound("Student not found");

            if (!ValidationHelper.ValidName(dto.Name))
                return BadRequest("Invalid name");

            if (!ValidationHelper.ValidPhone(dto.Phone))
                return BadRequest("Invalid phone");

            student.Name = dto.Name;
            student.Phone = dto.Phone;

            _context.SaveChanges();

            return Ok("Student updated");
        }
    }
}