using Microsoft.Extensions.Logging;

namespace Community.PowerToys.Run.Plugin.Lint;

public interface IReleaseHandler
{
    Task<Package[]> GetPackagesAsync();
}

public sealed class ReleaseHandler(Release? release, ILogger logger) : IReleaseHandler, IDisposable
{
    public async Task<Package[]> GetPackagesAsync()
    {
        if (release == null)
        {
            return [];
        }

        var result = new List<Package>();
        using var client = new HttpClient();
        foreach (var asset in release.assets)
        {
            if (!asset.content_type.Contains("zip", StringComparison.OrdinalIgnoreCase)) continue;

            var path = Path.Combine(Path.GetTempPath(), asset.name);

            var response = await client.GetAsync(asset.browser_download_url);
            await using var stream = new FileStream(path, FileMode.Create);
            await response.Content.CopyToAsync(stream);

            result.Add(new Package(asset, path));
            logger.LogInformation("File downloaded: {Path}", path);
        }

        return [.. result];
    }

    public void Dispose()
    {
        if (release == null) return;

        foreach (var asset in release.assets)
        {
            var path = Path.Combine(Path.GetTempPath(), asset.name);

            if (File.Exists(path))
            {
                File.Delete(path);
                logger.LogInformation("File deleted: {Path}", path);
            }
        }
    }
}
