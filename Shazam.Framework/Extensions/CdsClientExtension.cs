using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;

namespace Shazam.Framework.Extensions
{
    public static class CdsClientExtension
    {
        public static IServiceCollection AddCdsClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IOrganizationService>(CreateCdsServiceClient(services));

            return services;
        }

        private static CdsServiceClient CreateCdsServiceClient(IServiceCollection services)
        {
            var settings = services.BuildServiceProvider().GetRequiredService<IOptions<AuthSettings>>().Value;
            var authType = settings.AuthType;
            var url = settings.Url;
            var clientId = settings.ClientId;
            var clientSecret = settings.ClientSecret;
            var connectionString = $"AuthType={authType};url={url};ClientId={clientId};ClientSecret={clientSecret}";
            return new CdsServiceClient(connectionString);
        }
    }
}
