using System.IO.Compression;
using System.Text.Json;
using Mono.Cecil;

namespace Community.PowerToys.Run.Plugin.Lint;

public class Metadata
{
    public string ID { get; set; }
    public string ActionKeyword { get; set; }
    public bool IsGlobal { get; set; }
    public string Name { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }
    public string Language { get; set; }
    public string Website { get; set; }
    public string ExecuteFileName { get; set; }
    public string IcoPathDark { get; set; }
    public string IcoPathLight { get; set; }
    public bool DynamicLoading { get; set; }
}

public sealed class Package(Asset asset, string path) : IDisposable
{
    public Asset Asset { get; } = asset;
    public FileInfo FileInfo { get; } = new FileInfo(path);
    public FileStream FileStream { get; private set; }
    public ZipArchive ZipArchive { get; private set; }
    public Metadata? Metadata { get; private set; }
    public AssemblyDefinition? AssemblyDefinition { get; private set; }

    public Package Load()
    {
        FileStream = FileInfo.OpenRead();
        ZipArchive = new ZipArchive(FileStream, ZipArchiveMode.Read);
        Metadata = GetMetadata();
        AssemblyDefinition = GetAssemblyDefinition();

        Metadata? GetMetadata()
        {
            var entry = ZipArchive.Entries.SingleOrDefault(x => x.Name == "plugin.json");
            if (entry == null) return null;
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            return JsonSerializer.Deserialize<Metadata>(content);
        }

        AssemblyDefinition? GetAssemblyDefinition()
        {
            var entry = ZipArchive.Entries.SingleOrDefault(x => x.Name == Metadata?.ExecuteFileName);
            if (entry == null) return null;
            var stream = entry.Open(); // undisposed
            var memoryStream = new MemoryStream(); // undisposed
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0; // rewind
            return AssemblyDefinition.ReadAssembly(memoryStream);
        }

        return this;
    }

    public void Dispose()
    {
        AssemblyDefinition?.Dispose();
        ZipArchive?.Dispose();
        FileStream?.Dispose();
    }

    public override string ToString() => Asset.name;
}
