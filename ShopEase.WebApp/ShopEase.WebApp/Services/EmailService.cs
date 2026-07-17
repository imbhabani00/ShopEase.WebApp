using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ShopEase.WebApp.Models.Email;

namespace Ecommerce.Application.Services
{
    #region IEmailService
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string toName, string otp);
        Task SendResetPasswordAsync(string toEmail, string toName, string resetLink);
        Task SendWelcomeAsync(string toEmail, string toName);
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    }
    #endregion

    public class EmailService : IEmailService
    {
        #region Properties
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _env;
        #endregion

        #region Constructor
        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IWebHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _env = env;
        }
        #endregion

        #region SendOtpAsync
        public async Task SendOtpAsync(string toEmail, string toName, string otp)
        {
            try
            {
                var subject = "Your ShopEase Verification Code";
                var htmlBody = await RenderTemplateAsync("OtpEmail", new { Name = toName, Otp = otp });
                await SendAsync(toEmail, toName, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendOtpAsync: Error sending OTP email");
                throw;
            }
        }
        #endregion

        #region SendResetPasswordAsync
        public async Task SendResetPasswordAsync(string toEmail, string toName, string resetLink)
        {
            try
            {
                var subject = "Reset Your ShopEase Password";
                var htmlBody = await RenderTemplateAsync("ResetPassword", new { Name = toName, ResetLink = resetLink });
                await SendAsync(toEmail, toName, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendResetPasswordAsync: Error sending reset password email");
                throw;
            }
        }
        #endregion

        #region SendWelcomeAsync
        public async Task SendWelcomeAsync(string toEmail, string toName)
        {
            try
            {
                var subject = "Welcome to ShopEase!";
                var htmlBody = await RenderTemplateAsync("WelcomeEmail", new { Name = toName });
                await SendAsync(toEmail, toName, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendWelcomeAsync: Error sending welcome email");
                throw;
            }
        }
        #endregion

        #region RenderTemplateAsync
        private async Task<string> RenderTemplateAsync(string templateName, dynamic model)
        {
            try
            {
                // Build path to Views/Email folder
                var templatePath = Path.Combine(
                    _env.ContentRootPath,
                    "Views",
                    "Email",
                    $"{templateName}.cshtml");

                // Check if file exists
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Email template not found: {TemplatePath}", templatePath);
                    throw new FileNotFoundException($"Email template '{templateName}' not found");
                }

                // Read template content
                var content = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders
                content = ReplacePlaceholders(content, model);

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RenderTemplateAsync: Error rendering template {TemplateName}", templateName);
                throw;
            }
        }
        #endregion

        #region ReplacePlaceholders
        private string ReplacePlaceholders(string content, dynamic model)
        {
            if (model == null) return content;

            // Replace {{Name}}
            if (model is IDictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    var placeholder = $"{{{{{kvp.Key}}}}}";
                    var value = kvp.Value?.ToString() ?? string.Empty;
                    content = content.Replace(placeholder, value);
                }
            }
            else
            {
                // For anonymous objects
                var properties = model.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var placeholder = $"{{{{{prop.Name}}}}}";
                    var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                    content = content.Replace(placeholder, value);
                }
            }

            // Replace {{Year}} with current year
            content = content.Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

            return content;
        }
        #endregion

        #region SendAsync
        public async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody)
        {
            try
            {
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail));

                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();

                await client.ConnectAsync(
                    _emailSettings.SmtpHost,
                    _emailSettings.SmtpPort,
                    SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _emailSettings.Username,
                    _emailSettings.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Email} | Subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendAsync: Failed to send email to {Email}", toEmail);
                throw;
            }
        }
        #endregion
    }
}