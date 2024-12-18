using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Spectre.Console;

namespace Community.PowerToys.Run.Plugin.Lint;

public interface IRule
{
    int Id { get; }
    string Code => $"PTRUN{Id.ToString("D4", CultureInfo.InvariantCulture)}";
    string Description { get; }
    IEnumerable<string> Validate();
}

public static class RuleStyle
{
    public static Style Code => Color.Red;

    public static Style Filename => new(Color.Yellow, Color.Default, Decoration.Bold);

    public static Style Dependency => Color.Green;

    public static Style Quote => Color.Blue;

    public static Style Dimmed => Color.Grey;

    public static string ToCode(this string value) => $"[{Code.ToMarkup()}]{value}[/]{":".ToDimmed()}";

    public static string ToFilename(this string value) => $"{"(".ToDimmed()}[{Filename.ToMarkup()}]{value}[/]{")".ToDimmed()}";

    public static string ToDependency(this string value) => $"[{Dependency.ToMarkup()}]{value}[/]";

    public static string ToQuote(this string value) => $"{"\"".ToDimmed()}[{Quote.ToMarkup()}]{value}[/]{"\"".ToDimmed()}";

    public static string ToDimmed(this string value) => $"[{Dimmed.ToMarkup()}]{value}[/]";
}

public class ArgsRules(string[] args) : IRule
{
    public int Id => 0001;
    public string Description => "Args should be valid";

    public IEnumerable<string> Validate()
    {
        if (args == null || args.Length == 0)
        {
            yield return "Args missing";
            yield break;
        }

        if (!Extensions.GitHubRegex().IsMatch(args[0])) yield return "GitHub repo URL missing";
    }
}

public class RepoRules(Repository? repository) : IRule
{
    public int Id => 1001;
    public string Description => "Repo should be valid";

    public IEnumerable<string> Validate()
    {
        if (repository == null)
        {
            yield return "Repository missing";
            yield break;
        }
    }
}

public class RepoDetailsRules(Repository repository) : IRule
{
    public int Id => 1002;
    public string Description => "Repo details should be valid";

    public IEnumerable<string> Validate()
    {
        if (repository == null)
        {
            yield return "Repository missing";
            yield break;
        }

        if (repository.topics?.Contains("powertoys-run-plugin") != true) yield return $"Topic {"powertoys-run-plugin".ToQuote()} missing";
        if (repository.license?.name == null) yield return "License missing";
    }
}

public class ReadmeRules(Readme? readme) : IRule
{
    public int Id => 1101;
    public string Description => "Readme should be valid";

    public IEnumerable<string> Validate()
    {
        if (readme == null)
        {
            yield return "Readme missing";
            yield break;
        }

        var content = Decode(readme.content);
        if (!content.Contains("installation", StringComparison.InvariantCultureIgnoreCase)) yield return "Installation instructions missing";
        if (!content.Contains("usage", StringComparison.InvariantCultureIgnoreCase)) yield return "Usage instructions missing";

        static string Decode(string value) => value != null ? Encoding.UTF8.GetString(Convert.FromBase64String(value)) : string.Empty;
    }
}

public class ReleaseRules(Release? release) : IRule
{
    public int Id => 1201;
    public string Description => "Release should be valid";

    public IEnumerable<string> Validate()
    {
        if (release == null)
        {
            yield return "Release missing";
            yield break;
        }

        if (release.assets == null)
        {
            yield return "Asset missing";
            yield break;
        }

        if (!release.assets.Any(x => x.IsZip()))
        {
            yield return $"Asset {".zip".ToQuote()} missing";
            yield break;
        }

        if (!release.assets.Any(x => x.name.Contains("arm64", StringComparison.OrdinalIgnoreCase))) yield return $"Asset {"arm64".ToQuote()} platform missing";
        if (!release.assets.Any(x => x.name.Contains("x64", StringComparison.OrdinalIgnoreCase))) yield return $"Asset {"x64".ToQuote()} platform missing";
    }
}

public class ReleaseNotesRules(Release release, Package package) : IRule
{
    public int Id => 1202;
    public string Description => $"Release notes should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (release?.body == null)
        {
            yield return "Release notes missing";
            yield break;
        }

        if (package?.FileStream == null)
        {
            yield return "Package missing";
            yield break;
        }
    }
}

