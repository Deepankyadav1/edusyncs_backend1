using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using EduSync.dto;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(AppDbContext context, ILogger<CoursesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            _logger.LogInformation("GetCourses() called.");

            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .ToListAsync();

                return courses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all courses.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Courses/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(Guid id)
        {
            _logger.LogInformation("GetCourse() called with Id: {Id}", id);

            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Assessments)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                {
                    _logger.LogWarning("Course not found with Id: {Id}", id);
                    return NotFound();
                }

                return course;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving course with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Courses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(Guid id, CourseUpdateDto dto)
        {
            _logger.LogInformation("PutCourse() called with Id: {Id}", id);

            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    _logger.LogWarning("Course not found for update with Id: {Id}", id);
                    return NotFound();
                }

                course.Title = dto.Title;
                course.Description = dto.Description;
                course.InstructorId = dto.InstructorId;
                course.MediaUrl = dto.MediaUrl;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Course updated successfully with Id: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating course with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Courses
        [Authorize(Roles = "Instructor")]
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(CourseCreateDto dto)
        {
            _logger.LogInformation("PostCourse() called.");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID claim missing or empty.");
                return Unauthorized("User ID claim missing.");
            }

            if (!Guid.TryParse(userId, out var instructorId))
            {
                _logger.LogWarning("User ID claim is not a valid GUID: {UserId}", userId);
                return Unauthorized("Invalid user ID.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.UserId == instructorId);
            if (!userExists)
            {
                _logger.LogWarning("Instructor user not found with ID: {InstructorId}", instructorId);
                return Unauthorized("Instructor user not found.");
            }

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                MediaUrl = dto.MediaUrl,
                InstructorId = instructorId
            };

            _context.Courses.Add(course);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                var baseEx = dbEx.GetBaseException();
                _logger.LogError(baseEx, "Database error while saving course: {Message}", baseEx.Message);
                return StatusCode(500, $"Database error: {baseEx.Message}");
            }

            _logger.LogInformation("Course created with ID: {Id}", course.CourseId);
            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, course);
        }




        // DELETE: api/Courses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            _logger.LogInformation("DeleteCourse() called with Id: {Id}", id);

            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    _logger.LogWarning("Course not found for deletion with Id: {Id}", id);
                    return NotFound();
                }

                // Delete related assessments
                var relatedAssessments = _context.Assessments.Where(a => a.CourseId == id);
                _context.Assessments.RemoveRange(relatedAssessments);

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Course deleted successfully with Id: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting course with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
