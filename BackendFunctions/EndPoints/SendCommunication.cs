using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PersonalWebsite.BackendFunctions.Shared;
using PersonalWebsite.Shared.Contact;
using PersonalWebsite.Shared.Messaging;

namespace PersonalWebsite.BackendFunctions.EndPoints
{
    public class SendCommunication(ICommunicationSender communicationSender,
        ICommunicationValidator communicationValidator,
        ILogger<SendCommunication> logger)
    {
        private readonly ICommunicationSender CommunicationSender = communicationSender;
        private readonly ICommunicationValidator CommunicationValidator = communicationValidator;
        private readonly ILogger<SendCommunication> Logger = logger;

        [Function("SendCommunication")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendcommunication")] HttpRequestData req)
        {
            Logger.LogInformation("SendCommunicationByEmail: C# HTTP trigger function received a request.");
            var requestMessage = await req.ReadFromJsonAsync<ContactRequest>();

            if (requestMessage == null)
            {
                var body = await req.ReadAsStringAsync();
                Logger.LogError("SendCommunicationByEmail: Cannot convert HTTP request body json to ContactRequest - {}", body);
                return new BadRequestObjectResult(req);
            }

            var errorMessages = CommunicationValidator.Validate(requestMessage);
            if (errorMessages.Any())
            {
                Logger.LogError("SendCommunicationByEmail: Invalid argument - {}", string.Join(", ", errorMessages));
                return new BadRequestObjectResult(req);
            }

            Logger.LogInformation("SendCommunicationByEmail: Request is - {} : {} : {}",
                requestMessage.From, requestMessage.Subject, requestMessage.Content);

            try
            {
                await CommunicationSender.SendAsync(requestMessage);
                return new OkObjectResult("Message succesfully sent!");
            }
            catch (ArgumentException ex)
            {
                Logger.LogError("SendCommunicationByEmail: Invalid argument - {}", ex);
                return new BadRequestObjectResult(req);
            }
            catch (Exception ex)
            {
                Logger.LogError("SendCommunicationByEmail: The following exception has occured - {}", ex);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

        }
    }
}
