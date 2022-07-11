using DSharpPlus;
using DSharpPlus.SlashCommands;
using Friday.Common;
using Friday.Common.Services;
using Miuex.Modules.Proxmox.Commands;
using Miuex.Modules.Proxmox.Services;

namespace Miuex.Modules.Proxmox;

public class ProxmoxModule : ModuleBase
{
    public ConfigurationService Configuration { get; }
    public APIService Api { get; }
    public VPSSqlService VpsSqlService { get; }
    private DiscordShardedClient _shardedClient;
    public ProxmoxModule(DiscordShardedClient shardedClient, DatabaseProvider provider)
    {
        this._shardedClient = shardedClient;
        Configuration = new ConfigurationService("conf/proxmox.json");
        Constants.Config = Configuration.GetConfiguration();
        Api = new APIService(Configuration.GetConfiguration());
        this.VpsSqlService = new VPSSqlService(provider);
    }

    public override void RegisterSlashCommands(SlashCommandsExtension extension)
    {
        extension.RegisterCommands<NodeCommands>(Configuration.GetConfiguration().ServerId);
        extension.RegisterCommands<VPSCommands>(Configuration.GetConfiguration().ServerId);
        extension.RegisterCommands<AdministrationCommands>(Configuration.GetConfiguration().ServerId);
        extension.RegisterCommands<OtherCommands>(Configuration.GetConfiguration().ServerId);
    }

    public override Task OnLoad()
    {
        Constants.Instance = _shardedClient.GetShard(Configuration.GetConfiguration().ServerId);
        return Task.CompletedTask;
    }

    public override Task OnUnload()
    {
        return Task.CompletedTask;
    }
}