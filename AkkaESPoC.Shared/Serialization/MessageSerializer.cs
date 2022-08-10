using System.Collections.Immutable;
using Akka.Actor;
using Akka.Serialization;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Configuration;
using AkkaESPoC.Shared.Events;
using AkkaESPoC.Shared.Queries;
using protoNS = AkkaESPoC.Shared.Serialization.Proto;
using AkkaESPoC.Shared.Commands;
//using FetchAllQuestsResponse = AkkaESPoC.Shared.Queries.FetchAllQuestsResponse;
//using FetchQuest = AkkaESPoC.Shared.Queries.FetchQuest;
//using QuestCreated = AkkaESPoC.Shared.Events.QuestCreated;
//using MemberJoined = AkkaESPoC.Shared.Events.MemberJoined;
//using MemberDeparted = AkkaESPoC.Shared.Events.MemberDeparted;
//using ArrivedAtLocation = AkkaESPoC.Shared.Events.ArrivedAtLocation;
//using CharacterSlayed = AkkaESPoC.Shared.Events.CharacterSlayed;
//using CreateQuest = AkkaESPoC.Shared.Commands.CreateQuest;
//using JoinQuest = AkkaESPoC.Shared.Commands.JoinQuest;
//using DepartQuest = AkkaESPoC.Shared.Commands.DepartQuest;
//using ArriveAtLocation = AkkaESPoC.Shared.Commands.ArriveAtLocation;
//using SlayCharacter = AkkaESPoC.Shared.Commands.SlayCharacter;

namespace AkkaESPoC.Shared.Serialization;

public sealed class MessageSerializer : SerializerWithStringManifest
{
    public MessageSerializer(ExtendedActorSystem system) : base(system)
    {
    }

    public const string CreateQuestManifest = "cq";
    public const string JoinQuestManifest = "jq";
    public const string DepartQuestManifest = "dq";
    public const string SlayCharacterManifest = "sc";
    public const string ArriveAtLocationManifest = "alc";

    public const string QuestStateManifest = "qs";
    public const string QuestCreatedManifest = "qc";
    public const string MemberJoinedManifest = "mj";
    public const string MemberDepartedManifest = "md";
    public const string CharacterSlayedManifest = "cs";
    public const string ArrivedAtLocationManifest = "ale";

    public const string QuestCommandResponseManifest = "qcr";
    public const string FetchAllQuestsManifest = "fall";
    public const string FetchAllQuestsResponseManifest = "fallrsp";
    public const string FetchQuestManifest = "fq";
    public const string FetchQuestResultManifest = "fqr";

    /// <summary>
    /// Unique value greater than 100 as [0-100] is reserved for Akka.NET System serializers. 
    /// </summary>
    public override int Identifier => 556; //(int serializerId, string manifest)

    public override byte[] ToBinary(object obj)
    {
        switch (obj)
        {
            case QuestCommandResponse qcr:
                return ToProto(qcr).ToByteArray();
            case QuestState qs:
                return ToProto(qs).ToByteArray();
            case FetchQuest fq:
                return ToProto(fq).ToByteArray();
            case FetchResult fr:
                return ToProto(fr.State).ToByteArray();
            case FetchAllQuests _:
                return Array.Empty<byte>();
            case FetchAllQuestsResponse rsp:
                return ToProto(rsp).ToByteArray();
            case CreateQuest cp:
                return ToProto(cp).ToByteArray();
            case JoinQuest jq:
                return ToProto(jq).ToByteArray();
            case DepartQuest dq:
                return ToProto(dq).ToByteArray();
            case SlayCharacter sc:
                return ToProto(sc).ToByteArray();
            case ArriveAtLocation alc:
                return ToProto(alc).ToByteArray();

            case QuestCreated pc:
                return ToProto(pc).ToByteArray();
            case MemberJoined mj:
                return ToProto(mj).ToByteArray();
            case MemberDeparted md:
                return ToProto(md).ToByteArray();
            case CharacterSlayed cs:
                return ToProto(cs).ToByteArray();
            case ArrivedAtLocation ale:
                return ToProto(ale).ToByteArray();


            default:
                throw new ArgumentOutOfRangeException(nameof(obj), $"Unsupported message type [{obj.GetType()}]");
        }
    }

