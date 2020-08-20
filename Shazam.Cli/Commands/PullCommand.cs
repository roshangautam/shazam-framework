using System;
using System.Diagnostics;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using NuGet.Configuration;

namespace Shazam.Cli.Commands
{
    [Command(Name = "pull", Description = "Export unmanaged solution from power platform environment and unpack it using solution packager")]
    public class PullCommand : CrmCommand
    {
        private readonly ILogger<PullCommand> _logger;
        private readonly SolutionSettings _solutionSettings;

        public PullCommand(IOrganizationService organizationService, ILogger<PullCommand> logger,
            IOptions<SolutionSettings> solutionSettings) : base(organizationService)
        {
            _logger = logger;
            _solutionSettings = solutionSettings.Value;
        }

        public void OnExecute(CommandLineApplication app)
        {
            try
            {
                ExportSolution();
                UnPackSolution();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }

            ;
        }

        private void ExportSolution()
        {
            Console.WriteLine("Exporting solution {0} to environment {1}.",
                _solutionSettings.SolutionName,
                CdsClient.ConnectedOrgFriendlyName);
            var exportSolutionRequest = new ExportSolutionRequest
                {Managed = false, SolutionName = _solutionSettings.SolutionName};
            var exportSolutionResponse = CdsClient.Execute(exportSolutionRequest) as ExportSolutionResponse;
            var solutionFilePath = $@"{_solutionSettings.SolutionExportDirectory}{_solutionSettings.SolutionName}.zip";
            if (exportSolutionResponse == null)
            {
                _logger.LogError("Failed exporting {0}.", _solutionSettings.SolutionName);
                return;
            }

            File.WriteAllBytes(solutionFilePath, exportSolutionResponse.ExportSolutionFile);
            _logger.LogDebug("Solution exported to {0}.", solutionFilePath);
        }

        private void UnPackSolution()
        {
            Console.WriteLine("Extracting solution...");
            var settings = Settings.LoadDefaultSettings(null);
            var nugetPackagesDirectory = SettingsUtility.GetGlobalPackagesFolder(settings);

            var toolsVersion = $@"{_solutionSettings.ToolsVersion}";
            var solutionPackagerFilePath =
                $@"{nugetPackagesDirectory}Microsoft.CrmSdk.CoreTools/{toolsVersion}/content/bin/coretools/SolutionPackager.exe";
            var solutionPackageType = $@"/packagetype:{_solutionSettings.SolutionPackageType}";
            var solutionPackageMap = $@"/map:{_solutionSettings.SolutionPackageMapFilePath}";
            var solutionExtractPath = $@"/folder:{_solutionSettings.SolutionExtractPath}";
            var zip = $@"/zipfile:{_solutionSettings.SolutionExportDirectory}{_solutionSettings.SolutionName}.zip";
            var errorLevel = $@"/errorlevel:{_solutionSettings.ErrorLevel}";
            var logFile = $@"/l:{_solutionSettings.SolutionExportDirectory}SolutionPackager.log";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = solutionPackagerFilePath,
                    Arguments =
                        $@"/action:Extract {solutionPackageType} {solutionPackageMap} {zip} {solutionExtractPath} {errorLevel} {logFile}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = _solutionSettings.CdsSolutionProjectPath
                }
            };
            process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit(1000 * 15);
            process.Kill();
        }
    }
}
