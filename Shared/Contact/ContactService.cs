using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalWebsite.Shared.Messaging;

namespace PersonalWebsite.Shared.Contact
{
    public interface IContactService
    {
        Task<ContactResponse> SendAsync(ContactRequest message);
    }

    public sealed class HttpContactOptions
    {
        [Required]
        public string? RequestUri { get; set; }
    }

    public sealed class HttpContactService : IContactService
    {
        private readonly ILogger<IContactService> Logger;
        private readonly HttpClient HttpClient;
        private readonly HttpContactOptions ContactOptions;

        public HttpContactService(HttpClient httpClient, ILogger<HttpContactService> logger, IOptions<HttpContactOptions> contactOptions)
        {
            ContactOptions = contactOptions.Value;
            HttpClient = httpClient;
            Logger = logger;
            if (string.IsNullOrWhiteSpace(ContactOptions.RequestUri))
            {
                throw new ArgumentException("A valid request uri must be supplied.", nameof(contactOptions));
            }
        }

        private readonly string LoggerString = $"{nameof(HttpContactService)}: ";

        public async Task<ContactResponse> SendAsync(ContactRequest message)
        {
            var response = new ContactResponse();

            try
            {
                Logger.LogInformation("{LoggerString}{}", LoggerString, $"Sending request to: {ContactOptions?.RequestUri}");
                var result = await HttpClient.PostAsJsonAsync(ContactOptions.RequestUri, message);
                response.Response = await result.Content.ReadAsStringAsync();
                response.ResponseCode = result.StatusCode;
            }
            catch (Exception ex)
            {
                // Log the error
                Logger.LogError("{LoggerString}{}", LoggerString, $"Error sending communication: {ex.Message}");
                response.ResponseCode = 0;
                response.Response = ex.Message;
            }

            return response;
        }
    }

    public sealed class StubContactService(ILogger<StubContactService> logger) : IContactService
    {
        private ILogger<IContactService> Logger = logger;

        private readonly string LoggerString = $"{nameof(StubContactService)}: ";

        public async Task<ContactResponse> SendAsync(ContactRequest message)
        {
            var result = new ContactResponse
            {
                Response = "Success!"
            };
            return await Task.FromResult(result);
        }
    }
}
