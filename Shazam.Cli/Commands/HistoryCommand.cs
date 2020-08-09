using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Shazam.Framework;

namespace Shazam.Cli.Commands
{
    [Command(Name = "history", Description = "Retrieve audit history of an entity attribute from a power platform environment configured in settings.[dev].json")]
    public class HistoryCommand
    {
        private readonly ILogger<PullCommand> _logger;
        private readonly CdsServiceClient _cdsServiceClient;

        [Required]
        [Option("-en|--entity-name", CommandOptionType.SingleValue, Description = "Entity Name")]
        public string EntityName { get; set; }

        [Required]
        [Option("-ei|--entity-id", CommandOptionType.SingleValue, Description = "Entity Guid")]
        public string EntityId { get; set; }

        [Required]
        [Option("-an|--attribute-name", CommandOptionType.SingleValue, Description = "Attribute Name")]
        public string AttributeName { get; set; }

        public HistoryCommand(ICrmClientFactory crmClientFactory, ILogger<PullCommand> logger)
        {
            _logger = logger;
            _cdsServiceClient = crmClientFactory.Manufacture();
        }

        public void OnExecute(CommandLineApplication app)
        {
            try
            {
                RetrieveAttributeHistory();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            };
        }

        private void RetrieveAttributeHistory()
        {
            var req = new RetrieveAttributeChangeHistoryRequest
            {
                Target = new EntityReference(EntityName, new Guid(EntityId)),
                AttributeLogicalName = AttributeName
            };

            if (!(_cdsServiceClient.Execute(req) is RetrieveAttributeChangeHistoryResponse resp))
            {
                return;
            }

            var details = resp.AuditDetailCollection;
            foreach (var detail in details.AuditDetails)
            {
                //Important: the AuditDetailCollection.AuditDetails doesn’t always contain the type of AttributeAuditDetail, so make sure it is of correct type before casting

                if (!(detail is AttributeAuditDetail attributeDetail)) continue;
                string oldValue = " ", newValue = " ";

                if (attributeDetail.OldValue.Contains(AttributeName)) {
                    oldValue = attributeDetail.OldValue[AttributeName].ToString();
                }
                Console.WriteLine($"Old Value : {oldValue ?? ""}");

                if (attributeDetail.NewValue.Contains(AttributeName)) {
                    newValue = attributeDetail.NewValue[AttributeName].ToString();
                }
                Console.WriteLine($"New Value : {newValue ?? ""}");

            }
        }
    }
}



