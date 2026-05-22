namespace aoe.DTOs.AI
{
    public class AIQuestionDTO
    {
        public string Content { get; set; } = default!;

        public List<AIOptionDTO>? Options { get; set; }

        public string CorrectAnswer { get; set; } = default!;

        public string Explanation { get; set; } = default!;
    }
}
