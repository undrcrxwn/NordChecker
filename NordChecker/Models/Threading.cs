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
    public class MasterToken
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

    public class MasterTokenSource
    {
        public List<MasterToken> Tokens = new List<MasterToken>();

        public MasterToken MakeToken()
        {
            var token = new MasterToken();
            Tokens.Add(token);
            return token;
        }

        public void Cancel() => Tokens.ForEach(x => x.Cancel());
        public void Pause() => Tokens.ForEach(x => x.Pause());
        public void Continue() => Tokens.ForEach(x => x.Continue());
    }

    public class ThreadDistributor<TPayload>
    {
        private ObservableCollection<TPayload> payloads;
        private Func<TPayload, bool> predicate;
        private Action<TPayload, MasterToken> handler;
        private int superfluousDistributonsCount;
        private object abortionLocker = new object();
        private object payloadsLocker = new object();
        public MasterTokenSource TokenSource = new MasterTokenSource();
        public event EventHandler<TPayload> OnTaskCompleted;

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
                    nameof(ThreadCount), value, delta);

                // if delta is positive
                for (var i = 0; i < delta; i++)
                    Distribute();

                // if delta is negative
                if (delta < 0)
                    superfluousDistributonsCount -= delta;
            }
        }

        public ThreadDistributor(
            int threadCount,
            ObservableCollection<TPayload> payloads,
            Func<TPayload, bool> predicate,
            Action<TPayload, MasterToken> handler)
        {
            this.payloads = payloads;
            this.predicate = predicate;
            this.handler = handler;
            OnTaskCompleted += (sender, e) => Distribute();
            ThreadCount = threadCount;
        }

        public void Distribute()
        {
            Task.Factory.StartNew(() =>
            {
                lock (abortionLocker)
                {
                    if (superfluousDistributonsCount > 0)
                    {
                        Log.Debug("New distribution");
                        superfluousDistributonsCount--;
                        return;
                    }
                }

                Log.Debug("New distribution");

                TPayload payload;
                try
                {
                    lock (payloadsLocker)
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
                    handler(payload, TokenSource.MakeToken());
                }
                catch { }
                OnTaskCompleted?.Invoke(this, payload);
            },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                payloads.CollectionChanged -= OnCollectionChanged;
                Log.Debug("Subscribtion acquired");
                Distribute();
            }
        }
    }
}
