using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace CannedNet.Services;

public class NotificationService
{
    private readonly ConcurrentDictionary<string, HashSet<int>> _connectionSubscriptions = new();
    private readonly ConcurrentDictionary<int, HashSet<string>> _playerConnections = new();

    public void OnConnected(string connectionId)
    {
        _connectionSubscriptions.TryAdd(connectionId, new HashSet<int>());
    }

    public void OnDisconnected(string connectionId)
    {
        if (_connectionSubscriptions.TryRemove(connectionId, out var subscriptions))
        {
            foreach (var playerId in subscriptions)
            {
                if (_playerConnections.TryGetValue(playerId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _playerConnections.TryRemove(playerId, out _);
                    }
                }
            }
        }
    }

    public Task SubscribeToPlayers(string connectionId, SubscriptionList subscriptionList)
    {
        var oldSubscriptions = _connectionSubscriptions.GetValueOrDefault(connectionId, new HashSet<int>());
        var newPlayerIds = subscriptionList.PlayerIds ?? new List<int>();

        _connectionSubscriptions[connectionId] = new HashSet<int>(newPlayerIds);

        foreach (var playerId in newPlayerIds)
        {
            _playerConnections.AddOrUpdate(
                playerId,
                _ => new HashSet<string> { connectionId },
                (_, existing) =>
                {
                    existing.Add(connectionId);
                    return existing;
                });
        }

        foreach (var playerId in oldSubscriptions)
        {
            if (!newPlayerIds.Contains(playerId) && _playerConnections.TryGetValue(playerId, out var connections))
            {
                connections.Remove(connectionId);
            }
        }

        return Task.CompletedTask;
    }

    public HashSet<int>? GetSubscriptions(string connectionId)
    {
        return _connectionSubscriptions.GetValueOrDefault(connectionId);
    }

    public IEnumerable<string> GetConnectionsForPlayer(int playerId)
    {
        if (_playerConnections.TryGetValue(playerId, out var connections))
        {
            return connections.ToList();
        }
        return Enumerable.Empty<string>();
    }

    public IEnumerable<int> GetSubscribedPlayers(string connectionId)
    {
        if (_connectionSubscriptions.TryGetValue(connectionId, out var subscriptions))
        {
            return subscriptions.ToList();
        }
        return Enumerable.Empty<int>();
    }

    public async Task SendNotificationToPlayer(string callerConnectionId, IHubContext hubContext, int playerId, NotificationMessage notification)
    {
        var connections = GetConnectionsForPlayer(playerId);
        foreach (var connectionId in connections)
        {
            if (connectionId == callerConnectionId)
                continue;

            await hubContext.Clients.Client(connectionId).SendAsync("Notification", notification);
        }
    }

    public async Task BroadcastNotification(IHubContext hubContext, NotificationMessage notification)
    {
        await hubContext.Clients.All.SendAsync("Notification", notification);
    }

    public NotificationMessage CreateNotification(PushNotificationId notificationType, int? id = null, int? fromAccountId = null, int? toAccountId = null, Dictionary<string, object>? data = null)
    {
        return new NotificationMessage
        {
            NotificationId = Guid.NewGuid().ToString("N"),
            NotificationType = notificationType,
            Id = id,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            CreatedAt = DateTime.UtcNow,
            Data = data
        };
    }
}
