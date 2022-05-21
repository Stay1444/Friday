using DSharpPlus;
using DSharpPlus.Entities;
using Friday.Common;

namespace Friday.UI.Entities;

public class FridayUIPage
{
    internal DiscordClient Client { get; }
    //public DiscordEmbedBuilder Embed { get; set; }
    
    private DiscordMessageBuilder? _msgBuilder;
    private DiscordEmbedBuilder? _embedBuilder;
    
    internal Dictionary<string, object> State { get; set; } = new Dictionary<string, object>();
    
    public DiscordMessageBuilder Message => _msgBuilder ??= new DiscordMessageBuilder();

    public DiscordEmbedBuilder Embed => _embedBuilder ??= new DiscordEmbedBuilder();

    internal bool UsedMessageBuilder => _msgBuilder != null;
    internal bool UsedEmbedBuilder => _embedBuilder != null;
    
    internal List<FridayUIComponent> Components { get; }
    internal Action<DiscordClient, DiscordMessage>? EventOnCancelled;
    internal Func<DiscordClient, DiscordMessage,Task>? EventOnCancelledAsync;
    private readonly FridayUIBuilder _builder;
    internal Dictionary<string, FridayUIPage> SubPages { get; set; } = new Dictionary<string, FridayUIPage>();

    internal Dictionary<string, (Action<FridayUIPage>? render, Func<FridayUIPage, Task>? task)> SubPagesRender =
        new Dictionary<string, (Action<FridayUIPage>? render, Func<FridayUIPage, Task>? task)>();
    
    public string? SubPage { get; set; }
    internal FridayUIPage(DiscordClient client, FridayUIBuilder builder)
    {
        this.Client = client;
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
        this.SubPages.Add(id, page);
        this.SubPagesRender.Add(id, (modify, null));
    }

    public void AddSubPageAsync(string id, Func<FridayUIPage, Task> modifyAsync)
    {
        var page = new FridayUIPage(this.Client, this._builder);
        this.SubPages.Add(id, page);
        this.SubPagesRender.Add(id, (null, modifyAsync));
    }

    public Ref<T> GetState<T>(string key, T def)
    {
        if (!State.ContainsKey(key))
        {
            State.Add(key, new Ref<T>(def));
        }
        
        return (Ref<T>) State[key];
    }

    internal void Reset()
    {
        _embedBuilder = null;
        _msgBuilder = null;
    }
}