namespace EduSync.dto
{
    public class UserUpdateDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string PasswordHash { get; set; } = null!;
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid InstructorId { get; set; }
        public string MediaUrl { get; set; } = null!;
    }

    public class AssessmentUpdateDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Questions { get; set; } = null!;
        public int MaxScore { get; set; }
    }

    public class ResultUpdateDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}