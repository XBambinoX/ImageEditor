using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageEditor.Models
{
    public class LimitedStack<T> : IEnumerable<T>
    {
        private readonly LinkedList<T> _list = new LinkedList<T>();
        private readonly int _capacity;
        private bool _trimmed;

        public LimitedStack(int capacity) => _capacity = capacity;
        public int Count => _list.Count;

        public void Push(T item)
        {
            _list.AddFirst(item);
            if (_list.Count > _capacity)
            {
                _list.RemoveLast();
                _trimmed = true;
                GC.Collect(0, GCCollectionMode.Optimized, blocking: false);
            }
        }

        public T Pop()
        {
            var val = _list.First.Value;
            _list.RemoveFirst();
            return val;
        }

        public void Clear()
        {
            _list.Clear();
            _trimmed = true;
        }

        public bool ShouldCollect()
        {
            if (_trimmed) { _trimmed = false; return true; }
            return false;
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}