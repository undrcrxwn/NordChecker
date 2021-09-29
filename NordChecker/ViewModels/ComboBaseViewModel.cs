using NordChecker.Models;
using NordChecker.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NordChecker.ViewModels
{
    public class ComboBaseViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        
    }
}
