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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            _logger.LogInformation("Fetching all users.");
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            _logger.LogInformation("Fetching user with ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", id);
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, UserUpdateDto dto)
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for update with ID: {UserId}", id);
                return NotFound();
            }

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.PasswordHash = dto.PasswordHash;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated with ID: {UserId}", id);
            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] UserCreateDto dto)
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = dto.PasswordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user created with ID: {UserId}", user.UserId);
            return CreatedAtAction("GetUser", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for deletion with ID: {UserId}", id);
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted with ID: {UserId}", id);
            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
