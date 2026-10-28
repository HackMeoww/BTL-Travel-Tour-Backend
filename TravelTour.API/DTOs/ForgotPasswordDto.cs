namespace TravelTour.API.DTOs
{
    public class SendOtpDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordByEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
