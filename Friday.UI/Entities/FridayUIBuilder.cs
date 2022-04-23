using DSharpPlus;

namespace Friday.UI.Entities;

public class FridayUIBuilder
{
    internal CancellationTokenSource CancellationTokenSource { get; private set; }
    internal FridayUIPage? Page { get; private set; }
    private Action<FridayUIPage>? _renderAction;
    private Func<FridayUIPage, Task>? _asyncRenderAction;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    internal bool StopRequested { get; private set; }
    public bool RenderRequested { get; private set; }
    public FridayUIBuilder()
    {
        this.CancellationTokenSource = new CancellationTokenSource();
    }
    
    public FridayUIBuilder OnRender(Action<FridayUIPage> renderAction)
    {
        this._renderAction = renderAction;
        return this;
    }
    
    public FridayUIBuilder OnRenderAsync(Func<FridayUIPage, Task> renderAction)
    {
        this._asyncRenderAction = renderAction;
        return this;
    }
    
    internal void ForceRender()
    {
        this.RenderRequested = true;
        CancellationTokenSource.Cancel();
    }

    internal void RequestStop()
    {
        StopRequested = true;
        CancellationTokenSource.Cancel();
    }

    internal void ResetToken()
    {
        CancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource.CancelAfter(Duration);
        RenderRequested = false;
        StopRequested = false;
    }
    
    internal async Task Render(DiscordClient client)
    {
        if (this.Page is null)
        {
            Page = new FridayUIPage(client, this);
        }
        
        Page.Components.Clear();
        var oldSubPages = Page.SubPages;
        Page.SubPages = new();
        if (_asyncRenderAction is not null)
        {
            await _asyncRenderAction(Page);
        }
        else
        {
            _renderAction?.Invoke(Page);
        }

        void SetSubPagesSubPage(Dictionary<string, FridayUIPage> old, Dictionary<string, FridayUIPage> @new)
        {
            foreach (var newPage in @new)
            {
                if (old.ContainsKey(newPage.Key))
                {
                    SetSubPagesSubPage(old[newPage.Key].SubPages, newPage.Value.SubPages);
                    
                    newPage.Value.SubPage = old[newPage.Key].SubPage;
                }
            }
        }
        
        SetSubPagesSubPage(oldSubPages, Page.SubPages);
        
    }
}