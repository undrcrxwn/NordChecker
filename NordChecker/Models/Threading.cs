using NordChecker.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    class ThreadMasterToken
    {
        public bool IsCancellationRequested { get; private set; }
        public bool IsPauseRequested { get; private set; }

        public void Cancel() => IsCancellationRequested = true;
        public void Pause() => IsPauseRequested = true;
        public void Continue() => IsPauseRequested = false;

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
                throw new OperationCanceledException();
        }

        public void WaitIfPauseRequested()
        {
            while (IsPauseRequested)
            {
                Thread.Sleep(50);
                ThrowIfCancellationRequested();
            }
        }

        public void ThrowOrWaitIfRequested()
        {
            ThrowIfCancellationRequested();
            WaitIfPauseRequested();
        }
    }

    class ThreadDistributor<TPayload>
    {
        private ObservableCollection<TPayload> payloads;
        private Func<TPayload, bool> predicate;
        private Action<TPayload, ThreadMasterToken> handler;
        private ThreadMasterToken token;
        private object locker = new object();
        public event Action OnTaskCompleted;

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

                Log.Information("Distributor's {property} has been set to {value} with {delta} delta",
                    Utils.GetMemberName(() => ThreadCount), value, delta);

                // if delta is positive
                for (var i = 0; i < delta; i++)
                    Distribute();

                // if delta is negative
                for (var i = 0; i > delta; i--)
                    payloads.CollectionChanged -= OnCollectionChanged;
            }
        }

        public ThreadDistributor(
            int threadCount,
            ObservableCollection<TPayload> payloads,
            Func<TPayload, bool> predicate,
            Action<TPayload, ThreadMasterToken> handler,
            ThreadMasterToken token)
        {
            this.payloads = payloads;
            this.predicate = predicate;
            this.handler = handler;
            this.token = token;
            OnTaskCompleted += Distribute;
            ThreadCount = threadCount;
        }

        public void Distribute()
        {
            Task.Factory.StartNew(() =>
            {
                Log.Debug("New distribution");
                token.ThrowOrWaitIfRequested();

                TPayload payload;
                try
                {
                    lock (locker)
                        payload = payloads.First(predicate);
                }
                catch
                {
                    payloads.CollectionChanged += OnCollectionChanged;
                    Log.Debug("New subscription");
                    return;
                }

                try
                {
                    handler(payload, token);
                }
                catch { }
                OnTaskCompleted?.Invoke();
            },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            payloads.CollectionChanged -= OnCollectionChanged;
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Log.Debug("Subscribtion acquired");
                Distribute();
            }
        }
    }
}
