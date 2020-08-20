using System;
using System.IO;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Shazam.Framework;
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
                ImportSolution();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
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
                OverwriteUnmanagedCustomizations = false
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
            var pbar = new ProgressBar(100, $"Import Job Started Successfully : {importId}", options);
            while (true)
            {
                try
                {
                    var job = CdsClient.Retrieve("importjob", (Guid) importId, new ColumnSet("solutionname", "progress"));
                    var progress = Convert.ToDecimal(job["progress"]);

                    pbar.Tick(Convert.ToInt32(job["progress"]));

                    if (progress == 100)
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
