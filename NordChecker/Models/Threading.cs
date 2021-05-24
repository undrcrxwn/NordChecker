using System;
using System.Collections.Generic;
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
            return Task.Run(() => action(ref isCanceled, payload));
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

    internal class QueueThread
    {
        private Queue<CancelableAction> actions = new Queue<CancelableAction>();
        private object locker = new object();

        public int Timeout;
        public int Count { get => actions.Count; }

        public QueueThread(int timeout)
        {
            Timeout = timeout;
        }

        private void Run(CancelableAction action)
        {
            new Thread(() =>
            {
                Task actionTask = action.Run();
                if (!actionTask.Wait(Timeout))
                    action.Cancel();

                actions.Dequeue();
                if (actions.Count > 0)
                    Run(actions.Peek());
            }).Start();
        }

        public void Push(CancelableAction action)
        {
            lock (locker)
            {
                actions.Enqueue(action);
                if (actions.Count == 1)
                    Task.Run(() => Run(action));
            }
        }
    }

    internal class ThreadDistributor
    {
        public readonly int MaxThreadAmount;
        public readonly int Timeout;
        private List<QueueThread> threads = new List<QueueThread>();
        private object locker = new object();

        public ThreadDistributor(int maxTheadAmount, int timeout)
        {
            MaxThreadAmount = maxTheadAmount;
            Timeout = timeout;
        }

        public void Push(CancelableAction action)
        {
            if (threads.Count < MaxThreadAmount)
            {
                QueueThread thread = new QueueThread(Timeout);
                thread.Push(action);
                threads.Add(thread);
            }
            else
            {
                QueueThread thread = threads.OrderBy(t => t.Count).First();
                thread.Push(action);
            }
        }
    }
}