    public override object FromBinary(byte[] bytes, string manifest)
    {
        switch (manifest)
        {
            case QuestCommandResponseManifest:
                return FromProto(protoNS.QuestCommandResponse.Parser.ParseFrom(bytes));
            case QuestStateManifest:
                return FromProto(protoNS.QuestState.Parser.ParseFrom(bytes));
            case FetchQuestManifest:
                return FromProto(protoNS.FetchQuest.Parser.ParseFrom(bytes));
            case FetchQuestResultManifest:
                return new FetchResult(FromProto(protoNS.QuestState.Parser.ParseFrom(bytes)));
            case FetchAllQuestsManifest:
                return FetchAllQuests.Instance;
            case FetchAllQuestsResponseManifest:
                return FromProto(protoNS.FetchAllQuestsResponse.Parser.ParseFrom(bytes));

            case CreateQuestManifest:
                return FromProto(protoNS.CreateQuest.Parser.ParseFrom(bytes));
            case JoinQuestManifest:
                return FromProto(protoNS.JoinQuest.Parser.ParseFrom(bytes));
            case DepartQuestManifest:
                return FromProto(protoNS.DepartQuest.Parser.ParseFrom(bytes));
            case SlayCharacterManifest:
                return FromProto(protoNS.SlayCharacter.Parser.ParseFrom(bytes));
            case ArriveAtLocationManifest:
                return FromProto(protoNS.ArriveAtLocation.Parser.ParseFrom(bytes));

            case QuestCreatedManifest:
                return FromProto(protoNS.QuestCreated.Parser.ParseFrom(bytes));
            case MemberJoinedManifest:
                return FromProto(protoNS.MemberJoined.Parser.ParseFrom(bytes));
            case MemberDepartedManifest:
                return FromProto(protoNS.MemberDeparted.Parser.ParseFrom(bytes));
            case ArrivedAtLocationManifest:
                return FromProto(protoNS.ArrivedAtLocation.Parser.ParseFrom(bytes));
            case CharacterSlayedManifest:
                return FromProto(protoNS.CharacterSlayed.Parser.ParseFrom(bytes));

            default:
                throw new ArgumentOutOfRangeException(nameof(manifest), $"Unsupported message manifest [{manifest}]");
        }
    }

    public override string Manifest(object o)
    {
        switch (o)
        {

            case QuestCommandResponse _:
                return QuestCommandResponseManifest;
            case QuestState _:
                return QuestStateManifest;
            case FetchQuest _:
                return FetchQuestManifest;
            case FetchResult _:
                return FetchQuestResultManifest;
            case FetchAllQuests _:
                return FetchAllQuestsManifest;
            case FetchAllQuestsResponse _:
                return FetchAllQuestsResponseManifest;

            case CreateQuest _:
                return CreateQuestManifest;
            case JoinQuest _:
                return JoinQuestManifest;
            case DepartQuest _:
                return DepartQuestManifest;
            case SlayCharacter _:
                return SlayCharacterManifest;
            case ArriveAtLocation _:
                return ArriveAtLocationManifest;

            case QuestCreated _:
                return QuestCreatedManifest;
            case MemberJoined _:
                return MemberJoinedManifest;
            case MemberDeparted _:
                return MemberDepartedManifest;
            case CharacterSlayed _:
                return CharacterSlayedManifest;
            case ArrivedAtLocation _:
                return ArrivedAtLocationManifest;
            default:
                throw new ArgumentOutOfRangeException(nameof(o), $"Unsupported message type [{o.GetType()}]");
        }
    }


    private static protoNS.FetchQuest ToProto(FetchQuest fq)
    {
        var proto = new protoNS.FetchQuest();
        proto.QuestId = fq.QuestId;
        return proto;
    }

    private static FetchQuest FromProto(protoNS.FetchQuest proto)
    {
        return new FetchQuest(proto.QuestId);
    }

    private static FetchAllQuestsResponse FromProto(protoNS.FetchAllQuestsResponse proto)
    {
        return new FetchAllQuestsResponse(proto.Quests.Select(c => FromProto(c)).ToList());
    }

    private static protoNS.FetchAllQuestsResponse ToProto(FetchAllQuestsResponse faqr)
    {
        var proto = new protoNS.FetchAllQuestsResponse();
        proto.Quests.AddRange(faqr.Quests.Select(c => ToProto(c)));
        return proto;
    }


    private static protoNS.QuestState ToProto(QuestState state)
    {
        var protoState = new protoNS.QuestState();
        protoState.Data = ToProto(state.Data);
        protoState.MembersJoined.AddRange(state.MembersJoined.Select(c => ToProto(c)));
        protoState.MembersDeparted.AddRange(state.MembersDeparted.Select(c => ToProto(c)));
        protoState.LocationArrivals.AddRange(state.LocationArrivals.Select(c => ToProto(c)));
        protoState.CharactersSlayed.AddRange(state.CharactersSlayed.Select(c => ToProto(c)));

        return protoState;
    }

