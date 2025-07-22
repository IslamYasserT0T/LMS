using LMS.Host.Dtos;
using System.Text.Json;

namespace LMS.Host.Services;

public class ModuleFolderCopier
{
    public static async Task CopyFoldersFromManifestAsync(string manifestPath, string destinationRoot)
    {
        if (!File.Exists(manifestPath))
            return;

        var json = await File.ReadAllTextAsync(manifestPath);
        var manifest = JsonSerializer.Deserialize<ModulesRoot>(json);

        if (manifest?.Modules == null)
            return;

        foreach (var module in manifest.Modules)
        {
            foreach(var item in module.Modules)
            {

                if (string.IsNullOrWhiteSpace(item.CopyFolder))
                    continue;
                var root = Directory.GetCurrentDirectory();
                string sourceFolder = Path.Combine(Directory.GetParent(root).Parent.FullName, item.CopyFolder.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(sourceFolder))
                    continue;

                var destinationFolder = Path.Combine(destinationRoot, item.Name);

                CopyDirectory(sourceFolder, destinationFolder);
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
         Directory.CreateDirectory(destinationDir);

        foreach (var filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetFileName(filePath);
            var destinationPath = Path.Combine(destinationDir, relativePath);

            File.Copy(filePath, destinationPath, overwrite: true);

        }
    }
}
