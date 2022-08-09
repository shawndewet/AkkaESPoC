namespace AkkaESPoC.Shared.Queries;

/// <summary>
/// Fetch a particular quest
/// </summary>
public sealed class FetchQuest : IWithQuestId
{
    public FetchQuest(string questId)
    {
        QuestId = questId;
    }

    public string QuestId { get; }
}

public sealed class FetchResult : IAkkaESPoCProtocolMember
{
    public FetchResult(QuestState state)
    {
        State = state;
    }

    public QuestState State { get; }
}