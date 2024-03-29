﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Serilog;

namespace NordChecker.Services.Threading
{
    public partial class ThreadDistributor<TPayload> where TPayload : class
    {
        public event EventHandler<TPayload> TaskCompleted;
        public event EventHandler<TPayload> TaskAborted;

        private readonly MasterToken _Token;
        private readonly ObservableCollection<TPayload> _Payloads;
        private readonly Func<TPayload, bool> _Filter;
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

                for (int i = 0; i < delta; i++)
                    Distribute();

                if (delta < 0)
                    _SuperfluousDistributionsCount -= delta;
            }
        }

        private ThreadDistributor(Builder builder)
        {
            if (builder.Token is null)
            {
                _Token = new MasterToken();
                _Token.Pause();
            }
            else
                _Token = builder.Token;

            _Payloads = builder.Payloads;
            _Filter = builder.Filter;
            _Handler = builder.Handler;

            TaskCompleted += (sender, e) => Distribute();
            TaskAborted += (sender, e) => TaskCompleted?.Invoke(this, null);

            ThreadCount = builder.ThreadCount;
        }
        
        public void Start()
        {
            Log.Warning("DISTRIBUTOR STARTED");
            _Token.Continue();
        }

        public void Stop() => _Token.Pause();

        public async Task Distribute()
        {
            Log.Verbose("New distribution");
            await Task.Factory.StartNew(() =>
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

                try
                {
                    _Token.ThrowOrWaitIfRequested();
                }
                catch
                {
                    TaskAborted?.Invoke(this, null);
                    return;
                }

                TPayload payload;
                try
                {
                    lock (_PayloadsLocker)
                        payload = _Payloads.First(_Filter);
                }
                catch
                {
                    Log.Verbose("New subscription");
                    _Payloads.CollectionChanged += OnCollectionChanged;
                    return;
                }

                try
                {
                    _Token.ThrowOrWaitIfRequested();
                    _Handler(payload);
                }
                catch
                {
                    TaskAborted?.Invoke(this, payload);
                    return;
                }

                TaskCompleted?.Invoke(this, payload);
                
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                Dispatcher.Run();
            },
              CancellationToken.None,
              TaskCreationOptions.LongRunning,
              TaskScheduler.Current);
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
