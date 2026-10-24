using System.Net;
using System.Net.Mail;

namespace TravelTour.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string fullName, string otpCode);
        Task<bool> SendRegistrationOtpEmailAsync(string toEmail, string fullName, string otpCode);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string fullName, string otpCode)
        {
            return await SendEmailInternalAsync(
                toEmail,
                fullName,
                otpCode,
                "Yêu cầu xác nhận khôi phục mật khẩu",
                "Hệ thống nhận được yêu cầu đặt lại mật khẩu cho tài khoản liên kết với địa chỉ email này trên website <strong>HackMeoww Travel Tour</strong>. Dưới đây là mã xác thực OTP của bạn:"
            );
        }

        public async Task<bool> SendRegistrationOtpEmailAsync(string toEmail, string fullName, string otpCode)
        {
            return await SendEmailInternalAsync(
                toEmail,
                fullName,
                otpCode,
                "Xác nhận đăng ký tài khoản thành viên mới",
                "Cảm ơn bạn đã lựa chọn đăng ký tài khoản tại <strong>HackMeoww Travel Tour</strong>. Để hoàn tất quy trình kích hoạt tài khoản của bạn, vui lòng nhập mã xác thực OTP sau:"
            );
        }

        private async Task<bool> SendEmailInternalAsync(string toEmail, string fullName, string otpCode, string headerSubtitle, string bodyIntro)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var portStr = _configuration["EmailSettings:Port"] ?? "587";
                int.TryParse(portStr, out int port);
                if (port == 0) port = 587;

                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
                var senderName = _configuration["EmailSettings:SenderName"] ?? "HackMeoww - Travel Tour";
                var password = _configuration["EmailSettings:Password"] ?? "";

                // Nếu chưa cấu hình mật khẩu ứng dụng Gmail thì log cảnh báo và trả về false
                if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("EmailSettings chưa cấu hình SenderEmail hoặc Password trong appsettings.json. Hệ thống sử dụng chế độ OTP trực tiếp trên màn hình.");
                    return false;
                }

                using var message = new MailMessage();
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = $"[{senderName}] Mã xác thực OTP: {otpCode}";
                message.IsBodyHtml = true;

                message.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
</head>
<body style=""font-family: 'Segoe UI', Arial, sans-serif; background-color: #f1f5f9; padding: 30px 15px; margin: 0;"">
    <div style=""max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.06);"">
        <div style=""background: linear-gradient(135deg, #0ea5e9, #0284c7); padding: 28px 24px; text-align: center;"">
            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 800; letter-spacing: -0.5px;"">✈️ HackMeoww - Travel Tour</h1>
            <p style=""color: rgba(255,255,255,0.9); margin: 6px 0 0; font-size: 14px;"">{headerSubtitle}</p>
        </div>
        <div style=""padding: 32px 28px;"">
            <p style=""font-size: 16px; color: #0f172a; margin-top: 0;"">Xin chào <strong>{(string.IsNullOrWhiteSpace(fullName) ? "Quý khách" : fullName)}</strong>,</p>
            <p style=""font-size: 14.5px; color: #475569; line-height: 1.6; margin-bottom: 24px;"">
                {bodyIntro}
            </p>
            <div style=""text-align: center; margin: 28px 0;"">
                <span style=""display: inline-block; font-size: 34px; font-weight: 800; letter-spacing: 8px; color: #0284c7; background: #f0f9ff; padding: 14px 36px; border-radius: 12px; border: 2px dashed #0284c7; box-shadow: 0 4px 12px rgba(2, 132, 199, 0.15);"">{otpCode}</span>
            </div>
            <p style=""font-size: 13.5px; color: #64748b; line-height: 1.5; margin-bottom: 0; text-align: center;"">
                ⏱️ Mã OTP có hiệu lực trong vòng <strong>5 phút</strong>. Vui lòng tuyệt đối không chia sẻ mã này cho bất kỳ ai.
            </p>
        </div>
        <div style=""background: #f8fafc; border-top: 1px solid #f1f5f9; padding: 18px 24px; text-align: center; font-size: 12px; color: #94a3b8;"">
            Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ hỗ trợ.<br>
            © 2026 HackMeoww Travel Tour. All rights reserved.
        </div>
    </div>
</body>
</html>";

                using var client = new SmtpClient(smtpServer, port);
                client.Credentials = new NetworkCredential(senderEmail, password);
                client.EnableSsl = true;

                await client.SendMailAsync(message);
                _logger.LogInformation($"[EmailService] Đã gửi thành công mã OTP {otpCode} tới {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EmailService] Gửi email thất bại tới {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
