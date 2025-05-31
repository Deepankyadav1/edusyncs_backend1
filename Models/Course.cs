using System;
using System.Collections.Generic;

namespace EduSync.Models;

public partial class Course
{
    public Guid CourseId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Guid InstructorId { get; set; }

    public string? MediaUrl { get; set; }

    public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual User Instructor { get; set; } = null!;
}
