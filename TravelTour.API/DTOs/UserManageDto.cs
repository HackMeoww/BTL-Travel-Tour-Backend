namespace TravelTour.API.DTOs
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string PassWord { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string? Address { get; set; }
    }

    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? PassWord { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string? Address { get; set; }
    }

    public class UpdateRoleDto
    {
        public string Role { get; set; } = "Customer";
    }
}
