using System;
using System.Collections.Generic;
using System.Threading;

using Core.Math;

namespace Core.Chunking
{
//    public static class HashSetPositionChecker
//    {
//        private static ConcurrentHashSet<Int3> hashSet = new ConcurrentHashSet<Int3>();
//
//        public static int Count { get; private set; }
//
//        public static void Add(Int3 item)
//        {
//            if(!hashSet.Add(item))
//                throw new Exception($"Added an item {item} to the Hashset, that already exists.");
//
//            Count++;
//        }
//        public static bool Contains(Int3 item) => hashSet.Contains(item);
//
//        public static void Remove(Int3 item)
//        {
//            hashSet.Remove(item);
//            Count--;
//        }
//    }
    
    public static class HashSetPositionChecker
    {
        private static ConcurrentHashSet<Int2> hashSet = new ConcurrentHashSet<Int2>();

        public static int Count { get; private set; }

        public static void Add(Int2 item)
        {
            if(!hashSet.Add(item))
                throw new Exception($"Added an item {item} to the Hashset, that already exists.");

            Count++;
        }
        public static bool Contains(Int2 item) => hashSet.Contains(item);

        public static void Remove(Int2 item)
        {
            if (hashSet.Remove(item))
                Count--;
        }
    }

    public class ConcurrentHashSet<T> : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly HashSet<T> _hashSet = new HashSet<T>();

        #region Implementation of ICollection<T> ...ish
        public bool Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Add(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _hashSet.Clear();
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Contains(item);
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {
                return _hashSet.Remove(item);
            }
            finally
            {
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hashSet.Count;
                }
                finally
                {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lock?.Dispose();
            }
        }
        ~ConcurrentHashSet()
        {
            Dispose(false);
        }
        #endregion
    }
}