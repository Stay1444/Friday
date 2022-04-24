using DSharpPlus.Entities;
using Friday.UI.Entities;

namespace Friday.UI.Components;

public class SelectComponent : FridayUIComponent
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
    private List<SelectComponentOption> _options { get; set; } = new List<SelectComponentOption>();
    internal SelectComponent(FridayUIPage page) : base(page)
    {
        
    }

    internal async Task OnSelect(DiscordInteraction interaction)
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