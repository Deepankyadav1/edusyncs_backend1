using Microsoft.AspNetCore.Mvc;
using EduSync.dto;
using EduSync.Models;
using EduSync.Services;
using EduSync.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduSync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            _logger.LogInformation("Register() called for Email: {Email}", dto.Email);

            try
            {
                if (_context.Users.Any(u => u.Email == dto.Email))
                {
                    _logger.LogWarning("Attempt to register with an already existing email: {Email}", dto.Email);
                    return BadRequest("User already exists");
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = dto.Role
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = _tokenService.GenerateToken(user);
                _logger.LogInformation("User registered successfully with Email: {Email}", dto.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for Email: {Email}", dto.Email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            _logger.LogInformation("Login() called for Email: {Email}", dto.Email);

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: user not found for Email: {Email}", dto.Email);
                    return Unauthorized("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: password mismatch for Email: {Email}", dto.Email);
                    return Unauthorized("Password mismatch");
                }

                var token = _tokenService.GenerateToken(user);
                _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for Email: {Email}", dto.Email);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
