using System;
using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace NordChecker.Services.Storage
{
    public class Storage
    {
        public string RootDirectory;

        public Storage(string directory) => RootDirectory = directory;

        public void Save(object target, string identifier)
        {
            string path = GetAbsolutePath(identifier);
            string json = JsonConvert.SerializeObject(target);

            Directory.CreateDirectory(RootDirectory);
            File.WriteAllText(path, json);
            Log.Information("Entity [identifier = {0}] has been saved to {1}", identifier, path);
        }

        public void Save<T>(T target) => Save(target, typeof(T).Name);

        public T Load<T>(string identifier)
        {
            string path = GetAbsolutePath(identifier);
            string json;

            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to access {0} to load entity [identifier = {1}]", path, identifier);
                throw;
            }

            try
            {
                T entity = JsonConvert.DeserializeObject<T>(json);
                Log.Information("Entity [identifier = {0}] has been loaded from {1}", identifier, path);
                return entity;
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to deserialize entity [identifier = {0}] from {1}", identifier, json);
                throw;
            }
        }

        public T Load<T>() => Load<T>(typeof(T).Name);

        public T LoadOrDefault<T>(string identifier, T obj)
        {
            try
            {
                return Load<T>(identifier) ?? obj;
            }
            catch
            {
                return obj;
            }
        }

        public T LoadOrDefault<T>(T obj) => LoadOrDefault(typeof(T).Name, obj);

        private static string GetFileName(string identifier) => $"{identifier}.json";
        private static string GetFileName<T>() => GetFileName(typeof(T).Name);

        private string GetAbsolutePath(string identifier) => $"{RootDirectory}\\{GetFileName(identifier)}";
        private string GetAbsolutePath<T>() => GetAbsolutePath(GetFileName<T>());
    }
}
