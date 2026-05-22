namespace aoe.DTOs.Assignment
{
    public class CreateAssignmentDTO
    {
        public string Name { get; set; } = default!;

        public string QuestionType { get; set; } = default!;

        public int QuestionCount { get; set; }

        public DateTime? OpenTime { get; set; }

        public DateTime? CloseTime { get; set; }

        public bool ShowResult { get; set; }

        public bool ShowExplanation { get; set; }
    }
}
