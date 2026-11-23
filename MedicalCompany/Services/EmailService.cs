
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MedicalCompany.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendReplyAsync(
            string recipientEmail,
            string recipientName,
            string reply)
        {
            var section = _configuration.GetSection("Email");

            var smtpServer = section["SmtpServer"]
                ?? throw new InvalidOperationException("SMTP server is not configured.");

            var port = int.Parse(section["Port"]
                ?? throw new InvalidOperationException("SMTP port is not configured."));

            var username = section["Username"]
                ?? throw new InvalidOperationException("Email username is not configured.");

            var password = section["Password"]
                ?? throw new InvalidOperationException("Email password is not configured.");

            var fromAddress = section["FromAddress"]
                ?? throw new InvalidOperationException("Sender address is not configured.");

            var fromName = section["FromName"]
                ?? "Crescent Diagnostics Factory";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "Reply to your enquiry - Crescent Diagnostics Factory";
            message.Body = new TextPart("plain") { Text = reply };

            using var client = new SmtpClient();

            var secureOption = port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(smtpServer, port, secureOption);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
