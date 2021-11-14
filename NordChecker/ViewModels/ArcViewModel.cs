using NordChecker.Shared;
using System.ComponentModel;
using System.Windows;
using NordChecker.Infrastructure;

namespace NordChecker.ViewModels
{
    public class ArcViewModel : INotifyPropertyChangedAdvanced
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private float _StartAngle;
        public float StartAngle
        {
            get => _StartAngle;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _StartAngle, value, PropertyChanged);
        }

        private float _EndAngle;
        public float EndAngle
        {
            get => _EndAngle;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _EndAngle, value, PropertyChanged);
        }


        private Visibility _Visibility;

        public Visibility Visibility
        {
            get => _Visibility;
            set => (this as INotifyPropertyChangedAdvanced)
                .Set(ref _Visibility, value, PropertyChanged);
        }

        public ArcViewModel(float startAngle, float endAngle, Visibility visibility)
        {
            StartAngle = startAngle;
            EndAngle = endAngle;
            Visibility = visibility;
        }
    }
}
