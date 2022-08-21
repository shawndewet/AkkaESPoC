using AkkaESPoC.Shared;
using Microsoft.AspNetCore.SignalR;

namespace AkkaESPoC.Blazor.Hubs
{
    public class QuestHubHelper
    {
        private readonly IHubContext<QuestHub> _hub;
        private readonly ILogger<QuestHubHelper> _logger;
        private readonly SignalRConnectionList _list;

        public QuestHubHelper(IHubContext<QuestHub> hub, ILogger<QuestHubHelper> logger, SignalRConnectionList list)
        {
            _hub = hub;
            _logger = logger;
            _list = list;
            _logger.LogInformation("Instantiating QuestHubHelper");
        }

        public async Task AddQuest(QuestData questData)
        {
            //var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));

            _logger.LogInformation($"Have {_list.Count} connections in QuestHubHelper");
            await _hub.Clients.All.SendAsync("AddQuest", questData); //, cts.Token);
            _logger.LogInformation($"Sent AddQuest for {questData.QuestId}");
        }


        public async Task UpdateQuest(QuestData questData)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
            await _hub.Clients.All.SendAsync("UpdateQuest", questData, cts.Token);
        }

    }
}
