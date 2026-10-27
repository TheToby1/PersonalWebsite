using System.Net.Mail;
using System.Threading.Tasks;

namespace PersonalWebsite.Shared.Contact
{
    public sealed class SmtpMailSender : IMailSender
    {
        private readonly SmtpClient _smtpClient;

        public SmtpMailSender(SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public Task SendMailAsync(MailMessage message)
        {
            return _smtpClient.SendMailAsync(message);
        }
    }
}