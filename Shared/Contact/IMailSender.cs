using System.Net.Mail;
using System.Threading.Tasks;

namespace PersonalWebsite.Shared.Contact
{
    public interface IMailSender
    {
        Task SendMailAsync(MailMessage message);
    }
}