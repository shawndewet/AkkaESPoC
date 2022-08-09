using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AkkaESPoC.Shared;
using AkkaESPoC.Shared.Queries;
using AkkaESPoC.Shared.Sharding;

namespace AkkaESPoC.WebApp.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IActorRef _indexActor;

    public IndexModel(ILogger<IndexModel> logger, ActorRegistry registry)
    {
        _logger = logger;
        _indexActor = registry.Get<QuestIndexMarker>();
    }

    public IReadOnlyList<QuestData> Quests { get; set; } = Array.Empty<QuestData>();

    public async Task<IActionResult> OnGetAsync()
    {
        var quests = await _indexActor.Ask<FetchAllQuestsResponse>(FetchAllQuests.Instance, TimeSpan.FromSeconds(5));
        
        Quests = quests.Quests;
        
        return Page();
    }
}