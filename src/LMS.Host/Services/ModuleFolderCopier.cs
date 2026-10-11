using LMS.Host.Dtos;
using System.Text.Json;

namespace LMS.Host.Services;

public class ModuleFolderCopier
{
    public static async Task CopyFoldersFromManifestAsync(string manifestPath, string destinationRoot, string? configuration = null)
    {
#if DEBUG
        var activeConfig = configuration ?? "Debug";
#else
        var activeConfig = configuration ?? "Release";
#endif

        if (!File.Exists(manifestPath))
            return;

        var json = await File.ReadAllTextAsync(manifestPath);
        var manifest = JsonSerializer.Deserialize<ModulesRoot>(json);

        if (manifest?.Modules == null)
            return;

        var curDir = Directory.GetCurrentDirectory();
        var dirInfo = new DirectoryInfo(curDir);
        while (dirInfo != null && !File.Exists(Path.Combine(dirInfo.FullName, "LMS.sln")))
        {
            dirInfo = dirInfo.Parent;
        }
        var solutionRoot = dirInfo?.FullName ?? curDir;

        foreach (var module in manifest.Modules)
        {
            foreach (var item in module.Modules)
            {
                if (string.IsNullOrWhiteSpace(item.CopyFolder))
                    continue;

                var resolvedCopyFolder = item.CopyFolder
                    .Replace("{Configuration}", activeConfig, StringComparison.OrdinalIgnoreCase)
                    .Replace("$(Configuration)", activeConfig, StringComparison.OrdinalIgnoreCase);

                // Validation: In Release mode, never allow deploying a Debug plugin!
                if (string.Equals(activeConfig, "Release", StringComparison.OrdinalIgnoreCase))
                {
                    if (resolvedCopyFolder.Contains("Debug", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Release host attempted to deploy Debug plugin: {resolvedCopyFolder}");
                    }
                }

                string rawSource = Path.Combine(solutionRoot, resolvedCopyFolder.Replace('/', Path.DirectorySeparatorChar));
                string sourceFolder = Path.GetFullPath(rawSource);

                if (!Directory.Exists(sourceFolder))
                {
                    if (string.Equals(activeConfig, "Release", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DirectoryNotFoundException($"Release plugin source directory does not exist: {sourceFolder}");
                    }
                    continue;
                }

                // If sourceFolder contains a child folder with item.Name, copy from that child folder
                var moduleSourceDir = Directory.Exists(Path.Combine(sourceFolder, item.Name))
                    ? Path.Combine(sourceFolder, item.Name)
                    : sourceFolder;

                var destinationFolder = Path.Combine(destinationRoot, item.Name);
                CopyDirectory(moduleSourceDir, destinationFolder);
            }
        }
    }

    public static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var fullSourceDir = Path.GetFullPath(sourceDir);
        var fullDestDir = Path.GetFullPath(destinationDir);

        if (!Directory.Exists(fullSourceDir))
            return;

        Directory.CreateDirectory(fullDestDir);

        foreach (var filePath in Directory.GetFiles(fullSourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(fullSourceDir, filePath);

            // Path traversal protection
            var destinationPath = Path.GetFullPath(Path.Combine(fullDestDir, relativePath));
            if (!destinationPath.StartsWith(fullDestDir, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Path traversal detected in module copy: {relativePath}");
            }

            var destFileDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destFileDir) && !Directory.Exists(destFileDir))
            {
                Directory.CreateDirectory(destFileDir);
            }

            File.Copy(filePath, destinationPath, overwrite: true);
        }
    }
}
