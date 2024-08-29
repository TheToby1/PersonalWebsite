using System.Net;
using System.Net.Mail;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalWebsite.BackendFunctions.Shared;
using PersonalWebsite.Shared.Contact;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(con =>
    {
        con.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
    })
    .ConfigureServices((builder, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddScoped<SmtpClient>((serviceProvider) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            return new SmtpClient()
            {
                Host = config.GetValue<String>("SmtpOptions:Host"),
                Port = config.GetValue<int>("SmtpOptions:Port"),
                Credentials = new NetworkCredential(
                        config.GetValue<String>("SmtpOptions:Username"),
                        config.GetValue<String>("SmtpOptions:Password")
                    ),
                EnableSsl = true
            };
        });
        services.AddOptions<EmailSenderOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(nameof(EmailSenderOptions)).Bind(settings))
            .ValidateDataAnnotations();
        services.AddTransient<ICommunicationValidator, EmailValidator>();
        services.AddTransient<ICommunicationSender, EmailSender>();
    })
    .Build();

host.Run();