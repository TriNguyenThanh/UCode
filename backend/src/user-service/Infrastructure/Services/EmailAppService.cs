using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UserService.Application.DTOs.Common;
using UserService.Application.Interfaces.Services;

namespace UserService.Infrastructure.Services;

public class EmailAppService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailAppService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, 
                _emailSettings.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // Log error here
            Console.WriteLine($"Failed to send email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendOTPEmailAsync(string toEmail, string otp)
    {
        var subject = "Password Reset OTP - UCode System";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>You have requested to reset your password. Please use the following OTP code:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                        {otp}
                    </div>
                    <p style='color: #666;'>This OTP will expire in 10 minutes.</p>
                    <p style='color: #666;'>If you did not request this password reset, please ignore this email.</p>
                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                    <p style='color: #999; font-size: 12px;'>UCode - Code Submission System</p>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, subject, body);
    }
}

