using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Cds.Client;

namespace Shazam.Framework
{
    public class DefaultCrmClientFactory: ICrmClientFactory
    {
        private readonly AuthSettings _authSettings;

        public DefaultCrmClientFactory(IOptions<AuthSettings> cdsSettings)
        {
            _authSettings = cdsSettings.Value;
        }

        public CdsServiceClient Manufacture()
        {
            var authType = _authSettings.AuthType;
            var url = _authSettings.Url;
            var clientId = _authSettings.ClientId;
            var clientSecret = _authSettings.ClientSecret;
            var connectionString = $"AuthType={authType};url={url};ClientId={clientId};ClientSecret={clientSecret}";

            return new CdsServiceClient(connectionString);
        }
    }
}
