using EBookDashboard.Interfaces;
using System.Net;
using System.Net.Mail;
namespace EBookDashboard.Services
{
    public class EmailService : IEmailService
    {
        // Configuration to read email settings from appsettings.json
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var port = int.Parse(_configuration["Email:Port"]);
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }

}
