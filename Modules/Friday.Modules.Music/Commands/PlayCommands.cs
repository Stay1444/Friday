using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Friday.Common;
using Friday.Modules.Music.Processing;
using Friday.Modules.Music.Services;

namespace Friday.Modules.Music.Commands;

public partial class Commands
{
    private MusicModuleBase _moduleBase;

    public Commands(MusicModuleBase moduleBase)
    {
        _moduleBase = moduleBase;
    }

    [Command("play")]
    [RequireGuild]
    public async Task PlayCommand(CommandContext ctx)
    {
        var guildMusic = _moduleBase.GetGuildMusic(ctx.Guild);
        if (guildMusic.Channel is not null)
        {
            await ctx.RespondAsync("Already playing music!");
            return;
        }
        
        var voiceState = ctx.Member.VoiceState;
        if (voiceState is null)
        {
            await ctx.RespondAsync("You are not in a voice channel!");
            return;
        }
        
        var voiceChannel = voiceState.Channel;
        if (voiceChannel is null)
        {
            await ctx.RespondAsync("You are not in a voice channel!");
            return;
        }

        await guildMusic.Join(voiceChannel);
        var panel = MusicPanel.GetMusicPanel(guildMusic);
        if (panel is null)
        {
            panel = MusicPanel.CreateMusicPanel(guildMusic, ctx.Channel, _moduleBase);
            await panel.SendMessageAsync();
        }
    }

}