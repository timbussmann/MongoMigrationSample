using System;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Encryption.MessageProperty;
using NServiceBus.MongoDB;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.Store.CustomerRelations";
        var endpointConfiguration = new EndpointConfiguration("Store.CustomerRelations");
        endpointConfiguration.UseTransport<RabbitMQTransport>()
            .ConnectionString("host=localhost");

        endpointConfiguration.UsePersistence<MongoDBPersistence>()
            .SetConnectionString("mongodb://localhost")
            .SetDatabaseName("samples-store-customer-relations");

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
