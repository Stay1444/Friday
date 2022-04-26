using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;

namespace Friday.UI.Entities;

public class FridayUIPage
{
    internal DiscordClient Client { get; }
    public DiscordEmbedBuilder Embed { get; set; }
    internal List<FridayUIComponent> Components { get; }
    internal Action<DiscordClient, DiscordMessage>? EventOnCancelled;
    internal Func<DiscordClient, DiscordMessage,Task>? EventOnCancelledAsync;
    private FridayUIBuilder _builder;
    internal Dictionary<string, FridayUIPage> SubPages { get; set; } = new Dictionary<string, FridayUIPage>();
    public string? SubPage { get; set; } = null;
    internal FridayUIPage(DiscordClient client, FridayUIBuilder builder)
    {
        this.Client = client;
        this.Embed = new DiscordEmbedBuilder();
        this.Embed.Transparent();
        this.Components = new List<FridayUIComponent>();
        this._builder = builder;
    }

    public void Add(FridayUIComponent component)
    {
        this.Components.Add(component);
    }

    public void NewLine()
    {
        this.Components.Add(new FridayUINewLine(this));
    }
    
    public void Stop()
    {
        _builder.RequestStop();
    }
    
    public void ForceRender()
    {
        _builder.ForceRender();
    }
    
    public void OnCancelled(Action<DiscordClient, DiscordMessage> action)
    {
        this.EventOnCancelled = action;
    }
    
    public void OnCancelledAsync(Func<DiscordClient, DiscordMessage,Task> action)
    {
        this.EventOnCancelledAsync = action;
    }

    public void AddSubPage(string id, Action<FridayUIPage> modify)
    {
        var page = new FridayUIPage(this.Client, this._builder);
        modify(page);
        this.SubPages.Add(id, page);
    }

    public async Task AddSubPageAsync(string id, Func<FridayUIPage, Task> modifyAsync)
    {
        var page = new FridayUIPage(this.Client, this._builder);
        await modifyAsync(page);
        this.SubPages.Add(id, page);
    }
    
}