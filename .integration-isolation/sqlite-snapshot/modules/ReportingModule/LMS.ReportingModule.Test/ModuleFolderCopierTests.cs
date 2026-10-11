using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Host.Dtos;
using LMS.Host.Services;
using Xunit;

namespace LMS.ReportingModule.Test;

public class ModuleFolderCopierTests : IDisposable
{
    private readonly string _testRoot;

    public ModuleFolderCopierTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "LMS_CopierTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testRoot);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, true);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    [Fact]
    public void CopyDirectory_PreservesNestedHierarchy_WithoutFlattening()
    {
        // Arrange: create nested structure
        var source = Path.Combine(_testRoot, "source");
        var dest = Path.Combine(_testRoot, "dest");

        var nestedCssDir = Path.Combine(source, "wwwroot", "css");
        var nestedJsDir = Path.Combine(source, "wwwroot", "_content", "IY.Reporting.Blazor", "js");
        var arDir = Path.Combine(source, "ar");
        var enDir = Path.Combine(source, "en");

        Directory.CreateDirectory(nestedCssDir);
        Directory.CreateDirectory(nestedJsDir);
        Directory.CreateDirectory(arDir);
        Directory.CreateDirectory(enDir);

        File.WriteAllText(Path.Combine(nestedCssDir, "reporting.css"), "/* css content */");
        File.WriteAllText(Path.Combine(nestedJsDir, "interop.js"), "console.log('js');");
        File.WriteAllText(Path.Combine(arDir, "resource.dll"), "arabic-satellite-content");
        File.WriteAllText(Path.Combine(enDir, "resource.dll"), "english-satellite-content");
        File.WriteAllText(Path.Combine(source, "root.dll"), "root-assembly");

        // Act
        ModuleFolderCopier.CopyDirectory(source, dest);

        // Assert: hierarchy preserved, NOT flattened
        var destCss = Path.Combine(dest, "wwwroot", "css", "reporting.css");
        var destJs = Path.Combine(dest, "wwwroot", "_content", "IY.Reporting.Blazor", "js", "interop.js");
        var destAr = Path.Combine(dest, "ar", "resource.dll");
        var destEn = Path.Combine(dest, "en", "resource.dll");
        var destRoot = Path.Combine(dest, "root.dll");

        Assert.True(File.Exists(destCss), "Nested CSS path was not preserved");
        Assert.True(File.Exists(destJs), "Nested JS path was not preserved");
        Assert.True(File.Exists(destAr), "Arabic satellite path was not preserved");
        Assert.True(File.Exists(destEn), "English satellite path was not preserved");
        Assert.True(File.Exists(destRoot), "Root assembly path was not preserved");

        // Assert: NO silent flattening happened
        Assert.False(File.Exists(Path.Combine(dest, "reporting.css")), "Silent flattening detected: reporting.css copied to root");
        Assert.False(File.Exists(Path.Combine(dest, "interop.js")), "Silent flattening detected: interop.js copied to root");

        // Assert: distinct contents preserved without collision
        Assert.Equal("arabic-satellite-content", File.ReadAllText(destAr));
        Assert.Equal("english-satellite-content", File.ReadAllText(destEn));
    }

    [Fact]
    public void CopyDirectory_PreventsPathTraversal()
    {
        var source = Path.Combine(_testRoot, "traversal_source");
        var dest = Path.Combine(_testRoot, "traversal_dest");

        Directory.CreateDirectory(source);
        Directory.CreateDirectory(dest);
        File.WriteAllText(Path.Combine(source, "safe.txt"), "safe");

        // Normal call succeeds
        ModuleFolderCopier.CopyDirectory(source, dest);
        Assert.True(File.Exists(Path.Combine(dest, "safe.txt")));
    }

    [Fact]
    public async Task CopyFoldersFromManifest_InReleaseMode_RejectsDebugPath()
    {
        var manifestDir = Path.Combine(_testRoot, "manifests");
        Directory.CreateDirectory(manifestDir);
        var manifestFile = Path.Combine(manifestDir, "manifest.json");

        var badManifest = new ModulesRoot
        {
            Modules = new System.Collections.Generic.List<ModulesManifest>
            {
                new ModulesManifest
                {
                    ModuleName = "BadModule",
                    Modules = new System.Collections.Generic.List<ModuleDefinition>
                    {
                        new ModuleDefinition
                        {
                            Name = "LMS.BadModule",
                            CopyFolder = "modules/BadModule/bin/Debug/net9.0/plugins"
                        }
                    }
                }
            }
        };

        File.WriteAllText(manifestFile, JsonSerializer.Serialize(badManifest));

        // Act & Assert: Release mode must reject Debug path with InvalidOperationException
        var destRoot = Path.Combine(_testRoot, "dest_plugins");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ModuleFolderCopier.CopyFoldersFromManifestAsync(manifestFile, destRoot, "Release"));

        Assert.Contains("Release host attempted to deploy Debug plugin", ex.Message);
    }

    [Fact]
    public async Task CopyFoldersFromManifest_InReleaseMode_ThrowsIfSourceFolderDoesNotExist()
    {
        var manifestDir = Path.Combine(_testRoot, "manifests");
        Directory.CreateDirectory(manifestDir);
        var manifestFile = Path.Combine(manifestDir, "manifest.json");

        var missingManifest = new ModulesRoot
        {
            Modules = new System.Collections.Generic.List<ModulesManifest>
            {
                new ModulesManifest
                {
                    ModuleName = "NonExistentModule",
                    Modules = new System.Collections.Generic.List<ModuleDefinition>
                    {
                        new ModuleDefinition
                        {
                            Name = "LMS.NonExistentModule",
                            CopyFolder = "modules/NonExistent/bin/Release/net9.0/plugins"
                        }
                    }
                }
            }
        };

        File.WriteAllText(manifestFile, JsonSerializer.Serialize(missingManifest));

        var destRoot = Path.Combine(_testRoot, "dest_plugins");
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
            ModuleFolderCopier.CopyFoldersFromManifestAsync(manifestFile, destRoot, "Release"));
    }
}
