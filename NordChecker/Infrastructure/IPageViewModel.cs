namespace NordChecker.Infrastructure
{
    public interface IPageViewModel : INotifyPropertyChangedAdvanced
    {
        public string Title { get; }

        protected void OnFocused() { }
        protected void OnUnfocused() { }
    }
}
