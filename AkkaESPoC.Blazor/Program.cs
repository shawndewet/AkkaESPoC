using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using AkkaESPoC.Blazor.Actors;
using AkkaESPoC.Blazor.Data;
using AkkaESPoC.Blazor.Hubs;
using AkkaESPoC.Shared.Sharding;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var akkaSection = builder.Configuration.GetSection("Akka");

// maps to environment variable Akka__ClusterIp
var hostName = akkaSection.GetValue<string>("ClusterIp", "localhost");

// maps to environment variable Akka__ClusterPort
var port = akkaSection.GetValue<int>("ClusterPort", 7917);

var seeds = akkaSection.GetValue<string[]>("ClusterSeeds", new[] { "akka.tcp://AkkaESPoC@localhost:7919" }).Select(Address.Parse)
    .ToArray();

var connectionString = builder.Configuration.GetConnectionString("AkkaSqlConnection");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddAkka("AkkaESPoC", (configurationBuilder, provider) =>
{
    configurationBuilder
        .WithRemoting(hostName, port)
        .WithCustomSerializer("hyperion", new[] { typeof(object) }, system => new Akka.Serialization.HyperionSerializer(system))
        .WithClustering(new ClusterOptions()
            { Roles = new[] { "Blazor" }, SeedNodes = seeds })
        //.WithShardRegionProxy<QuestMarker>("quests", QuestActorProps.SingletonActorRole,
        //    new QuestMessageRouter())
        .WithActors((system, registry) =>
        {
            var proxyProps = system.QuestIndexProxyProps();
            registry.TryRegister<QuestIndexMarker>(system.ActorOf(proxyProps, "quest-proxy"));

            var resolverProps = Akka.DependencyInjection.DependencyResolver.For(system).Props<QuestViewActor>();
            //var viewActorProps = system.ActorOf(Props.Create<QuestViewActor>(provider), "questview");
            system.ActorOf(resolverProps, "questview");
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();

app.MapHub<QuestHub>("/questhub");
app.MapFallbackToPage("/_Host");

app.Run();
