using System.Text.Json;
using CannedNet.Services;
using Microsoft.AspNetCore.SignalR;

namespace CannedNet.Hubs;

public class NotificationsHub : Hub
{
    private static NotificationService _notificationService = new();

    public NotificationsHub()
    {
    }

    public override async Task OnConnectedAsync()
    {
        _notificationService.OnConnected(Context.ConnectionId);
        await Clients.Caller.SendAsync("OnConnect");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _notificationService.OnDisconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToPlayers(SubscriptionList subscriptionList)
    {
        await _notificationService.SubscribeToPlayers(Context.ConnectionId, subscriptionList);
    }

    public Task<HashSet<int>> GetSubscriptions()
    {
        return Task.FromResult(_notificationService.GetSubscribedPlayers(Context.ConnectionId).ToHashSet());
    }

    public static NotificationService GetNotificationService()
    {
        return _notificationService;
    }
}
