namespace EduSync.dto
{
    public class AssessmentCreateDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Questions { get; set; } = null!;
        public int MaxScore { get; set; }

    }
}
