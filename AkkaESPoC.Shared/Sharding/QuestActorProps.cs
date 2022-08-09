using Akka.Actor;
using Akka.Cluster.Tools.Singleton;

namespace AkkaESPoC.Shared.Sharding;

/// <summary>
/// Key type for DI
/// </summary>
public sealed class QuestIndexMarker
{
    public static readonly QuestIndexMarker Instance = new();
    private QuestIndexMarker(){}
}

/// <summary>
/// Key type for DI
/// </summary>
public sealed class QuestMarker
{
    public static readonly QuestMarker Instance = new QuestMarker();
    private QuestMarker(){}
}

/// <summary>
/// Utility classes for creating singleton proxies / singletons for quest actors
/// </summary>
public static class QuestActorProps
{
    public const string SingletonActorName = "quest-index";
    public const string SingletonActorRole = "host";
    
    public static Props QuestSingletonProps(this ActorSystem system, Props underlyingProps)
    {
        return ClusterSingletonManager.Props(underlyingProps,
            ClusterSingletonManagerSettings.Create(system).WithRole(SingletonActorRole).WithSingletonName(SingletonActorName));
    }
    
    public static Props QuestIndexProxyProps(this ActorSystem system)
    {
        return ClusterSingletonProxy.Props($"/user/{SingletonActorName}",
            ClusterSingletonProxySettings.Create(system).WithRole(SingletonActorRole).WithSingletonName(SingletonActorName));
    }
}