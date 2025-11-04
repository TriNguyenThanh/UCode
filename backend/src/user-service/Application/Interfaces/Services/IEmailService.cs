namespace UserService.Application.Interfaces.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    Task<bool> SendOTPEmailAsync(string toEmail, string otp);
}

