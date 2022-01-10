﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NordChecker.Models.Settings;

namespace NordChecker.Infrastructure
{
    public class Wrapped<T> where T : class
    {
        public event EventHandler InstanceReplaced;

        private T _Instance;
        public T Instance
        {
            get => _Instance;
            set
            {
                _Instance = value;
                InstanceReplaced?.Invoke(this, EventArgs.Empty);
            }
        }

        public Wrapped(T instance) => _Instance = instance;

        public void ReplaceWith(T instance) => Instance = instance;

        public void ForEach(Action<T> handle)
        {
            InstanceReplaced += (sender, e) => handle(Instance);
            handle(Instance);
        }
    }
}
