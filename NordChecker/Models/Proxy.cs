using System;
using Leaf.xNet;

namespace NordChecker.Models
{
    public enum ProxyState
    {
        Invalid,
        Valid
    }

    public class Proxy : IEquatable<Proxy>
    {
        public ProxyClient Client;
        public ProxyState State = ProxyState.Valid;

        public Proxy(ProxyClient client) => Client = client;

        public static Proxy Parse(ProxyType proxyType, string protoProxyAddress)
            => new Proxy(ProxyClient.Parse(proxyType, protoProxyAddress));

        public bool Equals(Proxy other) => Client.Equals(other.Client);
        public override string ToString() => Client.ToString();
    }
}
