using System;
using System.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using NuGet.Configuration;
using Shazam.Framework;

namespace Shazam.Cli.Commands
{
    [Command(Name = "generate", Description = "Generate early bound entity classes and optionset enumerations")]
    public class GenerateCommand
    {
        private readonly ILogger<GenerateCommand> _logger;
        private readonly AuthSettings _authSettings;

        public GenerateCommand(ILogger<GenerateCommand> logger, IOptions<AuthSettings> authSettings)
        {
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public void OnExecute(CommandLineApplication app)
        {
            try
            {
                Generate();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private void Generate()
        {
            Console.WriteLine("Generating early bound objects...");

            var authType = _authSettings.AuthType;
            var url = _authSettings.Url;
            var clientId = _authSettings.ClientId;
            var clientSecret = _authSettings.ClientSecret;
            var connectionString = $"AuthType={authType};url={url};ClientId={clientId};ClientSecret={clientSecret}";

            var settings = Settings.LoadDefaultSettings(null);
            var nugetPackagesDirectory = SettingsUtility.GetGlobalPackagesFolder(settings);
            const string toolsVersion = "9.1.0.49";
            var solutionPackagerFilePath =
                $@"{nugetPackagesDirectory}Microsoft.CrmSdk.CoreTools/{toolsVersion}/content/bin/coretools/CrmSvcUtil.exe";
            var connectionParameter = $@"/connectionstring:{connectionString}";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = solutionPackagerFilePath,
                    Arguments = $@"{connectionParameter} /out:Generated.cs /namespace:LinkedIn.Internal.Crm.Common.Generated",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            Console.WriteLine("Generating...");
            process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Kill();
        }
    }
}
