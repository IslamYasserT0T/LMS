using LMS.Host.Dtos;
using System.Text.Json;

namespace LMS.Host.Services;

public class ModuleManifestsManager
{
    public void ProcessManifests(string manifestsPath, string hostManifestPath)
    {
        var manifests = LoadAllManifests(manifestsPath);
        var rootObject = new ModulesRoot { Modules = manifests };
        WriteToJsonFile(rootObject, hostManifestPath);
    }

    private static List<ModulesManifest> LoadAllManifests(string rootFolderPath)
    {
        var manifests = new List<ModulesManifest>();
        var manifestFiles = FindManifestFiles(rootFolderPath);

        foreach (var file in manifestFiles)
        {
            try
            {
                var module=ReadManifestFile(file);
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
        return Directory.GetFiles(rootFolderPath, "*.json", SearchOption.AllDirectories);
    }

    private static ModulesManifest? ReadManifestFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
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
        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(outputPath, json);
        Console.WriteLine($"Merged modules written to: {outputPath}");
    }
}
