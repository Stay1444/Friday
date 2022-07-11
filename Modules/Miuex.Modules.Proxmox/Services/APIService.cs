using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using DSharpPlus.Entities;
using Miuex.Modules.Proxmox.Models;

namespace Miuex.Modules.Proxmox.Services;

public class APIService
{
    private HttpClient _client;
    
    public APIService(Configuration configuration)
    {
        
        var handler = new HttpClientHandler() 
        { 
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        
        _client = new HttpClient(handler);
        _client.BaseAddress = new Uri(configuration.ApiUrl!);
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", configuration.ApiKey);
    } 
    
    public async Task<Node[]> GetNodesAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            
            var response = await _client.GetAsync("Nodes");
            if (response.IsSuccessStatusCode)
            {
                var nodes = await response.Content.ReadFromJsonAsync<Node[]>();
                return nodes ?? new Node[0];
            }

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "GET /Nodes")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            return Array.Empty<Node>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            stopwatch.Stop();
            throw;
        }
    }

    public async Task<VirtualMachine[]> GetVmsAsync(int node)
    {
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _client.GetAsync($"Nodes/{node}/vms");
            if (response.IsSuccessStatusCode)
            {
                var vms = await response.Content.ReadFromJsonAsync<VirtualMachine[]>();
                return vms ?? new VirtualMachine[0];
            }
            
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "GET /Nodes/{node}/vms")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            return Array.Empty<VirtualMachine>();
        }catch (Exception e)
        {
            Console.WriteLine(e);
            stopwatch.Stop();
            throw;
        }
    }

    public async Task<(bool success, string message)> AddNodeAsync(string name, string endpoint, string apikey)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            
            var response = await _client.PutAsJsonAsync("Nodes", new
            {
                name = name,
                endpoint = endpoint,
                apikey = apikey
            });
            
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "PUT /Nodes")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "Node added successfully");
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                return (false, "Node does not respond");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return (false, "Node already exists");
            }
            
            return (false, "Unknown error");
            
        }catch (Exception e)
        {
            stopwatch.Stop();
            return (false, e.Message);
        }
    }

    public async Task<bool> RemoveNodeAsync(int nodeId)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _client.DeleteAsync($"Nodes/{nodeId}");
            
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "DELETE /Nodes/{nodeId}")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            return response.IsSuccessStatusCode;
        }catch
        {
            stopwatch.Stop();
            return false;
        }
    }

    public async Task<VirtualMachine?> GetVirtualMachineAsync(int node, int vm)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _client.GetAsync($"Nodes/{node}/vms/{vm}");
            
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "GET /Nodes/{node}/vms/{vm}")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            if (response.IsSuccessStatusCode)
            {
                var vmResponse = await response.Content.ReadFromJsonAsync<VirtualMachine>();
                return vmResponse;
            }

            return null;
        }catch (Exception e)
        {
            stopwatch.Stop();
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<bool> StartVirtualMachineAsync(int node, int vm)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _client.PostAsync($"Nodes/{node}/vms/{vm}/start", null);
            
            stopwatch.Stop();

            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "POST /Nodes/{node}/vms/{vm}/start")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            
            return response.IsSuccessStatusCode;
        }catch (Exception e)
        {
            stopwatch.Stop();
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> StopVirtualMachineAsync(int node, int vm)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await _client.PostAsync($"Nodes/{node}/vms/{vm}/stop", null);
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "POST /Nodes/{node}/vms/{vm}/stop")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            return response.IsSuccessStatusCode;
        }catch (Exception e)
        {
            stopwatch.Stop();
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> ResetVirtualMachineAsync(int node, int vm)
    {
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _client.PostAsync($"Nodes/{node}/vms/{vm}/reset", null);
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                await Extensions.Log(new DiscordEmbedBuilder().WithTitle("Long API call")
                    .AddField("Method", "POST /Nodes/{node}/vms/{vm}/reset")
                    .AddField("Time", stopwatch.ElapsedMilliseconds.ToString() + "ms")
                    .AddField("Discord Latency", Constants.Instance?.Ping.ToString() + "ms")
                );
            }
            
            return response.IsSuccessStatusCode;
        }catch (Exception e)
        {
            stopwatch.Stop();
            Console.WriteLine(e);
            return false;
        }
    }
}