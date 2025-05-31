using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduSync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleTestController : ControllerBase
    {
        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public IActionResult InstructorEndpoint() => Ok("Only Instructors can access this.");

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public IActionResult StudentEndpoint() => Ok("Students  can access this.");
    }
}
