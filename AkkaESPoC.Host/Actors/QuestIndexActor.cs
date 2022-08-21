using System.Collections.Immutable;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Event;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaESPoC.Shared;
using AkkaESPoC.Shared.Queries;

namespace AkkaESPoC.Host.Actors;

/// <summary>
/// Uses Akka.Persistence.Query to index all actively maintained <see cref="QuestActor"/>.
/// </summary>
public sealed class QuestIndexActor : ReceiveActor
{
    private readonly ILoggingAdapter _logging = Context.GetLogger();
    private readonly IActorRef _shardRegion;
    private readonly IActorRef _mediator;
    private ImmutableDictionary<string, QuestData> _questIds = ImmutableDictionary<string, QuestData>.Empty;

    public QuestIndexActor(IActorRef shardRegion)
    {
        _shardRegion = shardRegion;
        _mediator = Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.Get(Context.System).Mediator;

        Receive<QuestFound>(found =>
        {
            _logging.Info("Found quest [{0}]", found);
            _questIds = _questIds.Add(found.QuestId, QuestData.Empty);
            _shardRegion.Tell(new FetchQuest(found.QuestId));
        });

        Receive<FetchResult>(result =>
        {
            _logging.Info("Received quest state for quest [{0}]", result.State.QuestId);
            _questIds = _questIds.SetItem(result.State.QuestId, result.State.Data);
            _mediator.Tell(new Akka.Cluster.Tools.PublishSubscribe.Publish("questfound", result.State));
        });

        Receive<FetchAllQuests>(f =>
        {
            Sender.Tell(new FetchAllQuestsResponse(_questIds.Values.ToList()));
        });

        Receive<Done>(_ =>
        {
            // this should never happen
            throw new InvalidOperationException("SHOULD NOT REACH END OF ID STREAM");
        });
    }

    private readonly record struct QuestFound(string QuestId);

    private sealed class Done
    {
        public static readonly Done Instance = new();

        private Done()
        {
        }
    }

    protected override void PreStart()
    {
        /*
         * Kicks off an Akka.Persistence.Query instance that will continuously
         */
        var query = Context.System.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier);
        query.PersistenceIds()
            .Where(c => c.StartsWith(QuestActor.QuestEntityNameConstant))
            .Select(c =>
            {
                var splitPivot = c.IndexOf("-", StringComparison.Ordinal);
                return new QuestFound(c[(splitPivot + 1)..]);
            })
            .To(Sink.ActorRef<QuestFound>(Self, Done.Instance))
            .Run(Context.Materializer());
    }
}