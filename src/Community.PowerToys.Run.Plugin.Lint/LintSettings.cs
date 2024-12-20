using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Community.PowerToys.Run.Plugin.Lint;

public sealed class LintSettings : CommandSettings
{
    [Description("Path to the plugin zip file. Optional, if not provided the plugin is downloaded from the GitHubUrl release page.")]
    [CommandOption("--zipFile")]
    public string ZipFile { get; set; }

    [Description("Path to the readme file. Optional, if not provided the readme is downloaded from the GitHubUrl release page.")]
    [CommandOption("--readmeFile")]
    public string ReadmeFile { get; set; }

    [Description("GitHub personal access token to use when reading information from GitHub. Optional.")]
    [CommandOption("--gitHubPat")]
    public string GitHubPat { get; set; }

    [Description("URL to the GitHub respository that hosts the plugin.")]
    [CommandArgument(0, "<gitHubUrl>")]
    public string GitHubUrl { get; set; }

    public override ValidationResult Validate()
    {
        // If --zipFile or --readme are specified then both must be specified
        if ((ZipFile is null) != (ReadmeFile is null))
        {
            return ValidationResult.Error("If --zipFile or --readme is specified then both must be specified.");
        }

        // If --zipFile is specified then the file must exist on disk
        if (ZipFile is not null && !File.Exists(ZipFile))
        {
            return ValidationResult.Error($"The file '{ZipFile}' does not exist.");
        }

        // If --readmeFile is specified then the file must exist on disk
        if (ReadmeFile is not null && !File.Exists(ReadmeFile))
        {
            return ValidationResult.Error($"The file '{ReadmeFile}' does not exist.");
        }

        return ValidationResult.Success();
    }
}
