using System.Collections.Immutable;

namespace AkkaESPoC.Shared.Queries;

/// <summary>
/// Query to the index actor to retrieve all quests
/// </summary>
public sealed class FetchAllQuests : IAkkaESPoCProtocolMember
{
    public static readonly FetchAllQuests Instance = new();
    private FetchAllQuests(){}
}

public sealed class FetchAllQuestsResponse: IAkkaESPoCProtocolMember
{
    public FetchAllQuestsResponse(IReadOnlyList<QuestData> quests)
    {
        Quests = quests;
    }

    public IReadOnlyList<QuestData> Quests { get; }
}