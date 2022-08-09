using System.ComponentModel.DataAnnotations;
using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AkkaESPoC.Shared.Commands;
using AkkaESPoC.Shared.Sharding;

namespace AkkaESPoC.WebApp.Pages;


public sealed class NewQuest
{
    [BindProperty]
    [Required]
    [MinLength(6)]
    [MaxLength(50)]
    public string QuestName { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(50)]
    public string Location { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string InitialMembers { get; set; }
}

public class CreateQuest : PageModel
{
    public CreateQuest(ActorRegistry registry)
    {
        _questActor = registry.Get<QuestMarker>();
    }

    [BindProperty]
    public NewQuest Quest { get; set; }
    
    private readonly IActorRef _questActor;
    
    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var createQuestCommand = new AkkaESPoC.Shared.Commands.CreateQuest(Guid.NewGuid().ToString(),
            Quest.QuestName, Quest.Location, Quest.InitialMembers.Split(','));

        var createRsp = await _questActor.Ask<QuestCommandResponse>(createQuestCommand, TimeSpan.FromSeconds(3));

        return RedirectToPage("./quest", new { questId = createQuestCommand.QuestId });
    }
}