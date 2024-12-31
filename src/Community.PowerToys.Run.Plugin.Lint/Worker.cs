using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Community.PowerToys.Run.Plugin.Lint;

public class Worker(string[] args, IConfigurationRoot config, ILogger logger)
{
    private readonly JsonSerializerOptions options = new() { WriteIndented = true };

    // Events
    public event EventHandler<ValidationRuleEventArgs> ValidationRule;
    public event EventHandler<ValidationMessageEventArgs> ValidationMessage;

    private int ErrorCount { get; set; }

    public async Task<int> RunAsync()
    {
        if (Validate([new ArgsRules(args)]))
        {
            return ErrorCount;
        }

        if (args[0].IsPersonalAccessToken())
        {
            return await SavePersonalAccessTokenAsync(args);
        }
        else if (args[0].IsUrl())
        {
            return await ValidateRepositoryAsync(args);
        }
        else if (args[0].IsPath())
        {
            return await ValidatePackageAsync(args);
        }

        return ErrorCount;
    }

    private async Task<int> SavePersonalAccessTokenAsync(string[] args)
    {
        var dictionary = config.AsEnumerable()
            .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            .GroupBy(kvp => kvp.Key.Split(':')[0])
            .ToDictionary(
                group => group.Key,
                group => group
                    .Where(kvp => kvp.Key.Split(':').Length > 1)
                    .ToDictionary(
                        kvp => string.Join(':', kvp.Key.Split(':').Skip(1)),
                        kvp => kvp.Value));

        dictionary[nameof(GitHubOptions)][nameof(GitHubOptions.PersonalAccessToken)] = args[0];

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        await File.WriteAllTextAsync(path!, JsonSerializer.Serialize(dictionary, options));

        return 0;
    }

    private async Task<int> ValidateRepositoryAsync(string[] args)
    {
        var url = args[0];
        var options = url.GetGitHubOptions(config);
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
                new PluginMetadataRules(package, repository!, user!),
                new AssemblyRules(package),
            ];

            package.Load();
            Validate(rules);
            package.Dispose();
        }

        handler.Dispose();

        return ErrorCount;
    }

    private async Task<int> ValidatePackageAsync(string[] args)
    {
        var path = args[0];

        var package = new Package(path);
        package.Load();

        var url = package.Metadata?.Website;
        var options = url.GetGitHubOptions(config);
        Repository? repository = null;
        User? user = null;

        if (options != null)
        {
            var client = new GitHubClient(options, logger);
            repository = await client.GetRepositoryAsync();
            user = await client.GetUserAsync();
        }

        IRule[] rules =
        [
            new PackageRules(package),
            new PackageContentRules(package),
            new PluginDependenciesRules(package),
            new PluginMetadataRules(package, repository!, user!),
            new AssemblyRules(package),
        ];

        Validate(rules);
        package.Dispose();

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
