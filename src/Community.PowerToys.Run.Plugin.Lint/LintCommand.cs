using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Community.PowerToys.Run.Plugin.Lint;

internal sealed class LintCommand : AsyncCommand<LintCommand.Settings>
{
    public override Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        AnsiConsole.WriteLine("Linting plugin...");
        return Task.FromResult(0);
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Path to the plugin zip file. Optional, if not provided the plugin is downloaded from the GitHubUrl release page.")]
        [CommandOption("--zipFile")]
        public string ZipFile { get; set; }

        [Description("GitHub personal access token to use when reading information from GitHub. Optional.")]
        [CommandOption("--gitHubPat")]
        public string GitHubPat { get; set; }

        [Description("URL to the GitHub respository that hosts the plugin.")]
        [CommandArgument(0, "gitHubUrl")]
        public string GitHubUrl { get; set; }
    }
}
