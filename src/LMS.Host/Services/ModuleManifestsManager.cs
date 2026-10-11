using LMS.Host.Dtos;
using System.Text.Json;

namespace LMS.Host.Services;

public class ModuleManifestsManager
{
    public void ProcessManifests(string manifestsPath, string hostManifestPath, string? configuration = null)
    {
#if DEBUG
        var activeConfig = configuration ?? "Debug";
#else
        var activeConfig = configuration ?? "Release";
#endif
        var manifests = LoadAllManifests(manifestsPath, activeConfig);
        var rootObject = new ModulesRoot { Modules = manifests };
        WriteToJsonFile(rootObject, hostManifestPath);
    }

    private static List<ModulesManifest> LoadAllManifests(string rootFolderPath, string configuration)
    {
        var manifests = new List<ModulesManifest>();
        var manifestFiles = FindManifestFiles(rootFolderPath);

        foreach (var file in manifestFiles)
        {
            try
            {
                var module = ReadManifestFile(file, configuration);
                if (module != null)
                    manifests.Add(module);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file '{file}': {ex.Message}");
            }
        }

        return manifests;
    }

    private static IEnumerable<string> FindManifestFiles(string rootFolderPath)
    {
        if (!Directory.Exists(rootFolderPath))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(rootFolderPath, "*.json", SearchOption.AllDirectories);
    }

    private static ModulesManifest? ReadManifestFile(string filePath, string configuration)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            json = json.Replace("{Configuration}", configuration, StringComparison.OrdinalIgnoreCase)
                       .Replace("$(Configuration)", configuration, StringComparison.OrdinalIgnoreCase);

            return JsonSerializer.Deserialize<ModulesManifest>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
            return null;
        }
    }

    private static void WriteToJsonFile(ModulesRoot root, string outputPath)
    {
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(outputPath, json);
        Console.WriteLine($"Merged modules written to: {outputPath}");
    }
}
