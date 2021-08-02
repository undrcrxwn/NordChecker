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
        private int threadCount;
        private ObservableCollection<TPayload> targets;
        private object locker = new object();
        private Func<TPayload, bool> predicate;
        private Action<TPayload, ThreadMasterToken> handler;
        private ThreadMasterToken token;
        public event Action OnTaskCompleted;

        public ThreadDistributor(
            int threadCount,
            ObservableCollection<TPayload> targets,
            Func<TPayload, bool> predicate,
            Action<TPayload, ThreadMasterToken> handler,
            ThreadMasterToken token)
        {
            this.threadCount = threadCount;
            this.targets = targets;
            this.predicate = predicate;
            this.handler = handler;
            this.token = token;

            OnTaskCompleted += Distribute;
            for (int i = 0; i < threadCount; i++)
                Distribute();
        }

        public void Distribute()
        {
            Task.Factory.StartNew(() =>
            {
                Interlocked.Decrement(ref threadCount);
                token.ThrowOrWaitIfRequested();

                TPayload payload;
                lock (locker)
                {
                    try
                    {
                        payload = targets.First(predicate);
                    }
                    catch
                    {
                        Console.WriteLine("not found");
                        targets.CollectionChanged +=
                            (object sender, NotifyCollectionChangedEventArgs e) => Distribute();
                        Console.WriteLine("new distribute");
                        return;
                    }
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
    }
}
