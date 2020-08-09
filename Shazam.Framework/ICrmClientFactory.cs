using Microsoft.PowerPlatform.Cds.Client;

namespace Shazam.Framework
{
    public interface ICrmClientFactory
    {
        CdsServiceClient Manufacture();
    }
}
