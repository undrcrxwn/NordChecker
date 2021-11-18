using System.Collections;
using System.Collections.Generic;

namespace NordChecker.Shared.Collections
{
    public class Cyclic<T> : ICollection<T>
    {
        private IEnumerator<T> _CyclicEnumerator;
        private readonly object _Locker = new();

        private ICollection<T> _Items;
        public ICollection<T> Items
        {
            get => _Items;
            set
            {
                _Items = value;
                _CyclicEnumerator = GetEnumerator();
            }
        }
        
        public Cyclic(ICollection<T> items = null)
        {
            Items = items ?? new List<T>();
            _CyclicEnumerator = GetEnumerator();
        }

        public T GetNext()
        {
            lock (_Locker)
            {
                _CyclicEnumerator.MoveNext();
                return _CyclicEnumerator.Current;
            }
        }

        public void Reset() => _CyclicEnumerator = GetEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            var nativeEnumerator = Items.GetEnumerator();
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
