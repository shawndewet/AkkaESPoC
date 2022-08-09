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
                        var questCreated = new QuestCreated(create.QuestId, create.QuestName, create.Location);
                        var memberJoined = new MemberJoined(create.QuestId, create.InitialMembers, 0);

                        var response = new QuestCommandResponse(create.QuestId, new IQuestEvent[] { questCreated, memberJoined });
                        return response;
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
                    var memberJoined = new MemberJoined(questId, memberNames, daysIn);
                    var response = new QuestCommandResponse(questId, new IQuestEvent[] { memberJoined });
                    return response;
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
                    var memberDeparted = new MemberDeparted(questId, memberNames, daysIn);
                    var response = new QuestCommandResponse(questId, new IQuestEvent[] { memberDeparted });
                    return response;
                }
            case ArriveAtLocation(string questId, int daysIn, string location) when !IsEmpty:
                {
                    if (Data.Location == location)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Quest is already at {location}");

                    if (Data.DaysIn > daysIn)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Cannot arrive earlier than {Data.DaysIn} days into the quest");

                    var arrivedAtLocation = new ArrivedAtLocation(questId, location, daysIn);
                    var response = new QuestCommandResponse(questId,
                    new IQuestEvent[] { arrivedAtLocation });
                    return response;
                }
            case SlayCharacter(string questId, int daysIn, string[] characterName) when !IsEmpty:
                {
                    if (Data.DaysIn > daysIn)
                        return new QuestCommandResponse(questCommand.QuestId, Array.Empty<IQuestEvent>(), false,
                                $"Cannot slay earlier than {Data.DaysIn} days into the quest");

                    var characterSlayed = new CharacterSlayed(questId, characterName, daysIn);
                    var response = new QuestCommandResponse(questId,
                    new IQuestEvent[] { characterSlayed });
                    return response;
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
            case QuestCreated(string questId, string questName, string location):
                {
                    return this with
                    {
                        Data = Data with
                        {
                            QuestId = questId,
                            QuestName = questName,
                            Location = location
                        }
                    };
                }
            case MemberJoined(var questId, var memberName, var daysIn) @event:
                {
                    var newMembers = this.Data.Members.ToList();
                    newMembers.AddRange(memberName);

                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = daysIn,
                            Members = newMembers.ToArray()
                        },
                        MembersJoined = MembersJoined.Add(@event)
                    };
                }
            case MemberDeparted(var questId, var memberName, var daysIn) @event:
                {
                    var newMembers = this.Data.Members.ToList();
                    foreach (var item in memberName)
                        newMembers.Remove(item);
                    

                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = daysIn,
                            Members = newMembers.ToArray()
                        },
                        MembersDeparted = MembersDeparted.Add(@event)
                    };
                }
            case ArrivedAtLocation(var questId, var locationName, var daysIn) @event:
                {
                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = daysIn,
                            Location = locationName
                        },
                        LocationArrivals = LocationArrivals.Add(@event)
                    };
                }
            case CharacterSlayed(var questId, var characterNames, var daysIn) @event:
                {
                    var newSlayed = this.Data.Slayed.ToList();
                    newSlayed.AddRange(characterNames);

                    return this with
                    {
                        Data = Data with
                        {
                            DaysIn = daysIn,
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