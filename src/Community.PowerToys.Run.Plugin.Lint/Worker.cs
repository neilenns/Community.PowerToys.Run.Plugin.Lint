using Microsoft.Extensions.Logging;

namespace Community.PowerToys.Run.Plugin.Lint;

public class Worker(string[] args, ILogger logger)
{
    // Events
    public event EventHandler<ValidationRuleEventArgs> ValidationRule;
    public event EventHandler<ValidationMessageEventArgs> ValidationMessage;

    public async Task<int> RunAsync()
    {
        var errorCount = 0;

        IRule[] rules =
        [
            new ArgsRules(args),
        ];

        if (Validate(rules))
        {
            return errorCount;
        }

        var url = args.FirstOrDefault();
        var options = url.GetGitHubOptions();

        var client = new GitHubClient(options, logger);
        var repository = await client.GetRepositoryAsync();

        rules =
        [
            new RepoRules(repository),
        ];

        if (Validate(rules))
        {
            return errorCount;
        }

        var readme = await client.GetReadmeAsync();
        var release = await client.GetLatestReleaseAsync();

        var handler = new ReleaseHandler(release, logger);
        var packages = await handler.GetPackagesAsync();

        rules =
        [
            new RepoDetailsRules(repository!),
            new ReadmeRules(readme),
            new ReleaseRules(release),
        ];

        Validate(rules);

        foreach (var package in packages)
        {
            rules =
            [
                new ReleaseNotesRules(release!, package),
                new PackageRules(package),
                new PackageContentRules(package),
                new PluginDependenciesRules(package),
                new PluginMetadataRules(package, repository!),
                new AssemblyRules(package),
            ];

            package.Load();
            Validate(rules);
            package.Dispose();
        }

        handler.Dispose();

        return errorCount;

        bool Validate(IRule[] rules)
        {
            var initialErrorCount = errorCount;
            foreach (var rule in rules)
            {
                var result = rule.Validate();
                if (result.Any())
                {
                    OnValidationRule(rule);
                    foreach (var message in result)
                    {
                        OnValidationMessage(message);
                    }

                    errorCount += result.Count();
                }
            }

            return initialErrorCount != errorCount;
        }
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
