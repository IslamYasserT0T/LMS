using Shared.Core.Contracts;
using System.Reflection;

namespace LMS.Host.Services;

public class ModuleInfo
{
    public IModule Module { get; private set; }
    public Assembly Assembly { get; private set; }

    private ModuleInfo(IModule module, Assembly assembly)
    {
        Module = module;
        Assembly = assembly;
    }

    public static bool TryCreateModule(Assembly assembly, out ModuleInfo moduleInfo)
    {

        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(IModule).IsAssignableFrom(type))
            {
                if (Activator.CreateInstance(type) is IModule module)
                {
                    moduleInfo = new ModuleInfo(module, assembly);
                    return true;
                }
            }
        }
        moduleInfo = null;
        return false;
    }
}
