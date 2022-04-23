using System.Reflection;
using DSharpPlus.Entities;
using Serilog;

namespace Friday.Common.Services;

public class LanguageProvider
{
    private DatabaseProvider _db;
    private UserConfigurationProvider _userConfiguration;
    private GuildConfigurationProvider _guildConfiguration;
    private List<Assembly> _modules;
    private Dictionary<string, Dictionary<string,string>> _language;
    public LanguageProvider(DatabaseProvider db, UserConfigurationProvider userConfiguration, GuildConfigurationProvider guildConfiguration, Assembly[] modules)
    {
        _db = db;
        _userConfiguration = userConfiguration;
        _guildConfiguration = guildConfiguration;
        _modules = modules.ToList();
        _language = new Dictionary<string, Dictionary<string, string>>();
    }

    public void Build()
    {
        Log.Information("[LanguageProvider] Building language provider...");
        Log.Information("[LanguageProvider] Loading languages...");
        
        foreach (var assembly in _modules)
        {
            
            var resourceNames = assembly.GetManifestResourceNames();
            var expectedPath = $"{assembly.GetName().Name}.Resources.Languages.";
            Log.Information("[LanguageProvider] Loading languages from {0}", assembly.GetName().Name);
            foreach (var resourceName in resourceNames.Where(x => x.StartsWith(expectedPath)))
            {
                if (!resourceName.EndsWith(".lang"))
                {
                    continue; // Skip non-language files
                }
                
                var splitName = resourceName.Split('.');
                var language = splitName[^2];
                if (!_language.ContainsKey(language))
                {
                    _language.Add(language, new Dictionary<string, string>());
                    Log.Information("[LanguageProvider] Registered language {0}", language);
                }

                using var stream = assembly.GetManifestResourceStream(resourceName);
                
                if (stream == null)
                {
                    Log.Error("[LanguageProvider] Failed to load language {0} from {1}", resourceName, assembly.FullName);
                    continue;
                }
                
                using var reader = new StreamReader(stream);
                var lines = reader.ReadToEnd().Split('\n');
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                    
                    // Format: key = value
                    // Remove text after # (comments)
                    var actualLine = line.Split('#')[0];
                    
                    var splitLine = actualLine.Split('=');
                    if (splitLine.Length < 2)
                    {
                        Log.Error("[LanguageProvider] Invalid line in language file {0} from {1}", actualLine, assembly.FullName);
                        continue;
                    }
                    
                    var key = splitLine[0];
                    // Value is everything after the first =
                    var value = string.Join("=", splitLine.Skip(1));
                    
                    if (_language[language].ContainsKey(key))
                    {
                        Log.Warning("[LanguageProvider] Overwriting language key {0} in language {1}", key, language);
                        _language[language][key] = value;
                    }else
                    {
                        _language[language].Add(key, value);
                    }
                }
            }
        }
        Log.Information("[LanguageProvider] Loaded {0} languages", _language.Count);
    }
    
    public string GetString(string language, string key, params object[] format)
    {
        if (!_language.ContainsKey(language))
        {
            // Language not found. Check if there is any other language with that key (as a fallback)
            foreach (var lang in _language)
            {
                if (lang.Value.ContainsKey(key))
                {
                    return lang.Value[key];
                }
            }
            
            return key;
        }
        
        if (!_language[language].ContainsKey(key))
        {
            return key;
        }

        return string.Format(_language[language][key], format);
    }
    
    public async Task<string> GetString(DiscordMember member, string key, params object[] format)
    {
        var userConfigTask = _userConfiguration.GetConfiguration(member);
        var guildConfig = await _guildConfiguration.GetConfiguration(member.Guild);
        var userConfig = await userConfigTask;

        var language = guildConfig.Language;
        if (userConfig.LanguageOverride is not null)
        {
            language = userConfig.LanguageOverride;
        }
            
        return GetString(language, key, format);
    }

    public async Task<string> GetString(DiscordUser user, string key, params object[] format)
    {
        var userConfigTask = _userConfiguration.GetConfiguration(user);
        var language = "en";
        var userConfig = await userConfigTask;
        if (userConfig.LanguageOverride is not null)
        {
            language = userConfig.LanguageOverride;
        }
            
        return GetString(language, key, format);
    }

    public async Task<string> GetDesiredLanguage(DiscordMember member)
    {
        var userConfigTask = _userConfiguration.GetConfiguration(member);
        var guildConfig = await _guildConfiguration.GetConfiguration(member.Guild);
        var userConfig = await userConfigTask;

        var language = guildConfig.Language;
        if (userConfig.LanguageOverride is not null)
        {
            language = userConfig.LanguageOverride;
        }
            
        return language;
    }
    
}