    private static QuestState FromProto(protoNS.QuestState protoState)
    {
        var questState = new QuestState
        {
            Data = FromProto(protoState.Data),
            MembersJoined = protoState.MembersJoined.Select(c => FromProto(c)).ToImmutableSortedSet(),
            MembersDeparted = protoState.MembersDeparted.Select(c => FromProto(c)).ToImmutableSortedSet(),
            LocationArrivals = protoState.LocationArrivals.Select(c => FromProto(c)).ToImmutableSortedSet(),
            CharactersSlayed = protoState.CharactersSlayed.Select(c => FromProto(c)).ToImmutableSortedSet()
        };

        return questState;
    }

    private static protoNS.QuestCommandResponse ToProto(QuestCommandResponse pcr)
    {
        var rsp = new protoNS.QuestCommandResponse();
        rsp.Events.AddRange(pcr.ResponseEvents.Select(c => ToQuestEvent(c)));
        rsp.Message = pcr.Message;
        rsp.Success = pcr.Success;
        rsp.QuestId = pcr.QuestId;
        return rsp;
    }

    private static QuestCommandResponse FromProto(protoNS.QuestCommandResponse pcr)
    {
        var rsp = new QuestCommandResponse(pcr.QuestId,
            pcr.Events.Select(c => FromQuestEvent(c)).ToArray(), pcr.Success, pcr.Message);
        return rsp;
    }

    private static protoNS.QuestEvent ToQuestEvent(IQuestEvent e)
    {
        var questEvent = new protoNS.QuestEvent();
        switch (e)
        {
            case QuestCreated created:
                questEvent.QuestCreated = ToProto(created);
                break;
            case MemberJoined joined:
                questEvent.MemberJoined = ToProto(joined);
                break;
            case MemberDeparted departed:
                questEvent.MemberDeparted = ToProto(departed);
                break;
            case ArrivedAtLocation arrived:
                questEvent.ArrivedAtLocation = ToProto(arrived);
                break;
            case CharacterSlayed slayed:
                questEvent.CharacterSlayed = ToProto(slayed);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e, null);
        }

