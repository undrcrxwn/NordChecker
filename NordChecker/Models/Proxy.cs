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
    }

    public class ProxyEnumerator : IEnumerator
    {
        private int i = -1;
        private List<Proxy> proxies;

        public object Current
        {
            get
            {
                if (i >= proxies.Count)
                    i = 0;
                return proxies[i];
            }
        }

        public bool MoveNext()
        {
            i++;
            return true;
        }

        public void Reset()
        {
            i = -1;
        }

        public ProxyEnumerator(List<Proxy> proxies)
        {
            this.proxies = proxies;
        }
    }

    public class ProxyDispenser
    {
        public List<Proxy> Proxies;
        private ProxyEnumerator enumerator;
        private object locker = new object();

        public ProxyDispenser()
        {
            Proxies = new List<Proxy>();
            enumerator = new ProxyEnumerator(Proxies);
        }

        public Proxy GetProxy()
        {
            lock (locker)
            {
                enumerator.MoveNext();
                return (Proxy)enumerator.Current;
            }
        }
    }
}
