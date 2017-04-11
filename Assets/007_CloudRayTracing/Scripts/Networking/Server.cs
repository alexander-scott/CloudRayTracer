
namespace BMW.Verification.CloudRayTracing
{
    public class Server : MasterServer<Peer>
    {
        public ServerConnection Connection { get; private set; }

        public Server()
        {
            // register a new authentication scope and set it as the default
            Connection = RegisterScope<ServerConnection>(0, true);
        }
    }
}