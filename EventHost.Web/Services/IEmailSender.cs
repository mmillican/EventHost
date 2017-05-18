using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHost.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(List<string> toAddresses, string subject, string body, string fromName = null,
            string fromEmail = null,
            List<string> ccAddresses = null, List<string> bccAddresses = null, string replyTo = null);

        Task SendEmailAsync(string email, string subject, string body, string fromName = null, string fromEmail = null);
    }
}
