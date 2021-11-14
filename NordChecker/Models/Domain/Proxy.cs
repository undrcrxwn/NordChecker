using Leaf.xNet;

namespace NordChecker.Models.Domain
{
    public enum ProxyState
    {
        Unused,
        Invalid,
        Valid
    }

    public class Proxy
    {
        public ProxyClient Client;
        public ProxyState State = ProxyState.Unused;

        public Proxy(ProxyClient client) => Client = client;
        public override string ToString() => Client.ToString();
    }
}
