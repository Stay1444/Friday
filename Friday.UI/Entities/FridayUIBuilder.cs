using DSharpPlus;

namespace Friday.UI.Entities;

public class FridayUIBuilder
{
    internal CancellationTokenSource CancellationTokenSource { get; }
    internal FridayUIPage? Page { get; private set; }
    private Action<FridayUIPage>? _renderAction;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    public FridayUIBuilder()
    {
        this.CancellationTokenSource = new CancellationTokenSource();
    }
    
    public FridayUIBuilder OnRender(Action<FridayUIPage> renderAction)
    {
        this._renderAction = renderAction;
        return this;
    }
    
    internal void Render(DiscordClient client)
    {
        if (this.Page is null)
        {
            Page = new FridayUIPage(client, this);
        }
        
        Page.Components.Clear();
        var oldSubPages = Page.SubPages;
        Page.SubPages = new();
        _renderAction?.Invoke(Page);

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