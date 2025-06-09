using MailKit.Net.Smtp;
using MimeKit;

namespace KPI_Dashboard.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("KPI Dashboard", _configuration["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            // Safely parse SMTP port with a default value
            var portString = _configuration["EmailSettings:SmtpPort"];
            int port = 25; // default SMTP port
            if (!int.TryParse(portString, out port))
            {
                port = 25;
            }

            // Safely parse UseSsl with a default value
            var useSslString = _configuration["EmailSettings:UseSsl"];
            bool useSsl = false;
            if (!bool.TryParse(useSslString, out useSsl))
            {
                useSsl = false;
            }

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    port,
                    useSsl);

                await client.AuthenticateAsync(
                    _configuration["EmailSettings:SenderEmail"],
                    _configuration["EmailSettings:SmtpPassword"]);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
