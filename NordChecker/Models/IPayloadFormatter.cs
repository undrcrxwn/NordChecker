using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public interface IPayloadFormatter<TPayload, TOutput>
    {
        public TOutput Format(TPayload payload);
    }
}
