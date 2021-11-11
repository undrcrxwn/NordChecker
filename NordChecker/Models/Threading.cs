using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NordChecker.Models
{
    public class ThreadDistributor<TPayload>
    {
        public event EventHandler<TPayload> TaskCompleted;

        private readonly MasterToken _Token;
        private readonly ObservableCollection<TPayload> _Payloads;
        private readonly Func<TPayload, bool> _Predicate;
        private readonly Action<TPayload> _Handler;

        private readonly object _AbortionLocker = new();
        private readonly object _PayloadsLocker = new();
        private int _SuperfluousDistributionsCount;

        private int _ThreadCount;
        public int ThreadCount
        {
            get => _ThreadCount;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();

                int delta = value - _ThreadCount;
                _ThreadCount = value;

                for (var i = 0; i < delta; i++)
                    Distribute();

                if (delta < 0)
                    _SuperfluousDistributionsCount -= delta;
            }
        }

        public ThreadDistributor(
            int threadCount,
            ObservableCollection<TPayload> payloads,
            Func<TPayload, bool> predicate,
            Action<TPayload> handler,
            MasterToken token = null)
        {
            _Token = token ?? new MasterToken();
            _Token.Pause();

            ThreadCount = threadCount;
            _Payloads = payloads;
            _Predicate = predicate;
            _Handler = handler;

            TaskCompleted += (sender, e) => Distribute();
        }

        public void Start() => _Token.Continue();

        public void Stop() => _Token.Pause();

        public void Distribute()
        {
            Log.Verbose("New distribution");
            Task.Factory.StartNew(() =>
            {
                _Token.ThrowOrWaitIfRequested();
                lock (_AbortionLocker)
                {
                    if (_SuperfluousDistributionsCount > 0)
                    {
                        _SuperfluousDistributionsCount--;
                        return;
                    }
                }

                _Token.ThrowOrWaitIfRequested();
                TPayload payload;
                try
                {
                    lock (_PayloadsLocker)
                        payload = _Payloads.First(_Predicate);
                }
                catch
                {
                    Log.Verbose("New subscription");
                    _Payloads.CollectionChanged += OnCollectionChanged;
                    return;
                }

                _Token.ThrowOrWaitIfRequested();
                _Handler(payload);
                TaskCompleted?.Invoke(this, payload);

                var dispatcher = Dispatcher.CurrentDispatcher;
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                Dispatcher.Run();
            },
              CancellationToken.None,
              TaskCreationOptions.LongRunning,
              TaskScheduler.Default);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _Payloads.CollectionChanged -= OnCollectionChanged;
                Log.Verbose("Subscription acquired");
                Distribute();
            }
        }
    }
}
