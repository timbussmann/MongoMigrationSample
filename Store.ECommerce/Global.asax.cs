using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NServiceBus;
using NServiceBus.Encryption.MessageProperty;
using NServiceBus.Persistence.MongoDB;
using Store.Messages.Commands;

public class MvcApplication :
    HttpApplication
{
    public static IEndpointInstance EndpointInstance;

    protected void Application_End()
    {
        EndpointInstance?.Stop().GetAwaiter().GetResult();
    }

    protected void Application_Start()
    {
        AsyncStart().GetAwaiter().GetResult();
    }

    static async Task AsyncStart()
    {
        var endpointConfiguration = new EndpointConfiguration("Store.ECommerce");
        endpointConfiguration.PurgeOnStartup(true);

        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString("host=localhost");
        var routing = transport.Routing();
        routing.RouteToEndpoint(typeof(SubmitOrder).Assembly, "Store.Messages.Commands", "Store.Sales");

        endpointConfiguration.UsePersistence<MongoDbPersistence>()
            .SetConnectionString("mongodb://localhost/samples-store-sales");

        var defaultKey = "2015-10";
        var ascii = Encoding.ASCII;
        var encryptionService = new RijndaelEncryptionService(
            encryptionKeyIdentifier: defaultKey,
            key: ascii.GetBytes("gdDbqRpqdRbTs3mhdZh9qCaDaxJXl+e6"));
        endpointConfiguration.EnableMessagePropertyEncryption(encryptionService);

        EndpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        AreaRegistration.RegisterAllAreas();
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}
