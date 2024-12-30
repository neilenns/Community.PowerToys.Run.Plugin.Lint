using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Community.PowerToys.Run.Plugin.Lint;

public static partial class Extensions
{
    [GeneratedRegex(@"^https:\/\/github\.com\/([a-zA-Z0-9._-]+)\/([a-zA-Z0-9._-]+)(?:\/)?(?:\?.*|#.*)?$")]
    public static partial Regex GitHubRegex();

    public static bool IsUrl(this string arg) =>
        Uri.TryCreate(arg, UriKind.Absolute, out Uri? uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFtp);

    public static bool IsPath(this string arg) => File.Exists(arg);

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

    public static bool IsChecksumsFile(this Asset asset)
    {
        return asset?.name != null && asset.name == "checksums.txt";
    }

    public static bool HasValidTargetFramework(this Package package)
    {
        return package.AssemblyAttributeValue(typeof(TargetFrameworkAttribute)) == ".NETCoreApp,Version=v9.0";
    }

    public static bool HasValidTargetPlatform(this Package package)
    {
        return package.AssemblyAttributeValue(typeof(TargetPlatformAttribute))?.StartsWith("Windows", StringComparison.Ordinal) == true;
    }

    public static bool HasValidAuthor(this Metadata metadata, User user)
    {
        return !string.IsNullOrEmpty(metadata?.Author) && (metadata.Author == user?.login || metadata?.Author == user?.name);
    }

    private static string? AssemblyAttributeValue(this Package package, Type type)
    {
        var attribute = package?.AssemblyDefinition?.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == type.FullName);
        return attribute?.ConstructorArguments[0].Value as string;
    }
}
