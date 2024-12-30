using Microsoft.Extensions.Logging;

namespace Community.PowerToys.Run.Plugin.Lint;

public class Worker(string[] args, ILogger logger)
{
    // Events
    public event EventHandler<ValidationRuleEventArgs> ValidationRule;
    public event EventHandler<ValidationMessageEventArgs> ValidationMessage;

    private int ErrorCount { get; set; }

    public async Task<int> RunAsync()
    {
        IRule[] rules =
        [
            new ArgsRules(args),
        ];

        if (Validate(rules))
        {
            return ErrorCount;
        }

        if (args[0].IsUrl())
        {
            return await ValidateRepositoryAsync(args);
        }

        return ErrorCount;
    }

    private async Task<int> ValidateRepositoryAsync(string[] args)
    {
        var url = args.FirstOrDefault();
        var options = url.GetGitHubOptions();

        var client = new GitHubClient(options, logger);
        var repository = await client.GetRepositoryAsync();

        IRule[] rules =
        [
            new RepoRules(repository),
        ];

        if (Validate(rules))
        {
            return ErrorCount;
        }

        var readme = await client.GetReadmeAsync();
        var release = await client.GetLatestReleaseAsync();

        rules =
        [
            new RepoDetailsRules(repository!),
            new ReadmeRules(readme),
            new ReleaseRules(release),
        ];

        Validate(rules);

        var handler = new ReleaseHandler(release, logger);
        var packages = await handler.GetPackagesAsync();
        var checksums = await handler.GetChecksumsAsync();
        var user = await client.GetUserAsync();

        foreach (var package in packages)
        {
            rules =
            [
                new ReleaseNotesRules(release!, package),
                new PackageRules(package),
                new PackageContentRules(package),
                new PackageChecksumRules(release!, package, checksums),
                new PluginDependenciesRules(package),
                new PluginMetadataRules(package, repository!, user),
                new AssemblyRules(package),
            ];

            package.Load();
            Validate(rules);
            package.Dispose();
        }

        handler.Dispose();

        return ErrorCount;
    }

    private bool Validate(IRule[] rules)
    {
        var initialErrorCount = ErrorCount;
        foreach (var rule in rules)
        {
            var result = rule.Validate().ToArray();
            if (result.Length != 0)
            {
                OnValidationRule(rule);
                foreach (var message in result)
                {
                    OnValidationMessage(message);
                }

                ErrorCount += result.Length;
            }
        }

        return initialErrorCount != ErrorCount;
    }

    // Events
    private void OnValidationRule(IRule rule) => ValidationRule?.Invoke(this, new ValidationRuleEventArgs(rule));
    private void OnValidationMessage(string message) => ValidationMessage?.Invoke(this, new ValidationMessageEventArgs(message));
}

// Events
public class ValidationRuleEventArgs(IRule rule) : EventArgs
{
    public IRule Rule { get; } = rule;
}

public class ValidationMessageEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}
