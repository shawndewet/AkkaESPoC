namespace AkkaESPoC.Shared.Events;

/// <summary>
/// Used to distinguish quest events from commands
/// </summary>
public interface IQuestEvent : IWithQuestId
{
}

public record QuestCreated(string QuestId, string QuestName, string Location) : IQuestEvent;

public record MemberJoined(string QuestId, string[] MemberName, int DaysIn) : IQuestEvent;

public record MemberDeparted(string QuestId, string[] MemberName, int DaysIn) : IQuestEvent;

public record ArrivedAtLocation(string QuestId, string Location, int DaysIn) : IQuestEvent;

public record CharacterSlayed(string QuestId, string[] CharacterName, int DaysIn) : IQuestEvent;