public partial class PackageRules(Package package) : IRule
{
    public int Id => 1301;
    public string Description => $"Package should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (package?.FileInfo == null)
        {
            yield return "Package missing";
            yield break;
        }

        if (!PackageRegex().IsMatch(package.FileInfo.Name)) yield return $"Filename does not match {"<name>-<version>-<platform>.zip".ToQuote()} convention";
    }

    [GeneratedRegex(@"^(?<name>[\w\.]+)-(?<version>\d+\.\d+\.\d+)-(?<platform>(?i:arm64|x64))\.zip$")]
    private static partial Regex PackageRegex();
}

public class PackageContentRules(Package package) : IRule
{
    public int Id => 1302;
    public string Description => $"Package content should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (package?.ZipArchive == null)
        {
            yield return "Package missing";
            yield break;
        }

        var folders = RootFolders();
        if (folders.Length != 1) yield return "Plugin folder missing";

        var files = Files();
        if (!files.Contains("plugin.json")) yield return $"Metadata {"plugin.json".ToQuote()} missing";
        if (!files.Any(x => x.EndsWith(".dll", StringComparison.Ordinal))) yield return $"Assembly {".dll".ToQuote()} missing";

        string[] RootFolders() => [.. package.ZipArchive.Entries.Select(x => x.FullName.Split('\\', '/')[0]).Distinct()];
        string[] Files() => [.. package.ZipArchive.Entries.Select(x => x.Name)];
    }
}

public class PackageChecksumRules(Release release, Package package, Checksum[] checksums) : IRule
{
    public int Id => 1303;
    public string Description => $"Package checksum should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (release?.body == null)
        {
            yield return "Release notes missing";
            yield break;
        }

        if (package?.FileStream == null)
        {
            yield return "Package missing";
            yield break;
        }

        if (package?.Asset?.name == null)
        {
            yield return "Package missing";
            yield break;
        }

        var hash = Hash();
        var validReleaseNotes = release.body.Contains(hash, StringComparison.OrdinalIgnoreCase);
        var validChecksumsFile = checksums?.Any(x => x.Hash.Equals(hash, StringComparison.OrdinalIgnoreCase) && x.Name.Contains(package.Asset.name, StringComparison.OrdinalIgnoreCase)) == true;

        if (!validReleaseNotes && !validChecksumsFile) yield return $"Hash {hash.ToQuote()} missing";

        string Hash()
        {
            using var algorithm = SHA256.Create();
            package.FileStream.Position = 0; // rewind
            return Convert.ToHexString(algorithm.ComputeHash(package.FileStream));
        }
    }
}

public partial class PluginMetadataRules(Package package, Repository repository, User? user) : IRule
{
    public int Id => 1401;
    public string Description => $"Plugin metadata should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (package?.Metadata == null)
        {
            yield return "Package missing";
            yield break;
        }

        if (repository == null)
        {
            yield return "Repository missing";
            yield break;
        }

        if (user == null)
        {
            yield return "User missing";
            yield break;
        }

        var metadata = package.Metadata;
        string[] actionKeyword = ["=", "?", "!!", ".", "o:", ":", "!", ">", ")", "%%", "#", "//", "{", "??", "$", "_", "<",];

        if (!Guid.TryParseExact(metadata.ID, "N", out Guid _)) yield return "ID is invalid";
        if (actionKeyword.Contains(metadata.ActionKeyword)) yield return "ActionKeyword is not unique";
        if (metadata.Name != RootFolder()) yield return "Name does not match plugin folder";
        if (!metadata.HasValidAuthor(user)) yield return "Author does not match GitHub user";
        if (!Version.TryParse(metadata.Version, out Version? _)) yield return "Version is invalid";
        if (metadata.Version != GetFilenameVersion()) yield return "Version does not match filename version";
        if (metadata.Website != repository.html_url) yield return "Website does not match repo URL";
        if (!Exists(metadata.ExecuteFileName)) yield return "ExecuteFileName missing in package";
        if (!AssemblyRegex().IsMatch(package.Metadata.ExecuteFileName)) yield return $"ExecuteFileName does not match {"Community.PowerToys.Run.Plugin.<Name>.dll".ToQuote()} convention";
        if (!Exists(metadata.IcoPathDark)) yield return "IcoPathDark missing in package";
        if (!Exists(metadata.IcoPathLight)) yield return "IcoPathLight missing in package";
        if (DynamicLoadingUnnecessary(metadata.DynamicLoading)) yield return "DynamicLoading is unnecessary";

