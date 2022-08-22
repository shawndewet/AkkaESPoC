using Akka.Actor;
using Akka.Event;
using AkkaESPoC.Blazor.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Immutable;
using Akka.Cluster.Tools.Singleton;
using AkkaESPoC.Shared;
using AkkaESPoC.Shared.Queries;



namespace AkkaESPoC.Blazor.Actors
{
    public class QuestViewActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly IActorRef _mediator;
        private readonly IHubContext<QuestHub> _hubContext;

        public QuestViewActor(IHubContext<QuestHub> hubContext)
        {
            _log = Context.GetLogger();

            _log.Info("Instantiating QuestViewActor...........................");
            
            _mediator = Akka.Cluster.Tools.PublishSubscribe.DistributedPubSub.Get(Context.System).Mediator;
            _mediator.Tell(new Akka.Cluster.Tools.PublishSubscribe.Subscribe("questfound", Self));
            Receive<Akka.Cluster.Tools.PublishSubscribe.SubscribeAck>(ack =>
            {
                _log.Info($"Received SubscribeAck with Topic {ack.Subscribe.Topic} and ref eq self? {ack.Subscribe.Ref.Equals(Self)}");

                if (ack != null && ack.Subscribe.Topic == "questfound" && ack.Subscribe.Ref.Equals(Self))
                {
                    _log.Info($"Become Ready");

                    Become(Ready);
                }
                else
                {
                    _log.Info($"Ref is {ack.Subscribe.Ref}");
                }
            });

            _hubContext = hubContext;
        }

        private void Ready()
        {
            Receive<QuestState>(async message =>
            {
                _log.Info("Got QuestState {0}", message.QuestId);

                await _hubContext.Clients.All.SendAsync("AddQuest", message.Data);
                _log.Info($"Sent AddQuest for {message.QuestId}");
            });
        }

        protected override void PostStop()
        {
            base.PostStop();
        }
    }
}