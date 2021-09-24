using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public interface IDataFormatter<TData, TOutput>
    {
        public TOutput Format(TData obj);
        public void AddPlaceholder(string key, Func<TData, object> handler);
    }
}
