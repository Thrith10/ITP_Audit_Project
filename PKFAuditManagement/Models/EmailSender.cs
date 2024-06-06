using PKFAuditManagement.Interface;
using System.Net;
using System.Net.Mail;

namespace PKFAuditManagement.Models
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtpOptions;

        public EmailSender(SmtpOptions smtpOptions)
        {
            _smtpOptions = smtpOptions;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
            {
                Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password),
                EnableSsl = _smtpOptions.EnableSsl
            })
            using (var mailMessage = new MailMessage(_smtpOptions.From, email)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            })
            {
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
