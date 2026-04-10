using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using CannedNet.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CannedNet.Services;

public class NotificationService
{
    private static readonly ConcurrentDictionary<string, HashSet<int>> _connectionSubscriptions = new();
    private static readonly ConcurrentDictionary<int, HashSet<string>> _playerConnections = new();
    private static readonly ConcurrentDictionary<int, Queue<NotificationMessage>> _pendingNotifications = new();
    private static IHubContext<NotificationsHub>? _hubContext;
    private readonly ILogger<NotificationService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public static void SetHubContext(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public void OnConnected(string connectionId)
    {
        _logger.LogInformation($"Client connected: {connectionId}");
        _connectionSubscriptions.TryAdd(connectionId, new HashSet<int>());
    }

    public void OnDisconnected(string connectionId)
    {
        _logger.LogInformation($"Client disconnected: {connectionId}");
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

    public async Task SubscribeToPlayers(string connectionId, SubscriptionList subscriptionList, IHubContext<NotificationsHub> hubContext)
    {
        var oldSubscriptions = _connectionSubscriptions.GetValueOrDefault(connectionId, new HashSet<int>());
        var newPlayerIds = subscriptionList.PlayerIds ?? new List<int>();

        _logger.LogInformation($"Connection {connectionId} subscribing to players: {string.Join(", ", newPlayerIds)}");

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

            if (_pendingNotifications.TryRemove(playerId, out var pendingQueue))
            {
                _logger.LogInformation($"Found {pendingQueue.Count} pending notifications for player {playerId}. Sending to connection {connectionId}");
                while (pendingQueue.TryDequeue(out var notification))
                {
                    try
                    {
                        await SendToConnection(hubContext, connectionId, notification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending pending notification to player {playerId}");
                    }
                }
            }
        }

        foreach (var playerId in oldSubscriptions)
        {
            if (!newPlayerIds.Contains(playerId) && _playerConnections.TryGetValue(playerId, out var connections))
            {
                connections.Remove(connectionId);
            }
        }
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

    public async Task SendNotificationToPlayer(int playerId, NotificationMessage notification)
    {
        if (_hubContext == null)
        {
            _logger.LogWarning("No hub context available. Cannot send notification.");
            return;
        }

        try
        {
            var connections = GetConnectionsForPlayer(playerId).ToList();
            _logger.LogInformation($"Attempting to send notification to player {playerId}. Found {connections.Count} connections. NotificationType: {notification.NotificationType}");

            if (connections.Any())
            {
                foreach (var connectionId in connections)
                {
                    try
                    {
                        await SendToConnection(_hubContext, connectionId, notification);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sending notification to player {playerId} on connection {connectionId}. NotificationType: {notification.NotificationType}");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"No connections found for player {playerId}. Queueing notification for later delivery. NotificationType: {notification.NotificationType}");
                _pendingNotifications.AddOrUpdate(
                    playerId,
                    _ => new Queue<NotificationMessage>(new[] { notification }),
                    (_, queue) =>
                    {
                        queue.Enqueue(notification);
                        return queue;
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending notification to player {playerId}. NotificationType: {notification.NotificationType}");
        }
    }

    public async Task BroadcastNotification(NotificationMessage notification)
    {
        if (_hubContext == null)
        {
            _logger.LogWarning("No hub context available. Cannot broadcast notification.");
            return;
        }

        try
        {
            foreach (var kvp in _playerConnections)
            {
                foreach (var connectionId in kvp.Value)
                {
                    await SendToConnection(_hubContext, connectionId, notification);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting notification. NotificationType: {notification.NotificationType}");
        }
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

    private static async Task SendToConnection(IHubContext<NotificationsHub> hubContext, string connectionId, NotificationMessage notification)
    {
        try
        {
            var msgData = new Dictionary<string, object>();
            if (notification.Data != null)
            {
                foreach (var kvp in notification.Data)
                {
                    if (kvp.Value == null)
                        continue;

                    if (kvp.Value is DateTime dt)
                    {
                        msgData[kvp.Key] = dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    }
                    else
                    {
                        msgData[kvp.Key] = kvp.Value;
                    }
                }
            }

            var notificationData = new
            {
                Id = (int?)notification.NotificationType ?? 0,
                Msg = msgData
            };
            
            var json = JsonSerializer.Serialize(notificationData);
            Console.WriteLine($"[NotificationService] Sending notification to {connectionId}: {json}");
            
            await hubContext.Clients.Client(connectionId).SendAsync("Notification", json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Error sending notification: {ex.Message}");
        }
    }
}
