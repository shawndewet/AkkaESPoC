using Akka.Actor;
using Akka.Event;
using AkkaESPoC.Blazor.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Immutable;
using Akka.Cluster.Tools.Singleton;
using Akka.Persistence.Query;
using Akka.Persistence.Query.Sql;
using Akka.Streams;
using Akka.Streams.Dsl;
using AkkaESPoC.Shared;
using AkkaESPoC.Shared.Queries;



namespace AkkaESPoC.Blazor.Actors
{
    public class QuestViewActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        //private readonly QuestHubHelper _questHub;
        private readonly IActorRef _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IHubContext<QuestHub> _hubContext;

        public QuestViewActor(IHubContext<QuestHub> hubContext) // IServiceProvider serviceProvider)
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
            //_serviceProvider = serviceProvider;
            //_scope = serviceProvider.CreateScope();
            _hubContext = hubContext;
        }
        private void Ready()
        {
            Receive<QuestState>(async message =>
            {
                _log.Info("Got QuestState {0}", message.QuestId);

                //var _questHub = Akka.DependencyInjection.DependencyResolver.For(Context.System).Resolver.GetService<IHubContext<QuestHub>>();
                //var _questHub = _serviceProvider.GetRequiredService<IHubContext<QuestHub>>();
                //var _questHub = _scope.ServiceProvider.GetRequiredService<IHubContext<QuestHub>>();
                //var _questHubHelper = _scope.ServiceProvider.GetRequiredService<QuestHubHelper>();
                //await _questHubHelper.AddQuest(message.Data);
                //var counter = Akka.DependencyInjection.DependencyResolver.For(Context.System).Resolver.GetService<SignalRConnectionList>();
                //var counter = _serviceProvider.GetRequiredService<SignalRConnectionList>();
                //var counter = _scope.ServiceProvider.GetRequiredService<SignalRConnectionList>();
                //_log.Info($"Have {counter.Count} connections");

                //await _questHub.Clients.All.SendAsync("AddQuest", message.Data);

                await _hubContext.Clients.All.SendAsync("AddQuest", message.Data.QuestId);
                _log.Info($"Sent AddQuest for {message.QuestId}");

                
            });
        }

        //protected override void PreStart()
        //{

        //    Context.System.ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier)
        //        .event()
        //        .Where(c => c.StartsWith(QuestActor.QuestEntityNameConstant))
        //        .Select(c =>
        //        {
        //            var splitPivot = c.IndexOf("-", StringComparison.Ordinal);
        //            _logging.Info($"***************** return new QuestFound for persistenceId {c}");
        //            return new QuestFound(c[(splitPivot + 1)..]);
        //        })
        //        .To(Sink.ActorRef<QuestFound>(Self, Done.Instance))
        //        .Run(Context.Materializer());

        //    base.PreStart();
        //}

        protected override void PostStop()
        {
            _scope.Dispose();
            base.PostStop();
        }
    }
}