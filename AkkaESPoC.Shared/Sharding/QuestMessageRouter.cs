using Akka.Cluster.Sharding;

namespace AkkaESPoC.Shared.Sharding;

public sealed class QuestMessageRouter : HashCodeMessageExtractor
{
    public QuestMessageRouter() : base(50) // use a default of 50 shards (5 ActorSystems hosting 10 a-piece)
    {
    }

    public override string? EntityId(object message)
    {
        if(message is IWithQuestId questId)
        {
            return questId.QuestId;
        }

        return null;
    }
}