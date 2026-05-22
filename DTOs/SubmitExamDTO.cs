namespace aoe.DTOs
{
    public class SubmitExamDTO
    {
        public int AssignmentId { get; set; }

        public List<StudentAnswerDTO> Answers { get; set; } = default!;
    }

    public class StudentAnswerDTO
    {
        public int QuestionId { get; set; }

        public string Answer { get; set; } = default!;
    }
}
