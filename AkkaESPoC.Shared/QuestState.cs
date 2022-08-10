using System.Collections.Immutable;
using AkkaESPoC.Shared.Commands;
using AkkaESPoC.Shared.Events;

namespace AkkaESPoC.Shared;

public record QuestData(string QuestId, string QuestName, string Location, int DaysIn, string[] Members, string[] Slayed) : IAkkaESPoCProtocolMember
{
    public static readonly QuestData Empty = new(string.Empty, string.Empty, string.Empty, 0, new string[] { }, new string[] { });
}

/// <summary>
/// The state object responsible for all event and message processing
/// </summary>
public record QuestState : IWithQuestId
{
    public QuestData Data { get; init; } = QuestData.Empty;

    public string QuestId => Data.QuestId;

    public ImmutableSortedSet<MemberJoined> MembersJoined { get; init; } = ImmutableSortedSet<MemberJoined>.Empty;

    public ImmutableSortedSet<MemberDeparted> MembersDeparted { get; init; } = ImmutableSortedSet<MemberDeparted>.Empty;

    public ImmutableSortedSet<ArrivedAtLocation> LocationArrivals { get; init; } = ImmutableSortedSet<ArrivedAtLocation>.Empty;

    public ImmutableSortedSet<CharacterSlayed> CharactersSlayed { get; init; } = ImmutableSortedSet<CharacterSlayed>.Empty;


    public bool IsEmpty => Data == QuestData.Empty;


    /// <summary>
    /// Stateful processing of commands. Performs input validation et al.
    /// </summary>
    /// <remarks>
    /// Intentionally kept simple.
    /// </remarks>
    /// <param name="questCommand">The command to process.</param>
    /// <returns></returns>
    public QuestCommandResponse ProcessCommand(IQuestCommand questCommand)
    {
        switch (questCommand)
        {
            case CreateQuest create when IsEmpty:
                {
                    if (IsEmpty)
                    {
                        // not initialized, can create

                        // events
                        var events = new List<IQuestEvent>();
                        events.Add(new QuestCreated(create.QuestId, create.QuestName, create.Location));
                        foreach (var item in create.InitialMembers)
                            events.Add(new MemberJoined(create.QuestId, item, 0, DateTime.UtcNow));

                        return new QuestCommandResponse(create.QuestId, events);
                    }
                    else
                    {
                        return new QuestCommandResponse(Data.QuestId, Array.Empty<IQuestEvent>(), false,
                            $"Quest with [Id={Data.QuestId}] already exists");
                    }
                }
            case JoinQuest(string questId, int daysIn, string[] memberNames) when !IsEmpty:
                {
                    if (Data.DaysIn > daysIn)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Cannot join earlier than {Data.DaysIn} days into the quest");

                    foreach (var item in memberNames)
                    {
                        if (Data.Members.Contains(item))
                            return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Member {item} already in Quest");
                    }
                    
                    var events = new List<IQuestEvent>();
                    foreach (var item in memberNames)
                        events.Add(new MemberJoined(questId, item, daysIn, DateTime.UtcNow));

                    return new QuestCommandResponse(questId, events);
                }
            case DepartQuest(string questId, int daysIn, string[] memberNames) when !IsEmpty:
                {
                    if (Data.DaysIn > daysIn)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Cannot depart earlier than {Data.DaysIn} days into the quest");

                    foreach (var item in memberNames)
                    {
                        if (!Data.Members.Contains(item))
                            return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Member {item} not in Quest");
                    }
                    var events = new List<IQuestEvent>();
                    foreach (var item in memberNames)
                        events.Add(new MemberDeparted(questId, item, daysIn, DateTime.UtcNow));

                    return new QuestCommandResponse(questId, events);
                }
            case ArriveAtLocation(string questId, int daysIn, string location) when !IsEmpty:
                {
                    if (Data.Location == location)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Quest is already at {location}");

                    if (Data.DaysIn > daysIn)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Cannot arrive earlier than {Data.DaysIn} days into the quest");

                    var arrivedAtLocation = new ArrivedAtLocation(questId, location, daysIn, DateTime.UtcNow);
                    var response = new QuestCommandResponse(questId,
                    new IQuestEvent[] { arrivedAtLocation });
                    return response;
                }
            case SlayCharacter(string questId, int daysIn, string[] characterName) when !IsEmpty:
                {
                    if (Data.DaysIn > daysIn)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Cannot slay earlier than {Data.DaysIn} days into the quest");

                    var events = new List<IQuestEvent>();
                    foreach (var item in characterName)
                        events.Add(new CharacterSlayed(questId, item, daysIn, DateTime.UtcNow));

                    return new QuestCommandResponse(questId, events);
                }
            default:
                {
                    return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                        $"Quest with [Id={Data.QuestId}] is not ready to process command [{questCommand}]");
                }
        }
    }

    public QuestState ProcessEvent(IQuestEvent questEvent)
    {
        switch (questEvent)
        {
            case QuestCreated @event:
                {
                    return this with
                    {
                        Data = Data with
                        {
                            QuestId = @event.QuestId,
                            QuestName = @event.QuestName,
                            Location = @event.Location
                        }
                    };
                }
            case MemberJoined @event:
                {
                    var newMembers = this.Data.Members.ToList();
                    newMembers.Add(@event.MemberName);

                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = @event.DaysIn,
                            Members = newMembers.ToArray()
                        },
                        MembersJoined = MembersJoined.Add(@event)
                    };
                }
            case MemberDeparted @event:
                {
                    var newMembers = this.Data.Members.ToList();
                        newMembers.Remove(@event.MemberName);

                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = @event.DaysIn,
                            Members = newMembers.ToArray()
                        },
                        MembersDeparted = MembersDeparted.Add(@event)
                    };
                }
            case ArrivedAtLocation @event:
                {
                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = @event.DaysIn,
                            Location = @event.Location
                        },
                        LocationArrivals = LocationArrivals.Add(@event)
                    };
                }
            case CharacterSlayed @event:
                {
                    var newSlayed = this.Data.Slayed.ToList();
                    newSlayed.Add(@event.CharacterName);

                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = @event.DaysIn,
                            Slayed = newSlayed.ToArray()
                        },
                        CharactersSlayed = CharactersSlayed.Add(@event)
                    };
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(questEvent));
        }
    }
}