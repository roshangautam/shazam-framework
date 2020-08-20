using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Shazam.Cli.Commands
{

    [Command(Name = "who", Description = "Display connection information for power platform environment")]
    public class WhoAmICommand : CrmCommand
    {
        private readonly ILogger<WhoAmICommand> _logger;

        public WhoAmICommand(IOrganizationService organizationService, ILogger<WhoAmICommand> logger) :
            base(organizationService)
        {
            _logger = logger;
        }

        public void OnExecute(CommandLineApplication app)
        {
            try
            {
                WhoAmI();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private void WhoAmI()
        {
            var whoAmIRequest = new WhoAmIRequest();
            if (!(CdsClient.Execute(whoAmIRequest) is WhoAmIResponse whoAmIResponse))
            {
                Console.WriteLine("Not Connected");
                return;
            }

            var systemUser = CdsClient.Retrieve("systemuser", whoAmIResponse.UserId, new ColumnSet("firstname", "lastname", "domainname"));
            Console.WriteLine("Logged in as {0} {1} to {2}\n", systemUser.Attributes["firstname"],
                systemUser.GetAttributeValue<string>("lastname"),
                CdsClient.ConnectedOrgFriendlyName);
            Console.WriteLine("Organization Information:");
            Console.WriteLine("\tOrg ID:\t{0}", CdsClient.ConnectedOrgId);
            Console.WriteLine("\tUnique Name:\t{0}", CdsClient.ConnectedOrgUniqueName);
            Console.WriteLine("\tFriendly Name:\t{0}", CdsClient.ConnectedOrgFriendlyName);
            Console.WriteLine("\tOrg Url:\t{0}", CdsClient.CdsConnectOrgUriActual);
            Console.WriteLine("\tUser Id:\t{0}", systemUser.Attributes["domainname"]);
        }
    }
}
