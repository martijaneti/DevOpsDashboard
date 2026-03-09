using Microsoft.AspNetCore.SignalR;

namespace DevOpsDashboard.API.Hubs;

public class DashboardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", "Dashboard hub connected successfully");
        await base.OnConnectedAsync();
    }
}