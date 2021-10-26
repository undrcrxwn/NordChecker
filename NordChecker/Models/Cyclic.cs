using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NordChecker.Models
{
    public class Cyclic<T> : ICollection<T>
    {
        private ICollection<T> _Items;
        public ICollection<T> Items
        {
            get => _Items;
            set
            {
                _Items = value;
                cyclicEnumerator = GetEnumerator();
            }
        }

        private IEnumerator<T> cyclicEnumerator;
        private object locker = new object();

        public Cyclic(ICollection<T> items = null)
        {
            Items = items ?? new List<T>();
            cyclicEnumerator = GetEnumerator();
        }

        public T GetNext()
        {
            lock (locker)
            {
                cyclicEnumerator.MoveNext();
                return cyclicEnumerator.Current;
            }
        }

        public void Reset() => cyclicEnumerator = GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> nativeEnumerator = Items.GetEnumerator();
            while (true)
            {
                if (!nativeEnumerator.MoveNext())
                {
                    nativeEnumerator.Reset();
                    if (!nativeEnumerator.MoveNext())
                        yield break;
                }
                yield return nativeEnumerator.Current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Items.Count;
        public bool IsReadOnly => Items.IsReadOnly;
        public void Add(T item) => Items.Add(item);
        public void Clear() => Items.Clear();
        public bool Contains(T item) => Items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        public bool Remove(T item) => Items.Remove(item);
    }
}
