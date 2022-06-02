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

    public string Path => this._path;

    public string FileName => Path.Substring(Path.LastIndexOf('.') + 1);
    
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
        var buffer = new List<byte>();
        var byteBuffer = new byte[1024];
        var r = stream.Read(byteBuffer, 0, byteBuffer.Length);
        
        while (r > 0)
        {
            buffer.AddRange(byteBuffer.Take(r));
            r = stream.Read(byteBuffer, 0, byteBuffer.Length);
        }
        
        return buffer.ToArray();
    }
    
    public async Task<byte[]> ReadBytesAsync()
    {
        await using var stream = GetStream();
        var buffer = new List<byte>();
        var byteBuffer = new byte[1024];
        var r = await stream.ReadAsync(byteBuffer, 0, byteBuffer.Length);
        
        while (r > 0)
        {
            buffer.AddRange(byteBuffer.Take(r));
            r = await stream.ReadAsync(byteBuffer, 0, byteBuffer.Length);
        }
        
        return buffer.ToArray();
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
    
    public static Resource[] LoadDirectory(string path, ResourceLifeSpan lifeSpan = ResourceLifeSpan.Permanent)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        return LoadDirectory(assembly, path, lifeSpan);
    }
    
    public static Resource[] LoadDirectory(Assembly assembly, string path, ResourceLifeSpan lifeSpan = ResourceLifeSpan.Permanent)
    {
        path = path.Replace("/", ".");
        path = path.Insert(0, assembly.GetName().Name + ".");
        
        var resources = new List<Resource>();
        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (resource.StartsWith(path))
            {
                resources.Add(Load(assembly, resource, lifeSpan));
            }
        }
        return resources.ToArray();
    }
    
    
}