namespace NordChecker.Infrastructure
{
    public interface IPageViewModel : INotifyPropertyChangedAdvanced
    {
        public string Title { get; }
    }
}
