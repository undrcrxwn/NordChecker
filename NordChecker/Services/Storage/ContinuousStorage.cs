using System;
using System.Collections.Generic;
using System.Timers;
using Serilog;

namespace NordChecker.Services.Storage
{
    public class ContinuousStorage : Storage
    {
        private readonly Dictionary<Type, Timer> _SynchronizationTimers = new();

        public ContinuousStorage(string directory) : base(directory) { }

        public void StartContinuousSync<T>(T target, TimeSpan interval)
        {
            Save(target);

            var timer = new Timer { Interval = interval.TotalMilliseconds };
            timer.Elapsed += (sender, e) => Save(target);
            timer.Start();

            if (_SynchronizationTimers.ContainsKey(typeof(T)))
                _SynchronizationTimers[typeof(T)].Stop();
            _SynchronizationTimers[typeof(T)] = timer;

            Log.Information("Continuous synchronization has started for {0} with {1} interval", typeof(T).Name, interval);
        }

        public void StopContinuousSync<T>()
        {
            _SynchronizationTimers[typeof(T)].Stop();
            _SynchronizationTimers.Remove(typeof(T));

            Log.Information("Continuous synchronization has been stopped for {0}", typeof(T).Name);
        }

        public bool IsSynchronized<T>() => _SynchronizationTimers.ContainsKey(typeof(T));
    }
}
