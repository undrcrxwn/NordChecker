using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;

namespace NordChecker.Models
{
    public class Storage
    {
        private readonly string _Directory;

        public Storage(string directory) => _Directory = directory;

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
                T obj = (T)JsonConvert.DeserializeObject(json, typeof(T));
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
}
