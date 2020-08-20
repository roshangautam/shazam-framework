using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;

namespace Shazam.Cli.Commands
{

    [Command(Name = "publish", Description = "Publish all customizations in the connected power platform environment")]
    public class PublishCommand : CrmCommand
    {
        private readonly ILogger<WhoAmICommand> _logger;

        public PublishCommand(IOrganizationService organizationService, ILogger<WhoAmICommand> logger) : base(organizationService)
        {
            _logger = logger;
        }

        public void OnExecute(CommandLineApplication app)
        {
            try
            {
                Publish();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private void Publish()
        {
            var publishAllXmlRequest = new PublishAllXmlRequest();
            if (!(CdsClient.Execute(publishAllXmlRequest) is PublishAllXmlResponse))
            {
                Console.WriteLine("Not Connected");
                return;
            }

            Console.WriteLine("All customizations published successfully");
        }
    }
}
