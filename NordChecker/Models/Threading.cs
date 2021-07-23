using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    internal delegate void ActionRef<T>(ref T obj, object payload);

    internal class CancelableAction
    {
        private ActionRef<bool> action;
        private bool isCanceled = false;
        public object payload;
        public event Action OnCanceled;

        public Task Run()
        {
            return Task.Factory.StartNew(
                () => action(ref isCanceled, payload),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Cancel()
        {
            isCanceled = true;
            OnCanceled();
        }

        public CancelableAction(ActionRef<bool> action, object payload)
        {
            this.payload = payload;
            this.action = action;
        }
    }
    
    internal class QueueThread : Queue<CancelableAction>
    {
        private object locker = new object();

        public int Timeout;

        public QueueThread(int timeout) => Timeout = timeout;

        private void Run(CancelableAction action)
        {
            new Thread(() =>
            {
                Task actionTask = action.Run();
                if (!actionTask.Wait(Timeout))
                    action.Cancel();

                Dequeue();
                if (Count > 0)
                    Run(Peek());
            }).Start();
        }

        public void Push(CancelableAction action)
        {
            lock (locker)
            {
                Enqueue(action);
                if (Count == 1)
                    Task.Run(() => Run(action));
            }
        }
    }

    internal class ThreadDistributor
    {
        public readonly int MaxThreadCount;
        public readonly int Timeout;
        public List<QueueThread> Threads = new List<QueueThread>();

        public ThreadDistributor(int maxTheadCount, int timeout)
        {
            MaxThreadCount = maxTheadCount;
            Timeout = timeout;
        }

        public int CountActiveThreads()
        {
            // Threads.Count(t => t.Count > 0);
            int res = 0;
            for (int x = Threads.Count - 1; x > -1; x--)
            {
                if (Threads[x].Count > 0)
                    res++;
            }
            return res;
        }

        public void Push(CancelableAction action)
        {
            if (Threads.Count < MaxThreadCount)
            {
                QueueThread thread = new QueueThread(Timeout);
                thread.Push(action);
                Threads.Add(thread);
            }
            else
            {
                QueueThread thread = Threads.OrderBy(t => t.Count).First();
                thread.Push(action);
            }
        }
    }
}
