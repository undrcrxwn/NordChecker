using Leaf.xNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public enum ProxyState
    {
        Valid,
        Invalid,
        Unchecked
    }

    public class Proxy
    {
        public ProxyClient Client { get; private set; }
        public ProxyState State = ProxyState.Unchecked;

        public Proxy(ProxyClient client)
        {
            Client = client;
        }

        public override string ToString() => Client.ToString();
    }
}
