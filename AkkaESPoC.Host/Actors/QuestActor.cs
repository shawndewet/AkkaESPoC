using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Dispatch.SysMsg;
using Akka.Event;
using Akka.Persistence;
using AkkaESPoC.Shared;
using AkkaESPoC.Shared.Commands;
using AkkaESPoC.Shared.Events;
using AkkaESPoC.Shared.Queries;

namespace AkkaESPoC.Host.Actors;

/// <summary>
/// Manages the state for a given quest.
/// </summary>
public sealed class QuestActor : ReceivePersistentActor
{
    public static Props GetProps(string persistenceId)
    {
        return Props.Create(() => new QuestActor(persistenceId));
    }
    
    /// <summary>
    /// Used to help differentiate what type of entity this is inside Akka.Persistence's database
    /// </summary>
    public const string QuestEntityNameConstant = "quest";

    private readonly ILoggingAdapter _log = Context.GetLogger();

    public QuestActor(string persistenceId)
    {
        PersistenceId = $"{QuestEntityNameConstant}-" + persistenceId;
        State = new QuestState();
        
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is QuestState state)
            {

               // _log.Info("Recovering Snapshot: {0} *********************************************", offer.Metadata.SequenceNr);

                State = state;
            }
        });

        Recover<IQuestEvent>(questEvent =>
        {
            //_log.Info("Recovering: Event {0} *********************************************", questEvent);

            State = State.ProcessEvent(questEvent);

            //_log.Info($"State is {System.Text.Json.JsonSerializer.Serialize(State.Data)}");

        });

        Command<IQuestCommand>(cmd =>
        {
            var response = State.ProcessCommand(cmd);
            var sentResponse = false; //required because questEvent is called for each response.ResponseEvent
            PersistAllAsync(response.ResponseEvents, questEvent =>
            {
               // _log.Info("Processing Event: {0} *********************************************", questEvent);
                
                State = State.ProcessEvent(questEvent);

               // _log.Info($"New State is {System.Text.Json.JsonSerializer.Serialize(State.Data)}");

                if (!sentResponse) // otherwise we'll generate a response-per-event
                {
                    sentResponse = true;
                    Sender.Tell(response);
                }
                
                if(LastSequenceNr % 10 == 0)
                    SaveSnapshot(State);
            });
        });

        Command<SaveSnapshotSuccess>(success =>
        {
            
        });

        Command<FetchQuest>(fetch =>
        {
            Sender.Tell(new FetchResult(State));

            if (State.IsEmpty)
            {
                // we don't exist, so don't let `remember-entities` keep us alive
                Context.Parent.Tell(new Passivate(PoisonPill.Instance));
            }
        });
    }
    
    public override string PersistenceId { get; }
    
    public QuestState State { get; set; }
}