(this PoC was based on this sample: https://github.com/petabridge/akkadotnet-code-samples/tree/master/src/clustering/sharding-sqlserver)

# Akka.NET Cluster.Sharding with Akka.Persistence.SqlServer and Razor Pages

## Running Sample

### Launch Dependencies

To run this sample, we first need to spin up our dependencies - a SQL Server instance with a predefined `Akka` database ready to be configured.

**Windows**

```shell
start-dependencies.cmd
```

**Linux or OS X**

```shell
./start-dependencies.sh
```

This will build a copy of our [MSSQL image](https://github.com/petabridge/akkadotnet-code-samples/tree/master/infrastructure/mssql) and run it on port 1533. The sample is already pre-configured to connect to it at startup.

> **N.B.** give the MSSQL image about 30 seconds to start up and check the logs to see if an `Akka` database was successfully created or not. You may have to restart the container if you see that it failed the first time as the initial database file system initialization overhead can take quite up to 45-60s sometimes. Restarting the container doesn't include any of this overhead as the database file system is now already instantiated inside the container's ephemeral storage.

### Run the Sample

Load up Rider or Visual Studio and

1. Launch `AkkaESPoC.Host`, followed by
2. Launch `AkkaESPoC.WebApp`. / Launch 'AkkaESPoC.Blazor`

The Domain models a Fantasy Quest scenario, in which the minimal API in the `WebApp` provide methods to create a new quest, join a quest, etc.  

The latest commit attempts to use DistributedPubSub to inform the `QuestViewActor` in the Blazor app when a new Quest has been added in the `WebApp`, and then have that Actor send a SignalR message to automagically add the newly-created quest to the list of quests bound to the UI view `\quests'. 

As of now, the Distributed PubSub part is working, but the SignalR message is not being sent to the UI.

