using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using AkkaESPoC.Shared.Sharding;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var akkaSection = builder.Configuration.GetSection("Akka");

// maps to environment variable Akka__ClusterIp
var hostName = akkaSection.GetValue<string>("ClusterIp", "localhost");

// maps to environment variable Akka__ClusterPort
var port = akkaSection.GetValue<int>("ClusterPort", 7918);

var seeds = akkaSection.GetValue<string[]>("ClusterSeeds", new[] { "akka.tcp://AkkaESPoC@localhost:7919" }).Select(Address.Parse)
    .ToArray();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAkka("AkkaESPoC", (configurationBuilder, provider) =>
{
    configurationBuilder.WithRemoting(hostName, port)
        .WithCustomSerializer("hyperion", new[] { typeof(object) }, system => new Akka.Serialization.HyperionSerializer(system))
        .WithClustering(new ClusterOptions()
        { Roles = new[] { "Web" }, SeedNodes = seeds })
        .WithShardRegionProxy<QuestMarker>("quests", QuestActorProps.SingletonActorRole,
            new QuestMessageRouter())
        .WithActors((system, registry) =>
        {
            var proxyProps = system.QuestIndexProxyProps();
            registry.TryRegister<QuestIndexMarker>(system.ActorOf(proxyProps, "quest-proxy"));
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for question scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

MapMinimalAPI(app);

app.Run();

static void MapMinimalAPI(WebApplication app)
{
    app.MapGet("/LOTR/", async (ActorRegistry registry) =>
    {
        var _indexActor = registry.Get<QuestIndexMarker>();

        var result = await _indexActor.Ask<AkkaESPoC.Shared.Queries.FetchAllQuestsResponse>(AkkaESPoC.Shared.Queries.FetchAllQuests.Instance, TimeSpan.FromSeconds(5));

        return Results.Ok(result.Quests);
    });

    app.MapGet("/LOTR/{questId}", async (string questId, ActorRegistry registry) =>
    {
        var _questActor = registry.Get<QuestMarker>();

        var result = await _questActor.Ask<AkkaESPoC.Shared.Queries.FetchResult>(new AkkaESPoC.Shared.Queries.FetchQuest(questId), TimeSpan.FromSeconds(3));

        if (result.State.IsEmpty) // no quest with this id
            return Results.NotFound();

        return Results.Ok(result.State.Data);
    });

    app.MapPost("/LOTR/StartQuest", async (AkkaESPoC.WebApp.Models.StartQuestModel model, ActorRegistry registry) =>
    {
        var _questActor = registry.Get<QuestMarker>();

        var command = new AkkaESPoC.Shared.Commands.CreateQuest(Guid.NewGuid().ToString(),
                model.QuestName, model.Location, model.InitialMembers.Members);

        var createRsp = await _questActor.Ask<AkkaESPoC.Shared.Commands.QuestCommandResponse>(command, TimeSpan.FromSeconds(3));

        return Results.Created($"/LOTR/{createRsp.QuestId}", createRsp);
    });

    app.MapPost("/LOTR/JoinQuest/{questId}", async (string questId, AkkaESPoC.WebApp.Models.JoinQuestModel model, ActorRegistry registry) =>
    {
        var _questActor = registry.Get<QuestMarker>();

        var command = new AkkaESPoC.Shared.Commands.JoinQuest(questId, model.Day, model.Members);

        var createRsp = await _questActor.Ask<AkkaESPoC.Shared.Commands.QuestCommandResponse>(command, TimeSpan.FromSeconds(3));

        return Results.Accepted(value : createRsp);
    });

    app.MapPost("/LOTR/Arrive/{questId}", async (string questId, AkkaESPoC.WebApp.Models.ArriveAtLocationModel model, ActorRegistry registry) =>
    {
        var _questActor = registry.Get<QuestMarker>();

        var command = new AkkaESPoC.Shared.Commands.ArriveAtLocation(questId, model.Day, model.Location);

        var createRsp = await _questActor.Ask<AkkaESPoC.Shared.Commands.QuestCommandResponse>(command, TimeSpan.FromSeconds(3));

        return Results.Accepted(value: createRsp);
    });


}