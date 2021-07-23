using NordChecker.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    internal class AppSettings
    {
        public bool AreComboDuplicatesSkipped { get; set; } = true;
        public int ThreadCount { get; set; } = 50;
        public int TimeoutInSeconds { get; set; } = 7;
    }
}
