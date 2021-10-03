using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Shared
{
    public interface IPageViewModel : INotifyPropertyChangedAdvanced
    {
        public string Title { get; }
    }
}
