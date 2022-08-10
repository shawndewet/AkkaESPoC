namespace AkkaESPoC.Shared.Events;

/// <summary>
/// Used to distinguish quest events from commands
/// </summary>
public interface IQuestEvent : IWithQuestId
{
}

public record QuestCreated(string QuestId, string QuestName, string Location) : IQuestEvent;

public record MemberJoined(string QuestId, string MemberName, int DaysIn, DateTime Timestamp) : IQuestEvent, IComparable<MemberJoined>
{
    public int CompareTo(MemberJoined? other)
    {
        if (other == null) return 1;

        return Timestamp.CompareTo(other.Timestamp);
    }
}

public record MemberDeparted(string QuestId, string MemberName, int DaysIn, DateTime Timestamp) : IQuestEvent, IComparable<MemberDeparted>
{
    public int CompareTo(MemberDeparted? other)
    {
        if (other == null) return 1;

        return Timestamp.CompareTo(other.Timestamp);
    }
}

public record ArrivedAtLocation(string QuestId, string Location, int DaysIn, DateTime Timestamp) : IQuestEvent, IComparable<ArrivedAtLocation>
{
    public int CompareTo(ArrivedAtLocation? other)
    {
        if (other == null) return 1;

        return Timestamp.CompareTo(other.Timestamp);
    }
}

public record CharacterSlayed(string QuestId, string CharacterName, int DaysIn, DateTime Timestamp) : IQuestEvent, IComparable<CharacterSlayed>
{
    public int CompareTo(CharacterSlayed? other)
    {
        if (other == null) return 1;

        return Timestamp.CompareTo(other.Timestamp);
    }
}
