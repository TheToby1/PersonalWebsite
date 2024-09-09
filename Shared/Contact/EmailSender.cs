using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PersonalWebsite.Shared.Messaging;

namespace PersonalWebsite.Shared.Contact
{
    public interface ICommunicationSender
    {
        Task SendAsync(ContactRequest request);
    }

    public class EmailSenderOptions
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? ToAddress { get; set; }
        public string? ToDisplayName { get; set; }
        public string? NoReplyAddress { get; set; }
    }

    public class EmailSender : ICommunicationSender
    {
        private readonly SmtpClient _smtpClient;
        private readonly MailAddress _toAddress;
        private readonly MailAddress _noReplyAddress;

        public EmailSender(SmtpClient smtpClient, IOptions<EmailSenderOptions> options)
        {
            _smtpClient = smtpClient;
            _toAddress = new MailAddress(options.Value.ToAddress ?? "", options.Value.ToDisplayName ?? "TheToby");
            _noReplyAddress = string.IsNullOrEmpty(options.Value.NoReplyAddress) ?
                _toAddress :
                new MailAddress(options.Value.NoReplyAddress);
        }

        public async Task SendAsync(ContactRequest request)
        {
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(request.From))
                throw new ArgumentException("Email Address Invalid", nameof(request));

            MailAddress fromAddress;
            try
            {
                fromAddress = new MailAddress(request.From ?? string.Empty);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Email Address Invalid", nameof(request), e);
            }

            var content = $"You have received a message from: {request.From}.\n\n\"{request.Content}\"";
            using MailMessage messageTo = new(_noReplyAddress, _toAddress)
            {
                Subject = request.Subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = content,
                BodyEncoding = System.Text.Encoding.UTF8
            };
            messageTo.ReplyToList.Add(fromAddress);
            await _smtpClient.SendMailAsync(messageTo);

            var confirmationContent = $"This message is to confirm that an email has been sent to " +
                $"{_toAddress.DisplayName} with the following content:\n\n{request.Content}";
            using MailMessage messageConfirmation = new(_noReplyAddress, fromAddress)
            {
                Subject = request.Subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = confirmationContent,
                BodyEncoding = System.Text.Encoding.UTF8
            };
            await _smtpClient.SendMailAsync(messageConfirmation);
        }
    }
}
