using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Friday.Modules.ChannelStats.Entities.Vars;

namespace Friday.Modules.ChannelStats.Services;

public class VariablesService
{
    private ChannelStatsModule _module;
    private static Dictionary<string, Variable>? _variables;
    internal VariablesService(ChannelStatsModule module)
    {
        this._module = module;

        if (_variables is null)
        {
            _variables = new Dictionary<string, Variable>();
            _variables.Add("memberCount", new MemberCountVariable());
            _variables.Add("memberCountType", new MemberCountTypeVariable());
            _variables.Add("roleCount", new RoleCountVariable());
        }
    }

    public Dictionary<string, Variable> GetVariables()
    {
        return _variables!.ToDictionary(x => x.Key, x => x.Value);
    }

    private static Regex _parseRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);
    private (string replace, string name, string[] args)[] Parse(string raw)
    {
        var matches = _parseRegex.Matches(raw);
        var result = new List<(string replace, string name, string[] args)>();
        
        foreach (Match match in matches)
        {
            var value = match.Groups[1].Value;
            var name = value.Split('[')[0];
            var args = new List<string>();
            if (value.Contains('['))
            {
                foreach (var arg in value.Split('[')[1].Split(']')[0].Split(','))
                {
                    args.Add(arg.Trim());
                }
            }
            
            result.Add(($"{{{value}}}", name, args.ToArray()));
        }
        
        return result.ToArray();
    }

    public async Task<string> Process(DiscordGuild guild, string raw)
    {
        var parsed = Parse(raw);

        foreach (var (replace, name, args) in parsed)
        {
            if (!_variables!.ContainsKey(name))
            {
                continue;
            }
            
            var variable = _variables[name];
            var value = await variable.Process(_module, guild, args);
            raw = raw.Replace(replace, value);
        }

        return raw;
    }

}