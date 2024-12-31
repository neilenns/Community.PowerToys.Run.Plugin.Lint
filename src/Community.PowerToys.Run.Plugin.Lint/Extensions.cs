using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Community.PowerToys.Run.Plugin.Lint;

public static partial class Extensions
{
    [GeneratedRegex(@"^https:\/\/github\.com\/([a-zA-Z0-9._-]+)\/([a-zA-Z0-9._-]+)(?:\/)?(?:\?.*|#.*)?$")]
    public static partial Regex GitHubRegex();

    public static bool IsUrl(this string arg) =>
        Uri.TryCreate(arg, UriKind.Absolute, out Uri? uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeFtp);

    public static bool IsPath(this string arg) => File.Exists(arg);

    public static GitHubOptions? GetGitHubOptions(this string? url, IConfigurationRoot? config = null)
    {
        if (url == null) return null;

        var match = GitHubRegex().Match(url);
        if (!match.Success) return null;

        return new GitHubOptions
        {
            PersonalAccessToken = config?.GetValue<string>(nameof(GitHubOptions) + ":" + nameof(GitHubOptions.PersonalAccessToken)),
            Owner = match.Groups[1].Value,
            Repo = match.Groups[2].Value,
        };
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
