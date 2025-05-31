using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using EduSync.Data;
using EduSync.dto;
using EduSync.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly ILogger<MediaController> _logger;

        public MediaController(IConfiguration config, AppDbContext context, ILogger<MediaController> logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
        }

        // POST: api/media/save
        [HttpPost("save")]
        public async Task<IActionResult> SaveMedia([FromBody] MediaDto dto)
        {
            _logger.LogInformation("SaveMedia() called for CourseId: {CourseId}", dto.CourseId);

            try
            {
                var course = await _context.Courses.FindAsync(dto.CourseId);
                if (course == null)
                {
                    _logger.LogWarning("Course not found for CourseId: {CourseId}", dto.CourseId);
                    return NotFound("Course not found.");
                }

                course.MediaUrl = dto.MediaUrl;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Media URL saved successfully for CourseId: {CourseId}", dto.CourseId);
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving media for CourseId: {CourseId}", dto.CourseId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/media/sas?fileName=abc.pdf
        [HttpGet("sas")]
        public IActionResult GenerateSasToken([FromQuery] string fileName)
        {
            _logger.LogInformation("GenerateSasToken() called for file: {FileName}", fileName);

            try
            {
                var accountName = _config["AzureStorage:AccountName"];
                var accountKey = _config["AzureStorage:AccountKey"];
                var containerName = _config["AzureStorage:ContainerName"];

                var blobUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}/{fileName}");
                var credential = new StorageSharedKeyCredential(accountName, accountKey);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = fileName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.Read);

                var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
                var fullUri = $"{blobUri}?{sasToken}";

                _logger.LogInformation("SAS token generated successfully for file: {FileName}", fileName);

                return Ok(new
                {
                    sasUrl = fullUri,
                    blobUrl = blobUri.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS token for file: {FileName}", fileName);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/media/download?fileName=abc.pdf
        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string fileName)
        {
            _logger.LogInformation("Download() called for file: {FileName}", fileName);

            try
            {
                var connectionString = _config["AzureStorage:ConnectionString"];
                var containerName = _config["AzureStorage:ContainerName"];

                var blobClient = new BlobContainerClient(connectionString, containerName)
                    .GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("File not found in blob storage: {FileName}", fileName);
                    return NotFound("File not found");
                }

                var stream = await blobClient.OpenReadAsync();
                _logger.LogInformation("File streamed successfully: {FileName}", fileName);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading file: {FileName}", fileName);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
