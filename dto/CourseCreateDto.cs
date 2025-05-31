namespace EduSync.dto
{
    public class CourseCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid? InstructorId { get; set; }



        public string MediaUrl { get; set; } = null!;

    }
}
