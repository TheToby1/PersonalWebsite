using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PersonalWebsite.Shared.Contact
{
    public sealed class StubMailSender : IMailSender
    {
        private readonly ILogger<StubMailSender> _logger;

        public StubMailSender(ILogger<StubMailSender> logger)
        {
            _logger = logger;
        }

        public Task SendMailAsync(MailMessage message)
        {
            var toAddresses = string.Join(", ", message.To.Cast<MailAddress>().Select(m => m.Address));
            _logger.LogInformation("StubMailSender: Would send email to {To} with subject: {Subject}", 
                toAddresses, 
                message.Subject);
            
            _logger.LogInformation("StubMailSender: Email body: {Body}", message.Body);
            
            return Task.CompletedTask;
        }
    }
}