using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Community.PowerToys.Run.Plugin.Lint;

public static partial class Extensions
{
    public static GitHubOptions GetGitHubOptions(this string? url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var result = new GitHubOptions();
        var match = GitHubRegex().Match(url);
        if (match.Success)
        {
            result.Owner = match.Groups[1].Value;
            result.Repo = match.Groups[2].Value;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(result.Owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(result.Repo);

        return result;
    }

    public static string? GetEmbeddedResourceContent(this string name)
    {
        using var stream = typeof(Extensions).Assembly.GetManifestResourceStream(typeof(Extensions).Namespace + "." + name);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static bool IsZip(this Asset asset)
    {
        return
            (asset?.content_type != null && asset.content_type.Contains("zip", StringComparison.OrdinalIgnoreCase)) ||
            (asset?.name != null && asset.name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasValidTargetFramework(this Package package)
    {
        return package.AssemblyAttributeValue(typeof(TargetFrameworkAttribute)) == ".NETCoreApp,Version=v8.0";
    }

    public static bool HasValidTargetPlatform(this Package package)
    {
        return package.AssemblyAttributeValue(typeof(TargetPlatformAttribute))?.StartsWith("Windows", StringComparison.Ordinal) == true;
    }

    private static string? AssemblyAttributeValue(this Package package, Type type)
    {
        var attribute = package?.AssemblyDefinition?.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == type.FullName);
        return attribute?.ConstructorArguments[0].Value as string;
    }

    [GeneratedRegex(@"https:\/\/github.com\/([^\/]+)\/([^\/]+)$")]
    private static partial Regex GitHubRegex();
}
