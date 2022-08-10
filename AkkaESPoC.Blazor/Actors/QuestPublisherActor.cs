using Akka.Actor;
using Akka.Event;
using AkkaESPoC.Blazor.Hubs;

namespace AkkaESPoC.Blazor.Actors
{
    public class QuestPublisherActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly QuestHub _hub;

        //public QuestPublisherActor(QuestHub hub)
        //{
        //    _hub = hub;

        //    ReceiveAsync<IPriceUpdate>(async p =>
        //    {
        //        try
        //        {
        //            _log.Info("Received event {0}", p);
        //            await hub.WritePriceChanged(p);
        //        }
        //        catch (Exception ex)
        //        {
        //            _log.Error(ex, "Error while writing price update [{0}] to StockHub", p);
        //        }
        //    });

        //    ReceiveAsync<IVolumeUpdate>(async p =>
        //    {
        //        try
        //        {
        //            _log.Info("Received event {0}", p);
        //            await hub.WriteVolumeChanged(p);
        //        }
        //        catch (Exception ex)
        //        {
        //            _log.Error(ex, "Error while writing volume update [{0}] to StockHub", p);
        //        }
        //    });
        //}
    }
}