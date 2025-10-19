using System.Collections.Generic;
using UnityEngine;


namespace Rhythm.Pooling
{
    public class SimplePool<T> where T : Component
    {
        private readonly Queue<T> _q = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;


        public SimplePool(T prefab, int initial, Transform parent)
        {
            _prefab = prefab; _parent = parent;
            for (int i = 0; i < initial; i++)
            {
                var o = GameObject.Instantiate(_prefab, _parent);
                o.gameObject.SetActive(false);
                _q.Enqueue(o);
            }
        }


        public T Get()
        {
            var t = _q.Count > 0 ? _q.Dequeue() : GameObject.Instantiate(_prefab, _parent);
            t.gameObject.SetActive(true);
            return t;
        }


        public void Release(T t)
        {
            t.gameObject.SetActive(false);
            _q.Enqueue(t);
        }
    }
}