        return questEvent;
    }

    private static IQuestEvent FromQuestEvent(protoNS.QuestEvent e)
    {
        if (e.QuestCreated != null)
        {
            return FromProto(e.QuestCreated);
        }

        if (e.MemberJoined != null)
        {
            return FromProto(e.MemberJoined);
        }

        if (e.MemberDeparted != null)
        {
            return FromProto(e.MemberDeparted);
        }

        if (e.CharacterSlayed != null)
        {
            return FromProto(e.CharacterSlayed);
        }

        if (e.ArrivedAtLocation != null)
        {
            return FromProto(e.ArrivedAtLocation);
        }

        throw new ArgumentException("Did not find matching CLR type for QuestEvent");
    }

    private static protoNS.CreateQuest ToProto(CreateQuest createQuest)
    {
        var proto = new protoNS.CreateQuest();
        proto.QuestId = createQuest.QuestId;
        proto.QuestName = createQuest.QuestName;
        proto.Location = createQuest.Location;
        return proto;
    }

    private static CreateQuest FromProto(protoNS.CreateQuest protoCreate)
    {
        var createQuest = new CreateQuest(protoCreate.QuestId,
            protoCreate.QuestName,
            protoCreate.Location,
            new string[] { });
        return createQuest;
    }

    private static protoNS.JoinQuest ToProto(JoinQuest jq)
    {
        var proto = new protoNS.JoinQuest();
        proto.DaysIn = jq.DaysIn;
        proto.QuestId = jq.QuestId;
        proto.MemberNames.AddRange(jq.MemberNames);
        return proto;
    }

    private static JoinQuest FromProto(protoNS.JoinQuest proto)
    {
        var supply = new JoinQuest(proto.QuestId, proto.DaysIn, proto.MemberNames.ToArray());
        return supply;
    }

    private static protoNS.DepartQuest ToProto(DepartQuest dq)
    {
        var proto = new protoNS.DepartQuest();
        proto.DaysIn = dq.DaysIn;
        proto.QuestId = dq.QuestId;
        proto.MemberNames.AddRange(dq.MemberNames);
        return proto;
    }

    private static DepartQuest FromProto(protoNS.DepartQuest proto)
    {
        var supply = new DepartQuest(proto.QuestId, proto.DaysIn, proto.MemberNames.ToArray());
        return supply;
    }

    private static protoNS.SlayCharacter ToProto(SlayCharacter sc)
    {
        var proto = new protoNS.SlayCharacter();
        proto.DaysIn = sc.DaysIn;
        proto.QuestId = sc.QuestId;
        proto.CharacterNamesNames.AddRange(sc.CharacterNames);
        return proto;
    }

    private static SlayCharacter FromProto(protoNS.SlayCharacter proto)
    {
        var supply = new SlayCharacter(proto.QuestId, proto.DaysIn, proto.CharacterNamesNames.ToArray());
        return supply;
    }

    private static protoNS.ArriveAtLocation ToProto(ArriveAtLocation aal)
    {
        var proto = new protoNS.ArriveAtLocation();
        proto.DaysIn = aal.DaysIn;
        proto.QuestId = aal.QuestId;
        proto.Location = aal.Location;
        return proto;
    }

    private static ArriveAtLocation FromProto(protoNS.ArriveAtLocation proto)
    {
        var supply = new ArriveAtLocation(proto.QuestId, proto.DaysIn, proto.Location);
        return supply;
    }




    private static protoNS.QuestCreated ToProto(QuestCreated created)
    {
        var questCreated = new protoNS.QuestCreated
        {
            Data = CreateQuestData(created)
        };
        return questCreated;
    }

    private static QuestCreated FromProto(protoNS.QuestCreated created)
    {
        return new QuestCreated(created.Data.QuestId, created.Data.QuestName, created.Data.Location);
    }

    private static protoNS.MemberJoined ToProto(MemberJoined entity)
    {
        var proto = new protoNS.MemberJoined
        {
            DaysIn = entity.DaysIn,
            QuestId = entity.QuestId,
            MemberName = entity.MemberName,
            Timestamp = entity.Timestamp.ToTimestamp()
        };
        return proto;
    }

    private static MemberJoined FromProto(protoNS.MemberJoined proto)
    {
        return new MemberJoined(proto.QuestId, proto.MemberName, proto.DaysIn, proto.Timestamp == null ? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) : proto.Timestamp.ToDateTime());
    }

    private static protoNS.MemberDeparted ToProto(MemberDeparted entity)
    {
        var proto = new protoNS.MemberDeparted
        {
            DaysIn = entity.DaysIn,
            QuestId = entity.QuestId,
            MemberName = entity.MemberName,
            Timestamp = entity.Timestamp.ToTimestamp()
        };

        return proto;
    }

    private static MemberDeparted FromProto(protoNS.MemberDeparted proto)
    {
        return new MemberDeparted(proto.QuestId, proto.MemberName, proto.DaysIn, proto.Timestamp.ToDateTime());
    }

    private static protoNS.CharacterSlayed ToProto(CharacterSlayed entity)
    {
        var proto = new protoNS.CharacterSlayed
        {
            DaysIn = entity.DaysIn,
            QuestId = entity.QuestId,
            CharacterName = entity.CharacterName,
            Timestamp = entity.Timestamp.ToTimestamp()
        };

        return proto;
    }

    private static CharacterSlayed FromProto(protoNS.CharacterSlayed proto)
    {
        return new CharacterSlayed(proto.QuestId, proto.CharacterName, proto.DaysIn, proto.Timestamp.ToDateTime());
    }

    private static protoNS.ArrivedAtLocation ToProto(ArrivedAtLocation entity)
    {
        return new protoNS.ArrivedAtLocation
        {
            DaysIn = entity.DaysIn,
            QuestId = entity.QuestId,
            Location = entity.Location,
            Timestamp = entity.Timestamp.ToTimestamp()
        };
    }

    private static ArrivedAtLocation FromProto(protoNS.ArrivedAtLocation proto)
    {
        return new ArrivedAtLocation(proto.QuestId, proto.Location, proto.DaysIn, proto.Timestamp.ToDateTime());
    }


    private static protoNS.QuestData CreateQuestData(QuestCreated created)
    {
        return new protoNS.QuestData()
        {
            QuestId = created.QuestId,
            QuestName = created.QuestName,
            Location = created.Location,
        };
    }

    private static QuestData FromProto(protoNS.QuestData data)
    {
        return new QuestData(data.QuestId, data.QuestName, data.Location, data.DaysIn, data.Members.ToArray(), data.Slayed.ToArray());
    }

    private static protoNS.QuestData ToProto(QuestData data)
    {
        var protoData = new protoNS.QuestData
        {
            QuestId = data.QuestId,
            QuestName = data.QuestName,
            Location = data.Location,
            DaysIn = data.DaysIn
        };
        protoData.Members.AddRange(data.Members);
        protoData.Slayed.AddRange(data.Slayed);

        return protoData;
    }

}

public static class ProtoSerializationExtensions
{
    private const decimal NanoFactor = 1_000_000_000;

    public static decimal ToDecimal(this protoNS.DecimalValue grpcDecimal)
    {
        return grpcDecimal.Units + grpcDecimal.Nanos / NanoFactor;
    }

    public static protoNS.DecimalValue FromDecimal(this decimal value)
    {
        var units = decimal.ToInt64(value);
        var nanos = decimal.ToInt32((value - units) * NanoFactor);
        return new protoNS.DecimalValue() { Units = units, Nanos = nanos };
    }
}