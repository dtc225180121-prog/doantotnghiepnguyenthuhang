namespace aoe.DTOs.Class
{
    public class UpdateStudentDTO
    {
        public int StudentId { get; set; }

        public string Name { get; set; } = default!;

        public string Phone { get; set; } = default!;
    }
}
