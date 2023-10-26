using System.Net.Http.Json;
using System.Web;
using RestSharp;
using Serilog;

namespace Friday.Modules.Minesprout.Minesprout;

using Friday.Modules.Minesprout.Minesprout.Entities;

public class MinesproutClient
{
    private const string API_URL = "https://api.minesprout.net/";
    private readonly MinesproutModule _module;
    private readonly RestClient _restClient;
    public MinesproutClient(MinesproutModule module)
    {
        this._module = module;

        _restClient = new RestClient(API_URL);
    }

    public async Task<MinesproutServer?> GetServerAsync(ulong id)
    {
        return await _restClient.GetJsonAsync<MinesproutServer>($"/api/v1/listing/{id}");
    }

    public async Task<MinesproutServerList?> GetServersAsync(bool newest = false, int start = 0, int end = int.MaxValue)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["newest"] = newest.ToString();
        query["_start"] = start.ToString();
        query["_end"] = end.ToString();

        return await _restClient.GetJsonAsync<MinesproutServerList>($"/api/v1/servers?{query}");
    }

    public async Task<int> GetServerCountAsync()
    {
        var servers = await  GetServersAsync(false, 0, 0);
        return servers?.TotalCount ?? 0;
    }

    public async Task DeleteServerAsync(int serverId)
    {
        var request = new RestRequest();

        request.AddHeader("admin-token", _module.Configuration.Token);
        
        request.Method = Method.Delete;
        
        request.Resource = $"api/v1/admin/server/delete/{serverId}";

        var response = await _restClient.ExecuteAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Log.Error("Error deleting server: " + response.StatusCode);
        }
    }
    
    public string GetBannerUrl(int id)
    {
        return $"{API_URL}api/v1/servers/{id}/banner";
    }
}
