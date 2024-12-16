using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Community.PowerToys.Run.Plugin.Lint;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;

using var factory = LoggerFactory.Create(builder => builder.AddFile("ptrun-lint.log", append: true));
var logger = factory.CreateLogger<Program>();

return await AnsiConsole.Status()
    .StartAsync("Linting", async _ =>
    {
        logger.LogInformation("Linting: {Args}", args);

        var zipFileOption = new Option<string?>(
                name: "--zipFile",
                parseArgument: ValidateFile,
                description: "The file to lint. Optional, if omitted the file will be downloaded from the GitHub repo.");

        var readmeFileOption = new Option<string?>(
            name: "--readmeFile",
            parseArgument: ValidateFile,
            description: "The readme to lint. Optional, if omitted the file will be downloaded from the GitHub repo.");

        var personalAccessToken = new Option<string?>(
            name: "--personalAccessToken",
            description: "A GitHub personal access token. Optional.");

        var rootCommand = new RootCommand("PowerToys Run plugin linter.")
        {
            zipFileOption,
            readmeFileOption,
            personalAccessToken,
            new Argument<string>("gitHubUrl", "The GitHub repository that hosts the plugin."),
        };

        var worker = new Worker(logger);

        // Events
        worker.ValidationRule += (object? sender, ValidationRuleEventArgs e) => Log($"{e.Rule.Code.ToCode()} {e.Rule.Description}");
        worker.ValidationMessage += (object? sender, ValidationMessageEventArgs e) => Log($" {"-".ToDimmed()} {e.Message}");

        rootCommand.Handler = CommandHandler.Create<Arguments>(worker.RunAsync);
        rootCommand.AddValidator(result =>
        {
            var zipFile = result.GetValueForOption(zipFileOption);
            var readmeFile = result.GetValueForOption(readmeFileOption);

            // XOR logic: one is null, the other isn't
            if ((zipFile is null) != (readmeFile is null))
            {
                result.ErrorMessage = "Both --zipFile and --readmeFile must be specified together.";
            }
        });

        return await rootCommand.InvokeAsync(args);
    });

void Log(string message)
{
    AnsiConsole.MarkupLine(message);
    logger.LogInformation(Markup.Remove(message));
}

// Reusable validation function
string? ValidateFile(ArgumentResult result)
{
    var filePath = result.Tokens.SingleOrDefault()?.Value;

    if (string.IsNullOrEmpty(filePath))
    {
        return null; // Optional file argument, no validation needed
    }

    if (!File.Exists(filePath))
    {
        result.ErrorMessage = $"The file '{filePath}' does not exist.";
        return null;
    }

    return Path.GetFullPath(filePath);
}
