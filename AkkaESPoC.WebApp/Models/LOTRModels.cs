namespace AkkaESPoC.WebApp.Models;

public class StartQuestModel
{
    public string QuestName { get; set; }
    public string Location { get; set; }
    public JoinQuestModel InitialMembers { get; set; }
}

public class ArriveAtLocationModel
{
    public string Location { get; set; }

    public int Day { get; set; }

}

public class JoinQuestModel
{
    public int Day { get; set; }

    //public string Location { get; set; }

    public string[] Members { get; set; }

}

public class LeaveQuestModel
{
    public int Day { get; set; }

    public string[] Members { get; set; }

}
public class SlayCharactersModel
{
    public int Day { get; set; }

    public string[] Characters { get; set; }

}