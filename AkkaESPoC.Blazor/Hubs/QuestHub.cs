using Microsoft.AspNetCore.SignalR;

namespace AkkaESPoC.Blazor.Hubs
{
    public class QuestHub : Hub
    {
        private readonly ILogger<QuestHub> _logger;

        public QuestHub(SignalRConnectionList connectionList, ILogger<QuestHub> logger)
        {
            ConnectionList = connectionList;
            _logger = logger;
        }

        private SignalRConnectionList ConnectionList { get; }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Hub client connected {Context.ConnectionId}");
            ConnectionList.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Hub client disconnected {Context.ConnectionId}");
            ConnectionList.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            _logger.LogInformation($"SendMessage to {ConnectionList.Count} connections...");

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }

    public class SignalRConnectionList
    {
        private readonly object _sync = new object();
        private readonly List<string> _items = new List<string>();

        public void Remove(string connectionId)
        {
            lock (_sync)
            {
                _items.Remove(connectionId);
            }
        }

        public void Add(string connectionId)
        {
            lock (_sync)
            {
                _items.Add(connectionId);
            }
        }

        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _items.Count;
                }
            }
        }
    }
}
