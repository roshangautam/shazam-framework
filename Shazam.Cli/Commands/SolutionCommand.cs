using System;
using System.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using NuGet.Configuration;

namespace Shazam.Cli.Commands
{
    [Command(Name = "solution", Description = "Commands to interact with a power platform solution")]
    [Subcommand(typeof(PullCommand))]
    [Subcommand(typeof(PushCommand))]
    public class SolutionCommand
    {
        private readonly SolutionSettings _solutionSettings;

        private enum Action
        {
            Pack,
            Unpack
        };

        [Option("-u|--unpack", CommandOptionType.NoValue, Description = "Unpack solution")]
        public bool unpack { get; set; }

        [Option("-pa|--pack", CommandOptionType.NoValue, Description = "Pack solution")]
        public bool pack { get; set; }

        public SolutionCommand(IOptions<SolutionSettings> solutionSettings)
        {
            _solutionSettings = solutionSettings.Value;
        }

        public void OnExecute(CommandLineApplication app)
        {
            if (unpack)
            {
                SolutionPackager(Action.Unpack);
            }
            else if (pack)
            {
                SolutionPackager(Action.Pack);
            }
            else
            {
                app.ShowHelp();
            }
        }

         private void SolutionPackager(Action action)
        {
            Console.WriteLine("Packing solution...");
            var settings = Settings.LoadDefaultSettings(null);
            var nugetPackagesDirectory = SettingsUtility.GetGlobalPackagesFolder(settings);
            var solutionPackageAction = action == Action.Pack ? "Pack" : "Extract";
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
                        $@"/action:{solutionPackageAction} {solutionPackageType} {solutionPackageMap} {zip} {solutionExtractPath} {errorLevel} {logFile}",
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
