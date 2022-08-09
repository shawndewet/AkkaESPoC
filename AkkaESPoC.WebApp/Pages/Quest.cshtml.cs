using System.ComponentModel.DataAnnotations;
using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AkkaESPoC.Shared;
using AkkaESPoC.Shared.Commands;
using AkkaESPoC.Shared.Queries;
using AkkaESPoC.Shared.Sharding;

namespace AkkaESPoC.WebApp.Pages;

public class Quest : PageModel
{
    private readonly IActorRef _questActor;

    public Quest(ActorRegistry registry)
    {
        _questActor = registry.Get<QuestMarker>();
    }

    [BindProperty(SupportsGet = true)]
    public string QuestId { get; set; }
    
    [BindProperty]
    [Required]
    [Range(1, 10000)]
    public int Quantity { get; set; }
    
    public QuestState State { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await _questActor.Ask<FetchResult>(new FetchQuest(QuestId), TimeSpan.FromSeconds(3));
        State = result.State;

        if (State.IsEmpty) // no quest with this id
            return NotFound();

        return Page();
    }

    //public async Task<IActionResult> OnPostNewOrderAsync()
    //{
    //    var newOrder = new QuestOrder(Guid.NewGuid().ToString(), QuestId, Quantity, DateTime.UtcNow);
    //    var createOrderCommand = new PurchaseQuest(newOrder);

    //    var result = await _questActor.Ask<QuestCommandResponse>(createOrderCommand, TimeSpan.FromSeconds(3));
    //    if (!result.Success)
    //    {
    //        return BadRequest();
    //    }

    //    return RedirectToAction(nameof(OnGetAsync), new{ questId=QuestId });
    //}
    
    //public async Task<IActionResult> OnPostInventoryUpdateAsync()
    //{
    //    var createOrderCommand = new SupplyQuest(QuestId, Quantity);

    //    var result = await _questActor.Ask<QuestCommandResponse>(createOrderCommand, TimeSpan.FromSeconds(3));
    //    if (!result.Success)
    //    {
    //        return BadRequest();
    //    }

    //    return RedirectToAction(nameof(OnGetAsync), new{ questId=QuestId });
    //}
}