using DSharpPlus;
using DSharpPlus.Entities;
using Friday.UI.Entities;

namespace Friday.UI.Components;

public class SelectComponent : FridayUISelectComponent
{
    public class SelectComponentOption
    {
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public DiscordEmoji? Emoji { get; set; }
    }
    
    public string? Placeholder { get; set; }
    public bool Disabled { get; set; }
    public int MinOptions { get; set; } = 1;
    public int MaxOptions { get; set; } = 1;
    private List<SelectComponentOption> _options = new List<SelectComponentOption>();
    private Action<string[]>? _onSelect;
    private Func<string[], Task>? _onSelectAsync;
    internal SelectComponent(FridayUIPage page) : base(page)
    {
        
    }

    internal override DiscordComponent? GetDiscordComponent()
    {
        var discordOptions = new List<DiscordSelectComponentOption>();
        
        foreach (var option in _options)
        {
            discordOptions.Add(new DiscordSelectComponentOption(option.Label, option.Value, option.Description, option.IsDefault, option.Emoji == null ? null : new DiscordComponentEmoji(option.Emoji)));
        }
        
        return new DiscordSelectComponent(Id, Placeholder, discordOptions, Disabled, MinOptions, MaxOptions);
    }

    internal override async Task<bool> OnSelect(DiscordInteraction interaction, string[] values)
    {
        if (_onSelect != null)
        {
            _onSelect(values);
        }
        else if (_onSelectAsync != null)
        {
            await _onSelectAsync(values);
        }

        return false;
    }
    
    public void OnSelect(Action<string[]> onSelect)
    {
        _onSelect = onSelect;
    }
    
    public void OnSelect(Func<string[], Task> onSelect)
    {
        _onSelectAsync = onSelect;
    }

    public void AddOption(Action<SelectComponentOption> modify)
    {
        var option = new SelectComponentOption();
        modify(option);
        _options.Add(option);
    }

    public async Task AddOption(Func<SelectComponentOption, Task> modifyAsync)
    {
        var option = new SelectComponentOption();
        await modifyAsync(option);
        _options.Add(option);
    }
}