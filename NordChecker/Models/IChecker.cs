using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public interface IChecker
    {
        public TimeSpan Timeout { get; set; }
        public void ProcessAccount(Account account);
    }
}
