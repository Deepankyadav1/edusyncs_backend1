using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using Microsoft.Extensions.Logging;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(AppDbContext context, ILogger<EnrollmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Enrollments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            _logger.LogInformation("GetEnrollments() called.");

            try
            {
                return await _context.Enrollments.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollments.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Enrollments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetEnrolledCoursesByUser(Guid userId)
        {
            _logger.LogInformation("GetEnrolledCoursesByUser() called for userId: {UserId}", userId);

            try
            {
                var enrollments = await _context.Enrollments
                    .Where(e => e.UserId == userId)
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                    .ToListAsync();

                return Ok(enrollments.Select(e => e.Course));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses for userId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Enrollments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(long id)
        {
            _logger.LogInformation("GetEnrollment() called with Id: {Id}", id);

            try
            {
                var enrollment = await _context.Enrollments.FindAsync(id);

                if (enrollment == null)
                {
                    _logger.LogWarning("Enrollment not found with Id: {Id}", id);
                    return NotFound();
                }

                return enrollment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving enrollment with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Enrollments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollment(long id, Enrollment enrollment)
        {
            _logger.LogInformation("PutEnrollment() called with Id: {Id}", id);

            if (id != enrollment.Id)
            {
                _logger.LogWarning("BadRequest: Enrollment Id mismatch.");
                return BadRequest();
            }

            _context.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Enrollment updated successfully with Id: {Id}", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!EnrollmentExists(id))
                {
                    _logger.LogWarning("Enrollment not found for update with Id: {Id}", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "Concurrency error while updating enrollment with Id: {Id}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Enrollments
        [HttpPost]
        public async Task<ActionResult<Enrollment>> PostEnrollment([FromBody] Enrollment enrollment)
        {
            _logger.LogInformation("PostEnrollment() called.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new enrollment.");
                return BadRequest(ModelState);
            }

            try
            {
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == enrollment.UserId && e.CourseId == enrollment.CourseId);

                if (existingEnrollment != null)
                {
                    _logger.LogWarning("User {UserId} is already enrolled in course {CourseId}.",
                        enrollment.UserId, enrollment.CourseId);

                    return BadRequest(new { message = "User is already enrolled in this course." });
                }

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Enrollment created successfully with Id: {Id}", enrollment.Id);

                return CreatedAtAction(nameof(GetEnrollment), new { id = enrollment.Id }, enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating enrollment.");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Enrollments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(long id)
        {
            _logger.LogInformation("DeleteEnrollment() called with Id: {Id}", id);

            try
            {
                var enrollment = await _context.Enrollments.FindAsync(id);
                if (enrollment == null)
                {
                    _logger.LogWarning("Enrollment not found for deletion with Id: {Id}", id);
                    return NotFound();
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Enrollment deleted successfully with Id: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting enrollment with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool EnrollmentExists(long id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
