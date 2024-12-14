using Microsoft.Extensions.Logging;

namespace Community.PowerToys.Run.Plugin.Lint;

public interface IReleaseHandler
{
    Task<Package[]> GetPackagesAsync();
    Task<Checksum[]> GetChecksumsAsync();
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
            if (!asset.IsZip()) continue;

            var path = Path.Combine(Path.GetTempPath(), asset.name);

            var response = await client.GetAsync(asset.browser_download_url);
            await using var stream = new FileStream(path, FileMode.Create);
            await response.Content.CopyToAsync(stream);

            result.Add(new Package(asset, path));
            logger.LogInformation("File downloaded: {Path}", path);
        }

        return [.. result];
    }

    public async Task<Checksum[]> GetChecksumsAsync()
    {
        if (release == null)
        {
            return [];
        }

        var result = new List<Checksum>();
        using var client = new HttpClient();

        var asset = release.assets.FirstOrDefault(x => x.IsChecksumsFile());

        if (asset == null)
        {
            return [];
        }

        var content = await client.GetStringAsync(asset.browser_download_url);

        foreach (var line in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var tokens = line.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 2)
            {
                result.Add(new Checksum(tokens[0], tokens[1]));
            }
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
