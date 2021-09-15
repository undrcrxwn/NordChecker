using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public class DataFormatter
    {
        public string FormatScheme;
    }

    public class DataFormatterBuilder
    {
        private DataFormatter formatter;

        public DataFormatterBuilder()
            => formatter = new DataFormatter();

        public DataFormatterBuilder SetScheme(string scheme)
        {
            formatter.FormatScheme = scheme;
            return this;
        }

        public DataFormatter Build() => formatter;
    }
}
