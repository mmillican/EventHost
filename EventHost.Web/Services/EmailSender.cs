using EventHost.Web.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHost.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;

        public EmailSender(IOptions<EmailConfig> emailOptions)
        {
            _emailConfig = emailOptions.Value;
        }

        public Task SendEmailAsync(List<string> toAddresses, string subject, string body, string fromName = null, string fromEmail = null,
            List<string> ccAddresses = null, List<string> bccAddresses = null, string replyTo = null)
        {
            if (string.IsNullOrEmpty(fromName))
                fromName = _emailConfig.FromName;

            if (string.IsNullOrEmpty(fromEmail))
                fromEmail = _emailConfig.FromAddress;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));

            if (toAddresses != null && toAddresses.Any())
            {
                foreach (var address in toAddresses)
                {
                    message.To.Add(new MailboxAddress(address));
                }
            }

            if (ccAddresses != null && ccAddresses.Any())
            {
                foreach (var address in ccAddresses)
                {
                    message.Cc.Add(new MailboxAddress(address));
                }
            }

            if (bccAddresses != null && bccAddresses.Any())
            {
                foreach (var address in bccAddresses)
                {
                    message.Bcc.Add(new MailboxAddress(address));
                }
            }

            if (!string.IsNullOrEmpty(replyTo))
            {
                message.ReplyTo.Add(new MailboxAddress(replyTo));
            }

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(_emailConfig.Server, _emailConfig.Port, _emailConfig.UseSsl);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                if (!string.IsNullOrEmpty(_emailConfig.User) && !string.IsNullOrEmpty(_emailConfig.Password))
                    client.Authenticate(_emailConfig.User, _emailConfig.Password);

                client.Send(message);
                client.Disconnect(true);
            }

            return Task.FromResult(0);
        }

        public Task SendEmailAsync(string email, string subject, string body, string fromName = null, string fromEmail = null)
        {
            if (string.IsNullOrEmpty(fromName))
                fromName = _emailConfig.FromName;

            if (string.IsNullOrEmpty(fromEmail))
                fromEmail = _emailConfig.FromAddress;

            var toList = new List<string> { email };
            return SendEmailAsync(toList, subject, body, fromName, fromEmail);
        }
    }
}
