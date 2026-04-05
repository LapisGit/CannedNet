using System.Text.Json;
using CannedNet.Services;
using Microsoft.AspNetCore.SignalR;

namespace CannedNet.Hubs;

public class NotificationsHub : Hub
{
    private readonly NotificationService _notificationService;

    public NotificationsHub(NotificationService notificationService)
    {
        _notificationService = notificationService;
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
        var hubContext = GetService<IHubContext<NotificationsHub>>();
        if (hubContext != null)
        {
            await _notificationService.SubscribeToPlayers(Context.ConnectionId, subscriptionList, hubContext);
        }
    }

    public Task<HashSet<int>> GetSubscriptions()
    {
        return Task.FromResult(_notificationService.GetSubscribedPlayers(Context.ConnectionId).ToHashSet());
    }

    public NotificationService GetNotificationService()
    {
        return _notificationService;
    }

    private T? GetService<T>() where T : class
    {
        return Context.GetHttpContext()?.RequestServices.GetService(typeof(T)) as T;
    }
}
