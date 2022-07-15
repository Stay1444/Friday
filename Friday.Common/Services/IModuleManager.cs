using Friday.Common.Entities;

namespace Friday.Common.Services;

public interface IModuleManager
{
    public IReadOnlyList<IModuleInfo> Modules { get; }
}