using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Linq;
using System;

namespace Unknown6656.Generics;


public class ConcurrentHashSet<T>
    : ISet<T>
    , IReadOnlyCollection<T>
    , IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly HashSet<T> _hashSet = new();


    /// <inheritdoc cref="HashSet{T}.Count"/>
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
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
            }
        }
    }

    public bool IsDisposed { get; private set; }

    public bool IsReadOnly { get; } = false;


    ~ConcurrentHashSet() => Dispose(false);

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
                if (_lock != null)
                {
                    if (_lock.IsWriteLockHeld)
                        _lock.ExitWriteLock();

                    _lock.Dispose();
                }

            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="HashSet{T}.Add(T)"/>
    public bool Add(T item)
    {
        _lock.EnterWriteLock();

        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
                _lock.ExitWriteLock();
        }
    }

    void ICollection<T>.Add(T item) => Add(item);

    /// <inheritdoc cref="HashSet{T}.Clear"/>
    public void Clear()
    {
        _lock.EnterWriteLock();

        try
        {
            _hashSet.Clear();
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
                _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc cref="HashSet{T}.Contains(T)"/>
    public bool Contains(T item)
    {
        _lock.EnterReadLock();

        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            if (_lock.IsReadLockHeld)
                _lock.ExitReadLock();
        }
    }

    /// <inheritdoc cref="HashSet{T}.Remove(T)"/>
    public bool Remove(T item)
    {
        _lock.EnterWriteLock();

        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld)
                _lock.ExitWriteLock();
        }
    }

    public T[] ToArray()
    {
        _lock.EnterReadLock();

        try
        {
            return _hashSet.ToArray();
        }
        finally
        {
            if (_lock.IsReadLockHeld)
                _lock.ExitReadLock();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        _lock.EnterReadLock();

        try
        {
            return _hashSet.ToList().GetEnumerator();
        }
        finally
        {
            if (_lock.IsReadLockHeld)
                _lock.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();





    ///////////////////////////////////////////////////////////////////// TODO /////////////////////////////////////////////////////////////////////

    void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
    void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotImplementedException();
    bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();
    bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();
    bool ISet<T>.IsSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();
    bool ISet<T>.IsSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();
    bool ISet<T>.Overlaps(IEnumerable<T> other) => throw new NotImplementedException();
    bool ISet<T>.SetEquals(IEnumerable<T> other) => throw new NotImplementedException();
    void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotImplementedException();
    void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotImplementedException();
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
}
