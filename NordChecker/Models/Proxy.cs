using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    internal class Proxy
    {
        public string Content;

        public Proxy(string content)
        {
            Content = content;
        }
    }

    internal class ProxyEnumerator : IEnumerator
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

    internal class ProxyDispenser
    {
        private List<Proxy> Proxies;
        private ProxyEnumerator enumerator;
        private object locker = new object();

        public ProxyDispenser(List<Proxy> proxies)
        {
            Proxies = proxies;
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
