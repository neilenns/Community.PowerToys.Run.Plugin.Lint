using Community.PowerToys.Run.Plugin.Lint;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Community.PowerToys.Run.Plugin.Lint;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<LintCommand>();

        return await app.RunAsync(args);
    }
}
