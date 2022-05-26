using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Email.Options;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Email;

public static class Extensions
{
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration,
        string optionSection = nameof(EmailOptions),
        EmailProvider provider = EmailProvider.MimKit,
        Action<EmailOptions>? configureOptions = null)
    {
        var config = configuration.GetOptions<EmailOptions>(optionSection);
        configureOptions?.Invoke(config ?? new EmailOptions());

        if (provider == EmailProvider.SendGrid)
        {
            services.AddSingleton<IEmailSender, SendGridEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, MailKitEmailSender>();
        }

        if (configureOptions is { })
        {
            services.Configure(optionSection, configureOptions);
        }
        else
        {
            services.AddOptions<EmailOptions>().Bind(configuration.GetSection(optionSection))
                .ValidateDataAnnotations();
        }

        return services;
    }
}
