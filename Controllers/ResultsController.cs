using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using EduSync.dto;
using Microsoft.Extensions.Logging;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResultsController> _logger;

        public ResultsController(AppDbContext context, ILogger<ResultsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            _logger.LogInformation("Fetching all results.");
            return await _context.Results.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(Guid id)
        {
            _logger.LogInformation("Fetching result with ID: {ResultId}", id);
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Result not found with ID: {ResultId}", id);
                return NotFound();
            }
            return result;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetResultsForUser(Guid userId)
        {
            _logger.LogInformation("Fetching results for User ID: {UserId}", userId);

            var results = await _context.Results
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.ResultId,
                    r.Score,
                    r.AttemptDate,
                    AssessmentTitle = r.Assessment.Title,
                    CourseId = r.Assessment.CourseId,
                    CourseTitle = r.Assessment.Course.Title
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<object>>> GetFilteredResults(Guid? userId, Guid? courseId)
        {
            _logger.LogInformation("Filtering results for UserId: {UserId}, CourseId: {CourseId}", userId, courseId);

            var query = _context.Results
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.User)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (courseId.HasValue)
                query = query.Where(r => r.Assessment.CourseId == courseId.Value);

            var results = await query
                .Select(r => new
                {
                    r.ResultId,
                    r.Score,
                    r.AttemptDate,
                    AssessmentTitle = r.Assessment.Title,
                    CourseTitle = r.Assessment.Course.Title,
                    UserName = r.User.Name,
                    CourseId = r.Assessment.CourseId,
                    UserId = r.UserId
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("detailed")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllResultsDetailed()
        {
            _logger.LogInformation("Fetching all results with detailed information.");
            var results = await _context.Results
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.ResultId,
                    r.Score,
                    r.AttemptDate,
                    AssessmentTitle = r.Assessment.Title,
                    CourseTitle = r.Assessment.Course.Title,
                    CourseId = r.Assessment.CourseId,
                    UserName = r.User.Name,
                    UserId = r.UserId
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> PostResult(ResultCreateDto dto)
        {
            _logger.LogInformation("Posting new result for UserId: {UserId}, AssessmentId: {AssessmentId}", dto.UserId, dto.AssessmentId);

            var result = new Result
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                Score = dto.Score,
                AttemptDate = dto.AttemptDate
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Result created with ID: {ResultId}", result.ResultId);
            return CreatedAtAction("GetResult", new { id = result.ResultId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutResult(Guid id, ResultUpdateDto dto)
        {
            _logger.LogInformation("Updating result with ID: {ResultId}", id);
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Result not found with ID: {ResultId}", id);
                return NotFound();
            }

            result.AssessmentId = dto.AssessmentId;
            result.UserId = dto.UserId;
            result.Score = dto.Score;
            result.AttemptDate = dto.AttemptDate;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Result updated with ID: {ResultId}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            _logger.LogInformation("Deleting result with ID: {ResultId}", id);
            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Result not found with ID: {ResultId}", id);
                return NotFound();
            }

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Result deleted with ID: {ResultId}", id);
            return NoContent();
        }

        private bool ResultExists(Guid id)
        {
            return _context.Results.Any(e => e.ResultId == id);
        }
    }
}
