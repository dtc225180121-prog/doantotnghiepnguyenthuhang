namespace aoe.DTOs.Question
{
    public class CreateQuestionDTO
    {
        public int AssignmentId { get; set; }

        public string Type { get; set; } = default!;

        public string Content { get; set; } = default!;

        public string CorrectAnswer { get; set; } = default!;

        public string? Explanation { get; set; }
    }
}
