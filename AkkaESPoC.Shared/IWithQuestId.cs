namespace AkkaESPoC.Shared;

/// <summary>
/// Marker interface for all commands and events associated with a quest.
/// </summary>
public interface IWithQuestId : IAkkaESPoCProtocolMember
{
    string QuestId { get; }
}