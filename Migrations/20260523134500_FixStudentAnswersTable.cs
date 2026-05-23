using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aoe.Migrations
{
    public partial class FixStudentAnswersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF to_regclass('public.student_answers') IS NULL THEN
                        CREATE TABLE public.student_answers (
                            student_id integer NOT NULL,
                            assignment_id integer NOT NULL,
                            question_id integer NOT NULL,
                            answer text NULL,
                            is_correct boolean NULL,
                            CONSTRAINT student_answers_pkey PRIMARY KEY (student_id, assignment_id, question_id)
                        );
                    END IF;

                    IF to_regclass('public."StudentAnswers"') IS NOT NULL THEN
                        EXECUTE 'INSERT INTO public.student_answers (student_id, assignment_id, question_id, answer, is_correct)
                                 SELECT "StudentId", "AssignmentId", "QuestionId", "Answer", "IsCorrect"
                                 FROM public."StudentAnswers"
                                 ON CONFLICT (student_id, assignment_id, question_id) DO NOTHING';

                        EXECUTE 'DROP TABLE public."StudentAnswers"';
                    END IF;
                END $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS public.student_answers;
                """);
        }
    }
}
