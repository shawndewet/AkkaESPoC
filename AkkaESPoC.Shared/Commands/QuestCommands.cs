using AkkaESPoC.Shared.Events;

namespace AkkaESPoC.Shared.Commands;

/// <summary>
/// Used to distinguish quest events from commands
/// </summary>
public interface IQuestCommand : IWithQuestId{}

public record CreateQuest(string QuestId, string QuestName, string Location, string[] InitialMembers) : IQuestCommand;

public record JoinQuest(string QuestId, int DaysIn, string[] MemberNames) : IQuestCommand;

public record DepartQuest(string QuestId, int DaysIn, string[] MemberNames) : IQuestCommand;

public record ArriveAtLocation(string QuestId, int DaysIn, string Location) : IQuestCommand;

public record SlayCharacter(string QuestId, int DaysIn, string[] CharacterNames) : IQuestCommand;

public record QuestCommandResponse(string QuestId, IReadOnlyCollection<IQuestEvent> ResponseEvents, bool Success = true, string Message = "") : IWithQuestId;