using System.Reflection;
using Friday.Common.Enums;

namespace Friday.Common.Entities;

public class Resource
{
    private static readonly Dictionary<string, Resource> ResourcesCache = new Dictionary<string, Resource>();

    private readonly string _path;
    private readonly Assembly _assembly;
    private Resource(string path, Assembly assembly)
    {
        this._path = path;
        this._assembly = assembly;
    }

    
    
    public Stream GetStream()
    {
        return _assembly.GetManifestResourceStream(_path)!;
    }
    
    public string ReadString()
    {
        using var stream = GetStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    public async Task<string> ReadStringAsync()
    {
        await using var stream = GetStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
    
    public byte[] ReadBytes()
    {
        var stream = GetStream();
        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }
    
    public async Task<byte[]> ReadBytesAsync()
    {
        await using var stream = GetStream();
        var buffer = new byte[stream.Length];
        await stream.ReadAsync(buffer, 0, buffer.Length);
        return buffer;
    }
    
    public static Resource Load(string path, ResourceLifeSpan lifeSpan = ResourceLifeSpan.Permanent)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        return Load(assembly, path, lifeSpan);
    }

    public static Resource Load(Assembly assembly, string path, ResourceLifeSpan lifeSpan = ResourceLifeSpan.Permanent)
    {
        path = path.Replace("/", ".");
        path = path.Insert(0, assembly.GetName().Name + ".");

        if (ResourcesCache.ContainsKey(path))
        {
            return ResourcesCache[path];
        }
        else
        {

            //Check if resource exists in that assembly
            if (assembly.GetManifestResourceInfo(path) is null)
            {
                throw new FileNotFoundException("Resource not found. Path: " + path);
            }
            
            var resource = new Resource(path, assembly);
            if (lifeSpan == ResourceLifeSpan.Permanent)
            {
                ResourcesCache.Add(path, resource);
            }
            return resource;
        }
    }
    
    
}