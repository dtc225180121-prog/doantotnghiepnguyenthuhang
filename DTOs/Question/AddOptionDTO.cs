namespace aoe.DTOs.Question
{
    public class AddOptionsDTO
    {
        public int QuestionId { get; set; }

        public string A { get; set; } = default!;

        public string B { get; set; } = default!;

        public string C { get; set; } = default!;

        public string D { get; set; } = default!;
    }
}
