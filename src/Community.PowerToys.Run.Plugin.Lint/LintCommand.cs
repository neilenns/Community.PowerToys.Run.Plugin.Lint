using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Community.PowerToys.Run.Plugin.Lint;

public sealed class LintCommand : AsyncCommand<LintSettings>
{
    private readonly ILogger<LintCommand> logger;

    public LintCommand()
    {
        using var factory = LoggerFactory.Create(builder => builder.AddFile("ptrun-lint.log", append: true));
        logger = factory.CreateLogger<LintCommand>();
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] LintSettings settings)
    {
        var worker = new Worker(settings, logger);

        worker.ValidationRule += (object? sender, ValidationRuleEventArgs e) => Log($"{e.Rule.Code.ToCode()} {e.Rule.Description}");
        worker.ValidationMessage += (object? sender, ValidationMessageEventArgs e) => Log($" {"-".ToDimmed()} {e.Message}");

        try
        {
            return await AnsiConsole.Status().StartAsync("Linting", async _ => await worker.RunAsync());
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            logger.LogError(ex, "RunAsync failed.");
            return ex.HResult;
        }
    }

    private void Log(string message)
    {
        AnsiConsole.MarkupLine(message);
        logger.LogInformation(Markup.Remove(message));
    }
}
