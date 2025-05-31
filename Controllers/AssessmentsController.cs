using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EduSync.Data;
using EduSync.Models;
using EduSync.dto;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AssessmentsController> _logger;

        public AssessmentsController(AppDbContext context, ILogger<AssessmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Assessments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Assessment>>> GetAssessments()
        {
            _logger.LogInformation("GetAssessments() called.");
            try
            {
                return await _context.Assessments.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all assessments.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Assessments/bycourse/{courseId}
        [HttpGet("bycourse/{courseId}")]
        public async Task<ActionResult<IEnumerable<Assessment>>> GetAssessmentsByCourse(Guid courseId)
        {
            _logger.LogInformation("GetAssessmentsByCourse() called with CourseId: {CourseId}", courseId);
            try
            {
                var assessments = await _context.Assessments
                    .Where(a => a.CourseId == courseId)
                    .ToListAsync();

                return Ok(assessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching assessments for course {CourseId}.", courseId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Assessments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Assessment>> GetAssessment(Guid id)
        {
            _logger.LogInformation("GetAssessment() called with Id: {Id}", id);
            try
            {
                var assessment = await _context.Assessments.FindAsync(id);

                if (assessment == null)
                {
                    _logger.LogWarning("Assessment not found with Id: {Id}", id);
                    return NotFound();
                }

                return assessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching assessment with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Assessments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessment(Guid id, AssessmentUpdateDto dto)
        {
            _logger.LogInformation("PutAssessment() called with Id: {Id}", id);
            try
            {
                var assessment = await _context.Assessments.FindAsync(id);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment not found for update with Id: {Id}", id);
                    return NotFound();
                }

                assessment.CourseId = dto.CourseId;
                assessment.Title = dto.Title;
                assessment.Questions = dto.Questions;
                assessment.MaxScore = dto.MaxScore;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating assessment with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Assessments
        [HttpPost]
        public async Task<ActionResult<Assessment>> PostAssessment(AssessmentCreateDto dto)
        {
            _logger.LogInformation("PostAssessment() called.");
            try
            {
                var assessment = new Assessment
                {
                    AssessmentId = Guid.NewGuid(),
                    CourseId = dto.CourseId,
                    Title = dto.Title,
                    Questions = dto.Questions,
                    MaxScore = dto.MaxScore
                };

                _context.Assessments.Add(assessment);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetAssessment", new { id = assessment.AssessmentId }, assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating assessment.");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Assessments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            _logger.LogInformation("DeleteAssessment() called with Id: {Id}", id);
            try
            {
                var assessment = await _context.Assessments.FindAsync(id);
                if (assessment == null)
                {
                    _logger.LogWarning("Assessment not found for deletion with Id: {Id}", id);
                    return NotFound();
                }

                _context.Assessments.Remove(assessment);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting assessment with Id: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private bool AssessmentExists(Guid id)
        {
            return _context.Assessments.Any(e => e.AssessmentId == id);
        }
    }
}
