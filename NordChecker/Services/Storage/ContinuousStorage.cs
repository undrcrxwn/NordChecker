using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Timers;
using Serilog;

namespace NordChecker.Services.Storage
{
    public class ContinuousStorage : Storage
    {
        private readonly Dictionary<string, Timer> _SynchronizationTimers = new();

        public ContinuousStorage(string directory) : base(directory) { }
        
        public void StartContinuousSync(Func<object> getEntity, string identifier, TimeSpan interval)
        {
            object entity = getEntity();

            Save(entity, identifier);

            var timer = new Timer { Interval = interval.TotalMilliseconds };
            timer.Elapsed += (sender, e) => Save(entity, identifier);
            timer.Start();

            if (_SynchronizationTimers.ContainsKey(identifier))
                _SynchronizationTimers[identifier].Stop();
            _SynchronizationTimers[identifier] = timer;

            Log.Information("Continuous synchronization has started for entity [identifier = {0}] with {1} interval", identifier, interval);
        }

        public void StartContinuousSync(object entity, string identifier, TimeSpan interval) =>
            StartContinuousSync(() => entity, identifier, interval);

        public void StartContinuousSync<T>(Func<T> getEntity, TimeSpan interval) =>
            StartContinuousSync(getEntity(), typeof(T).Name, interval);
        
        public void StartContinuousSync<T>(T entity, TimeSpan interval) =>
            StartContinuousSync(() => entity, typeof(T).Name, interval);

        public void StopContinuousSync(string identifier)
        {
            _SynchronizationTimers[identifier].Stop();
            _SynchronizationTimers.Remove(identifier);

            Log.Information("Continuous synchronization has been stopped for entity [identifier = {0}]", identifier);
        }

        public void StopContinuousSync<T>() => StopContinuousSync(typeof(T).Name);

        public bool IsSynchronized(string identifier) => _SynchronizationTimers.ContainsKey(identifier);
        public bool IsSynchronized<T>() => IsSynchronized(typeof(T).Name);
    }
}
