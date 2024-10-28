namespace API.SignalR;

public class PresenceTracker
{
    private readonly static Dictionary<string, List<string>> OnlineUsers = [];

    public Task<bool> UserConnected(string username, string connectedId)
    {
        var isOnline = false;
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username] = [];
                isOnline = true;
            }

            OnlineUsers[username].Add(connectedId);
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectedId)
    {
        var isOffline = false;
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

            OnlineUsers[username].Remove(connectedId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;

        lock(OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUser(string username) 
    {
        List<string> connectionIds;

        if (OnlineUsers.TryGetValue(username, out var connections))
        {
            lock(connections)
            {
                connectionIds = [.. connections];
            }
        }
        else
        {
            connectionIds = [];
        }

        return Task.FromResult(connectionIds);
    }
}
