using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shazam.Framework.Extensions
{
    public static class CrmClientExtension
    {
        public static IServiceCollection AddCrmClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddLogging();
            services.AddOptions();

            services.TryAddSingleton<DefaultCrmClientFactory>();
            services.TryAddSingleton<ICrmClientFactory>(serviceProvider => serviceProvider.GetRequiredService<DefaultCrmClientFactory>());

            return services;
        }
    }
}
