// See https://aka.ms/new-console-template for more information

using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Akka.Persistence.SqlServer.Hosting;
using Akka.Remote.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using AkkaESPoC.Host.Actors;
using AkkaESPoC.Shared.Sharding;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var builder = new HostBuilder()
    .ConfigureAppConfiguration(c => c.AddEnvironmentVariables()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{environment}.json"))
    .ConfigureServices((context, services) =>
    {
        // maps to environment variable ConnectionStrings__AkkaSqlConnection
        var connectionString = context.Configuration.GetConnectionString("AkkaSqlConnection");


        var akkaSection = context.Configuration.GetSection("Akka");

        // maps to environment variable Akka__ClusterIp
        var hostName = akkaSection.GetValue<string>("ClusterIp", "localhost");

        // maps to environment variable Akka__ClusterPort
        var port = akkaSection.GetValue<int>("ClusterPort", 7919);

        var seeds = akkaSection.GetValue<string[]>("ClusterSeeds", new []{ "akka.tcp://AkkaESPoC@localhost:7919" }).Select(Address.Parse)
            .ToArray();

        services.AddAkka("AkkaESPoC", (configurationBuilder, provider) =>
        {
            configurationBuilder
                .WithRemoting(hostName, port)
                .WithCustomSerializer("hyperion", new[] { typeof(object) }, system => new Akka.Serialization.HyperionSerializer(system))
                .WithClustering(new ClusterOptions()
                    { Roles = new[] { QuestActorProps.SingletonActorRole }, SeedNodes = seeds })
                .WithSqlServerPersistence(connectionString)
                .WithShardRegion<QuestMarker>("quests", s => QuestActor.GetProps(s),
                    new QuestMessageRouter(),
                    new ShardOptions()
                    {
                        RememberEntities = true, Role = QuestActorProps.SingletonActorRole,
                        StateStoreMode = StateStoreMode.DData
                    })
                .StartActors((system, registry) =>
                {
                    var shardRegion = registry.Get<QuestMarker>();

                    var indexProps = Props.Create(() => new QuestIndexActor(shardRegion));
                    var singletonProps = system.QuestSingletonProps(indexProps);
                    registry.TryRegister<QuestIndexActor>(system.ActorOf(singletonProps,
                        QuestActorProps.SingletonActorName));

                    // don't really need the ClusterSingletonProxy in this service, but it doesn't hurt to have it
                    // in case we do want to message the Singleton directly from the Host node
                    var proxyProps = system.QuestIndexProxyProps();
                    registry.TryRegister<QuestIndexMarker>(system.ActorOf(proxyProps, "quest-proxy"));
                });

        });
    })
    .Build();

await builder.RunAsync();