namespace EduSync.dto
{
    public class UserCreateDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string PasswordHash { get; set; } = null!;

    }
}
