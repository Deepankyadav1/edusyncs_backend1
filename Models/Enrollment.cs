using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace EduSync.Models;

public partial class Enrollment
{
    public Guid UserId { get; set; }
    public long Id { get; set; }
    public Guid CourseId { get; set; }

    // ✅ Make these nullable so they're not required in the request body
    public virtual Course? Course { get; set; }

    public virtual User? User { get; set; }
}
