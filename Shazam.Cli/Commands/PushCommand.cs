using System;
using System.IO;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk.Query;
using Shazam.Framework;
using ShellProgressBar;

namespace Shazam.Cli.Commands
{
    [Command(Name = "push", Description = "Import unmanaged solution to power platform environment configured in settings.[dev].json")]
    public class PushCommand
    {
        private readonly ILogger<PushCommand> _logger;
        private readonly SolutionSettings _solutionSettings;
        private readonly CdsServiceClient _cdsServiceClient;

        public PushCommand(ICrmClientFactory crmClientFactory, ILogger<PushCommand> logger,
            IOptions<SolutionSettings> solutionSettings)
        {
            _logger = logger;
            _solutionSettings = solutionSettings.Value;
            _cdsServiceClient = crmClientFactory.Manufacture();
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

            Console.WriteLine("Importing solution {0} into Server {1}.", solutionFilePath, _cdsServiceClient.ConnectedOrgFriendlyName);

            var importSolutionRequest = new ImportSolutionRequest()
            {
                CustomizationFile = data,
                ImportJobId = importId
            };

            void Starter() => ProgressReport(importId);
            var t = new Thread(Starter);
            t.Start();

            _cdsServiceClient.Execute(importSolutionRequest);
            Console.WriteLine("Solution {0} successfully imported into {1}", solutionFilePath, _cdsServiceClient.ConnectedOrgFriendlyName);
        }

        private void ProgressReport(object importId)
        {
            var options = new ProgressBarOptions
            {
                ProgressCharacter = '_',
                ProgressBarOnBottom = false
            };
            var pbar = new ProgressBar(100, "Successfully started import job", options);
            while (true)
            {
                try
                {
                    var job = _cdsServiceClient.Retrieve("importjob", (Guid) importId, new ColumnSet("solutionname", "progress"));
                    var progress = Convert.ToDecimal(job["progress"]);

                    pbar.WriteLine($"{progress}%");
                    pbar.Tick();

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
