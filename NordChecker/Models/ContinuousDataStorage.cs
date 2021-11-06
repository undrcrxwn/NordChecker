using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace NordChecker.Models
{
    public class DataStorage
    {
        private readonly string _Directory;
        
        public DataStorage(string directory) => _Directory = directory;

        public void Save<T>(T target)
        {
            string path = GetAbsolutePath<T>();
            string json = JsonConvert.SerializeObject(target);
            Directory.CreateDirectory(_Directory);
            File.WriteAllText(path, json);
            Log.Information("{0} has been saved to {1}", typeof(T).Name, path);
        }

        public T Load<T>()
        {
            string path = GetAbsolutePath<T>();
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to access {0} to load {1}", path, typeof(T).Name);
                throw;
            }

            try
            {
                T obj = (T) JsonConvert.DeserializeObject(json, typeof(T));
                Log.Information("{0} has been loaded from {1}", typeof(T).Name, path);
                return obj;
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to deserialize {0} from {1}", typeof(T).Name, json);
                throw;
            }
        }

        public T LoadOrDefault<T>(T obj)
        {
            try
            {
                return Load<T>();
            }
            catch
            {
                return obj;
            }
        }

        private string GetAbsolutePath<T>() => $"{_Directory}\\{typeof(T).Name}.json";
    }

    public class ContinuousDataStorage : DataStorage
    {
        private readonly Dictionary<Type, Timer> _SynchronizationTimers = new();

        public ContinuousDataStorage(string directory) : base(directory) {}

        public void StartContinuousSync<T>(T target, TimeSpan interval)
        {
            Save(target);

            Timer timer = new Timer { Interval = interval.TotalMilliseconds };
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
    }
}
