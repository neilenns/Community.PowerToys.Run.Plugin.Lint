using System.ComponentModel;
using Spectre.Console.Cli;

namespace Community.PowerToys.Run.Plugin.Lint;

public sealed class LintSettings : CommandSettings
{
    [Description("Path to the plugin zip file. Optional, if not provided the plugin is downloaded from the GitHubUrl release page.")]
    [CommandOption("--zipFile")]
    public string ZipFile { get; set; }

    [Description("Path to the readme file. Optional, if not provided the readme is downloaded from the GitHubUrl release page.")]
    [CommandOption("--readme")]
    public string Readme { get; set; }

    [Description("GitHub personal access token to use when reading information from GitHub. Optional.")]
    [CommandOption("--gitHubPat")]
    public string GitHubPat { get; set; }

    [Description("URL to the GitHub respository that hosts the plugin.")]
    [CommandArgument(0, "<gitHubUrl>")]
    public string GitHubUrl { get; set; }
}
