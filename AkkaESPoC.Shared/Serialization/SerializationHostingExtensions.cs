using Akka.Hosting;

namespace AkkaESPoC.Shared.Serialization;

public static class SerializationHostingExtensions
{
    /// <summary>
    /// Configures the custom serialization for the AkkaESPoC App.
    /// </summary>
    public static AkkaConfigurationBuilder AddAppSerialization(this AkkaConfigurationBuilder builder)
    {
        return builder.WithCustomSerializer("akka-es-poc", new[] { typeof(IAkkaESPoCProtocolMember) },
            system => new MessageSerializer(system));
    }
}