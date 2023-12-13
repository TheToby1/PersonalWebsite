using PersonalWebsite.Shared.Messaging;

namespace PersonalWebsite.BackendFunctions.Shared
{
    public interface ICommunicationValidator
    {
        IEnumerable<string> Validate(ContactRequest request);
    }

    public class EmailValidator : ICommunicationValidator
    {
        public IEnumerable<string> Validate(ContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.From))
            {
                yield return "No from address supplied in request";
            }
        }
    }
}
