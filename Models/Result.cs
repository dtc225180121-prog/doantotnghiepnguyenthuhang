using System.ComponentModel.DataAnnotations.Schema;

namespace aoe.Models
{
    [Table("results")]
    public partial class Result
    {

        [Column("id")]
        public int Id { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("assignment_id")]
        public int AssignmentId { get; set; }

        [Column("score")]
        public double Score { get; set; }

        [Column("submitted_at", TypeName = "timestamp with time zone")]
        public DateTime? SubmittedAt { get; set; }

    }
}
