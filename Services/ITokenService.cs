using EduSync.Models;

namespace EduSync.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}