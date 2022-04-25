using System.Reflection;

namespace Friday.Common.Services;

public class FridayAssemblyCollector
{
    private Assembly[] _modules;

    public IReadOnlyList<Assembly> Modules => _modules;
    public Assembly FridayAssembly { get; }

    public FridayAssemblyCollector(Assembly[] modules, Assembly fridayAssembly)
    {
        this._modules = modules;
        this.FridayAssembly = fridayAssembly;
    }
}