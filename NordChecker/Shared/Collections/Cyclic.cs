using System.Collections;
using System.Collections.Generic;

namespace NordChecker.Shared.Collections
{
    public class Cyclic<T> : ICollection<T>
    {
        private IEnumerator<T> _CyclicEnumerator;
        private readonly object _Locker = new();

        private ICollection<T> _FiniteCollection;
        public ICollection<T> FiniteCollection
        {
            get => _FiniteCollection;
            set
            {
                _FiniteCollection = value;
                _CyclicEnumerator = GetEnumerator();
            }
        }

        public Cyclic() : this(new List<T>()) { }
        
        public Cyclic(ICollection<T> items)
        {
            FiniteCollection = items;
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
            using IEnumerator<T> nativeEnumerator = FiniteCollection.GetEnumerator();
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

        public int Count => FiniteCollection.Count;
        public bool IsReadOnly => FiniteCollection.IsReadOnly;
        public void Add(T item) => FiniteCollection.Add(item);
        public void Clear() => FiniteCollection.Clear();
        public bool Contains(T item) => FiniteCollection.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => FiniteCollection.CopyTo(array, arrayIndex);
        public bool Remove(T item) => FiniteCollection.Remove(item);
    }
}
