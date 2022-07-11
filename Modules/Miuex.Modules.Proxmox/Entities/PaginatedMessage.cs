using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace Miuex.Modules.Proxmox.Entities;

public class PaginatedMessage
{
    private List<DiscordEmbed> _pages;
    public IReadOnlyList<DiscordEmbed> Pages => _pages;
    private InteractionContext _context;
    private CancellationTokenSource _cancellationTokenSource;
    
    public PaginatedMessage(InteractionContext context, List<DiscordEmbed> pages)
    {
        this._context = context ?? throw new ArgumentNullException(nameof(context));
        this._pages = pages ?? throw new ArgumentNullException(nameof(pages));
        this._cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task SendAsync(TimeSpan span)
    {
        this._cancellationTokenSource.CancelAfter(span);
        _ = Task.Run(ProcessAsync);
    }

    private async Task ProcessAsync()
    {
        try
        {
            int currentPage = 0;
            DiscordWebhookBuilder builder = new();
            builder.AddEmbed(this._pages[currentPage]);
            
            var leftButton = new DiscordButtonComponent(ButtonStyle.Secondary, "left", "⬅");
            var rightButton = new DiscordButtonComponent(ButtonStyle.Secondary, "right", "➡");
            
            builder.AddComponents(leftButton, rightButton);

            await _context.EditResponseAsync(builder);
            
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var result = await _context.Client.GetInteractivity().WaitForButtonAsync(await _context.GetOriginalResponseAsync(), _cancellationTokenSource.Token);
                if (result.TimedOut)
                {
                    _cancellationTokenSource.Cancel();
                    builder.ClearComponents();
                    var msg = await _context.GetOriginalResponseAsync();
                    msg?.DeleteAsync();
                    return;
                }
            
                await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                
                if (result.Result.Id == leftButton.CustomId)
                {
                    if (currentPage == 0)
                    {
                        currentPage = this._pages.Count - 1;
                    }
                    else
                    {
                        currentPage--;
                    }
                }
                else if (result.Result.Id == rightButton.CustomId)
                {
                    if (currentPage == this._pages.Count - 1)
                    {
                        currentPage = 0;
                    }
                    else
                    {
                        currentPage++;
                    }
                }
            
                builder = new();
                builder.AddEmbed(this._pages[currentPage]);
                builder.AddComponents(leftButton, rightButton);
                await _context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(_pages[currentPage])
                        .AddComponents(leftButton, rightButton));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}