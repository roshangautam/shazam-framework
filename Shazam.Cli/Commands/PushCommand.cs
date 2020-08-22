using System;
using System.IO;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using ShellProgressBar;

namespace Shazam.Cli.Commands
{
    [Command(Name = "push", Description = "Import unmanaged solution to power platform environment")]
    public class PushCommand : CrmCommand
    {
        private readonly ILogger<PushCommand> _logger;
        private readonly SolutionSettings _solutionSettings;

        public PushCommand(IOrganizationService organizationService, ILogger<PushCommand> logger,
            IOptions<SolutionSettings> solutionSettings): base(organizationService)
        {
            _logger = logger;
            _solutionSettings = solutionSettings.Value;
        }

        public void OnExecute(CommandLineApplication app)
        {
            try
            {
                DeleteSolution();
                ImportSolution();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private void DeleteSolution()
        {
            var queryImportedSolution = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet("solutionid", "friendlyname"),
                Criteria = new FilterExpression()
            };
            queryImportedSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, _solutionSettings.SolutionName);
            var importedSolution = CdsClient.RetrieveMultiple(queryImportedSolution);

            if (importedSolution.Entities == null || importedSolution.Entities.Count <= 0)
            {
                return;
            }

            var solution = importedSolution.Entities[0];
            CdsClient.Delete("solution", (Guid)solution["solutionid"]);

            Console.WriteLine("Deleted the {0} solution.", solution["friendlyname"]);
        }

        private void ImportSolution()
        {
            var solutionFilePath = $@"{_solutionSettings.SolutionExportDirectory}{_solutionSettings.SolutionName}.zip";

            var data = File.ReadAllBytes(solutionFilePath);
            var importId = Guid.NewGuid();

            Console.WriteLine("Importing unmanged solution {0} to environment {1}.",
                _solutionSettings.SolutionName,
                CdsClient.ConnectedOrgFriendlyName);

            var importSolutionRequest = new ImportSolutionRequest
            {
                CustomizationFile = data,
                ImportJobId = importId,
                PublishWorkflows = true,
                OverwriteUnmanagedCustomizations = true,
                ConvertToManaged = false,
                SkipProductUpdateDependencies = false
            };

            void Starter() => ProgressReport(importId);
            var t = new Thread(Starter);
            t.Start();

            CdsClient.Execute(importSolutionRequest);
            Console.WriteLine("Solution {0} successfully imported into {1}",
                solutionFilePath,
                CdsClient.ConnectedOrgFriendlyName);
        }

        private void ProgressReport(object importId)
        {
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '_',
                ProgressBarOnBottom = false
            };
            var pbar = new ProgressBar(100, $"\t Import Job Id : {importId}\t Connected : {CdsClient.ConnectedOrgFriendlyName}", options);
            while (true)
            {
                try
                {
                    var job = CdsClient.Retrieve("importjob", (Guid) importId, new ColumnSet("solutionname", "progress", "completedon"));
                    var progress = Convert.ToDecimal(job["progress"]);
                    var completed = job.Attributes.ContainsKey("completedon") ? job["completedon"] : null;
                    pbar.Tick(Convert.ToInt32(job["progress"]));

                    if (progress == 100 || completed != null)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    pbar.WriteLine(ex.Message);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
