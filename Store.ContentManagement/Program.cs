using System;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Encryption.MessageProperty;
using NServiceBus.Persistence.MongoDB;
using Store.Messages.RequestResponse;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.Store.ContentManagement";
        var endpointConfiguration = new EndpointConfiguration("Store.ContentManagement");

        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>()
            .ConnectionString("host=localhost");
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(ProvisionDownloadRequest), "Store.Operations");

        endpointConfiguration.UsePersistence<MongoDbPersistence>()
            .SetConnectionString("mongodb://localhost/samples-store-content-management");

        var defaultKey = "2015-10";
        var ascii = Encoding.ASCII;
        var encryptionService = new RijndaelEncryptionService(
            encryptionKeyIdentifier: defaultKey,
            key: ascii.GetBytes("gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6"));
        endpointConfiguration.EnableMessagePropertyEncryption(encryptionService);

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}