using System.Net;

namespace PersonalWebsite.Shared.Messaging
{
    public class ContactRequest
    {
        public string? From { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }

    public class ContactResponse
    {
        // Useful to use this enum but also kind of odd
        public HttpStatusCode ResponseCode { get; set; }
        public string? Response { get; set; }
    }
}