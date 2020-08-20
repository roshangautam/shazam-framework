using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;

namespace Shazam.Cli.Commands
{
    public class CrmCommand
    {
        protected readonly CdsServiceClient CdsClient;

        protected CrmCommand(IOrganizationService organizationService)
        {
            CdsClient = organizationService as CdsServiceClient;
        }
    }
}
