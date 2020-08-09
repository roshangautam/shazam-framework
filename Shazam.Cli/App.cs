using McMaster.Extensions.CommandLineUtils;
using Shazam.Cli.Commands;

namespace Shazam.Cli
{
    [Command(Name = "shazam", Description = "DotNet CLI Tool For Power Platform Development")]
    [HelpOption("-h|--help")]
    [Subcommand(typeof(SolutionCommand))]
    [Subcommand(typeof(HistoryCommand))]
    public class App
    {
        public void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }
    }
}
