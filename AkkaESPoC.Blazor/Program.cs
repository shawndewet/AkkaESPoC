using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using AkkaESPoC.Blazor.Data;
using AkkaESPoC.Shared.Serialization;
using AkkaESPoC.Shared.Sharding;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var akkaSection = builder.Configuration.GetSection("Akka");

// maps to environment variable Akka__ClusterIp
var hostName = akkaSection.GetValue<string>("ClusterIp", "localhost");

// maps to environment variable Akka__ClusterPort
var port = akkaSection.GetValue<int>("ClusterPort", 7917);

var seeds = akkaSection.GetValue<string[]>("ClusterSeeds", new[] { "akka.tcp://AkkaESPoC@localhost:7919" }).Select(Address.Parse)
    .ToArray();

// Add services to the container.
builder.Services.AddAkka("AkkaESPoC", (configurationBuilder, provider) =>
{
    configurationBuilder.WithRemoting(hostName, port)
        .AddAppSerialization()
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

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

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
app.MapFallbackToPage("/_Host");

app.Run();