        string? RootFolder() => package.ZipArchive.Entries.Select(x => x.FullName.Split('\\', '/')[0]).Distinct().FirstOrDefault();
        string? GetFilenameVersion() => FilenameVersionRegex().Match(package.FileInfo.Name).Value;
        bool Exists(string path) => package.ZipArchive.Entries.Any(x => NormalizePath(x.FullName).EndsWith(NormalizePath(path), StringComparison.Ordinal));
        bool DynamicLoadingUnnecessary(bool enabled) => enabled && package.ZipArchive.Entries.Count(x => x.Name.EndsWith(".dll", StringComparison.Ordinal)) == 1;
        string NormalizePath(string path) => path.Replace('\\', '/');
    }

    [GeneratedRegex(@"(\d+\.\d+\.\d+)")]
    private static partial Regex FilenameVersionRegex();

    [GeneratedRegex(@"^Community\.PowerToys\.Run\.Plugin\.(.+)\.dll$")]
    private static partial Regex AssemblyRegex();
}

public class PluginDependenciesRules(Package package) : IRule
{
    public int Id => 1402;
    public string Description => $"Package dependencies should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (package?.ZipArchive == null)
        {
            yield return "Package missing";
            yield break;
        }

        var files = Files();
        foreach (var dependency in PowerToysRunDependencies())
        {
            if (files.Contains(dependency)) yield return $"Unnecessary dependency: {dependency.ToDependency()}";
        }

        if (files.Contains("Newtonsoft.Json.dll")) yield return $"Unnecessary dependency: {"Newtonsoft.Json".ToDependency()}, consider using {"System.Text.Json".ToDependency()}";

        foreach (var package in PowerToysPackages())
        {
            if (files.Contains($"{package}.dll")) yield return $"Unnecessary dependency: {package.ToDependency()}, already defined in Central Package Management {"Directory.Packages.props".ToFilename()}";
        }

        string[] Files() => [.. package.ZipArchive.Entries.Select(x => x.Name)];
        string[] PowerToysRunDependencies() => ["PowerToys.Common.UI.dll", "PowerToys.ManagedCommon.dll", "PowerToys.Settings.UI.Lib.dll", "Wox.Infrastructure.dll", "Wox.Plugin.dll"];
        string[] PowerToysPackages()
        {
            var content = "Directory.Packages.props.xml".GetEmbeddedResourceContent();
            if (content == null) return [];
            return XDocument.Parse(content)
                .Descendants("PackageVersion")
                .Select(x => x.Attribute("Include")?.Value)
                .ToArray()!;
        }
    }
}

public class AssemblyRules(Package package) : IRule
{
    public int Id => 1501;
    public string Description => $"Plugin assembly should be valid {package.ToString().ToFilename()}";

    public IEnumerable<string> Validate()
    {
        if (package?.AssemblyDefinition == null)
        {
            yield return "Assembly could not be validated";
            yield break;
        }

        if (!package.HasValidTargetFramework()) yield return $"Target framework should be {"net9.0".ToQuote()}";
        if (!package.HasValidTargetPlatform()) yield return $"Target platform should be {"windows".ToQuote()}";

        var pluginId = PluginID(MainTypeDefinition());
        if (pluginId != package.Metadata?.ID) yield return $"Main.PluginID does not match metadata {"plugin.json".ToFilename()} ID";

        TypeDefinition? MainTypeDefinition()
        {
            foreach (var module in package.AssemblyDefinition.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (!type.IsClass) continue;

                    foreach (var abstraction in type.Interfaces)
                    {
                        if (abstraction.InterfaceType.Name == "IPlugin")
                        {
                            return type;
                        }
                    }
                }
            }

            return null;
        }

        string? PluginID(TypeDefinition? type)
        {
            if (type == null) return null;

            foreach (var property in type.Properties)
            {
                if (property.Name == "PluginID" && property.PropertyType.FullName == "System.String" && property.GetMethod.IsStatic)
                {
                    var method = property.GetMethod;

                    if (method.HasBody)
                    {
                        foreach (var instruction in method.Body.Instructions)
                        {
                            if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand is string value && !string.IsNullOrEmpty(value))
                            {
                                return value;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
