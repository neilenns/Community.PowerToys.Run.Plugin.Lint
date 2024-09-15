using Community.PowerToys.Run.Plugin.Lint;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;

using var factory = LoggerFactory.Create(builder => builder.AddFile("ptrun-lint.log", append: true));
var logger = factory.CreateLogger<Program>();

return await AnsiConsole.Status()
    .StartAsync("Linting", async ctx =>
    {
        logger.LogInformation("Linting: {Args}", args);

        var worker = new Worker(args, logger);

        // Events
        worker.ValidationRule += (object? sender, ValidationRuleEventArgs e) => Log($"{e.Rule.Code.ToCode()} {e.Rule.Description}");
        worker.ValidationMessage += (object? sender, ValidationMessageEventArgs e) => Log($" {"-".ToDimmed()} {e.Message}");

        try
        {
            return await worker.RunAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            logger.LogError(ex, "RunAsync failed.");
            return ex.HResult;
        }
    });

void Log(string message)
{
    AnsiConsole.MarkupLine(message);
    logger.LogInformation(Markup.Remove(message));
}
