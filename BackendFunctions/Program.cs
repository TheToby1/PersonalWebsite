using System.Net;
using System.Net.Mail;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        // Register IMailSender - use real SmtpMailSender if configured, otherwise use StubMailSender
        services.AddScoped<IMailSender>((serviceProvider) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var host = config.GetValue<String>("SmtpOptions:Host");
            
            if (!string.IsNullOrWhiteSpace(host))
            {
                var smtpClient = new SmtpClient()
                {
                    Host = host,
                    Port = config.GetValue<int>("SmtpOptions:Port"),
                    Credentials = new NetworkCredential(
                        config.GetValue<String>("SmtpOptions:Username"),
                        config.GetValue<String>("SmtpOptions:Password")
                    ),
                    EnableSsl = true
                };
                return new SmtpMailSender(smtpClient);
            }
            
            // Return stub if no SMTP configured
            var logger = serviceProvider.GetRequiredService<ILogger<StubMailSender>>();
            return new StubMailSender(logger);
        });
        
        services.AddOptions<EmailSenderOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(nameof(EmailSenderOptions)).Bind(settings))
            .ValidateDataAnnotations();
        services.AddTransient<ICommunicationValidator, EmailValidator>();
        services.AddTransient<ICommunicationSender, EmailSender>();
    })
    .Build();

host.Run();