using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
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

        var rootCommand = new RootCommand("PowerToys Run plugin linter.")
        {
            new Option<string?>(
                name: "--file",
                parseArgument: result =>
                {
                    string? filePath = result.Tokens.Single().Value;

                    if (filePath is null)
                    {
                        return null;
                    }

                    var fileInfo = new FileInfo(filePath);
                    if (!File.Exists(filePath))
                    {
                        result.ErrorMessage = $"The file {filePath} does not exist";
                        return null;
                    }
                    else
                    {
                        return fileInfo.FullName;
                    }
                },
                description: "The file to lint. Optional, if omitted the file will be downloaded from the GitHub repo."),
            new Argument<string>("url", "The GitHub repository that hosts the plugin."),
        };

        var worker = new Worker(logger);

        // Events
        worker.ValidationRule += (object? sender, ValidationRuleEventArgs e) => Log($"{e.Rule.Code.ToCode()} {e.Rule.Description}");
        worker.ValidationMessage += (object? sender, ValidationMessageEventArgs e) => Log($" {"-".ToDimmed()} {e.Message}");

        rootCommand.Handler = CommandHandler.Create<string?, string>(worker.RunAsync);

        return await rootCommand.InvokeAsync(args);
    });

void Log(string message)
{
    AnsiConsole.MarkupLine(message);
    logger.LogInformation(Markup.Remove(message));
}
