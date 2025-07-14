namespace LMS.Host.Dtos;

public class ModuleDefinition
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string CopyFolder { get; set; }
}
public class ModulesManifest
{
    public List<ModuleDefinition> Modules { get; set; }
}
