using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Transactions;
using System.Runtime.CompilerServices;

namespace Unknown6656.Generics;


public class DelayedQueue<T>
    : IDisposable
{
    private readonly object _mutex = new();
    private readonly Ref<bool> _running;
    private readonly int _min_timeout, _max_timeout;
    private readonly float _timeout_increment;
    private readonly Task _runner;
    private volatile int _timeout;

    public bool IsDisposed { get; private set; } = false;
    public ConcurrentQueue<T> Queue { get; } = new();


    public DelayedQueue(Action<T> on_dequeue, Ref<bool> running, int min_timeout, int max_timeout, float timeout_increment = 2)
    {
        _running = running;
        _min_timeout = min_timeout;
        _max_timeout = max_timeout;
        _timeout_increment = timeout_increment;
        _runner = Task.Factory.StartNew(async delegate
        {
            await SleepOrReset(false);

            while (IsDisposed && running)
            {
                bool sleep = true;

                while (Queue.TryDequeue(out T? value))
                {
                    sleep = false;
                    on_dequeue(value);
                }

                await SleepOrReset(sleep);
            }
        });
    }

    ~DelayedQueue() => Dispose(false);

    private async Task SleepOrReset(bool sleep)
    {
        if (sleep)
            await Task.WhenAll(
                Task.Delay(_timeout),
                Task.Factory.StartNew(delegate
                {
                    lock (_mutex)
                        _timeout = Math.Min(_max_timeout, (int)Math.Round(_timeout * _timeout_increment));
                })
            );
        else
            lock (_mutex)
                _timeout = _min_timeout;
    }

    protected virtual void Dispose(bool managed) => IsDisposed = true;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Enqueue(T value) => Queue.Enqueue(value);
}

public class DelayedCachedDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>
    , IReadOnlyDictionary<TKey, TValue>
    , IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    , ICollection<KeyValuePair<TKey, TValue>>
    , IEnumerable<KeyValuePair<TKey, TValue>>
    , IEnumerable
    , ICollection
    , IDictionary
    , IDisposable
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
    private readonly DelayedQueue<KeyValuePair<TKey, TValue>> _add_queue;
    private readonly DelayedQueue<TKey> _remove_queue;
    private volatile bool _disposed;
    private volatile bool _running;


    public static IEqualityComparer<TKey> DefaultKeyEqualityComparer { get; } = EqualityComparer<TKey>.Default;


    public bool IsDisposed => _disposed;

    public IEqualityComparer<TKey> KeyEqualityComparer { get; }

    public TValue? this[TKey key]
    {
        get => _dictionary[key];
        set => Add(key, value);
    }

    public object? this[object key]
    {
        set => this[(TKey)key] = (TValue?)value;
        get => this[(TKey)key];
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public bool IsSynchronized => true;

    object ICollection.SyncRoot => throw new InvalidOperationException();

    public ICollection<TKey> Keys => _dictionary.Keys;

    public ICollection<TValue> Values => _dictionary.Values;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    ICollection IDictionary.Keys => (ICollection)Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    ICollection IDictionary.Values => (ICollection)Values;


    public DelayedCachedDictionary()
        : this(DefaultKeyEqualityComparer)
    {
    }

    public DelayedCachedDictionary(IDictionary<TKey, TValue?> dictionary)
        : this(dictionary as IEnumerable<KeyValuePair<TKey, TValue>>)
    {
    }

    public DelayedCachedDictionary(IDictionary<TKey, TValue?> dictionary, IEqualityComparer<TKey>? comparer)
        : this(dictionary as IEnumerable<KeyValuePair<TKey, TValue>>, comparer)
    {
    }

    public DelayedCachedDictionary(IEnumerable<KeyValuePair<TKey, TValue?>> collection)
        : this(collection, DefaultKeyEqualityComparer)
    {
    }

    public DelayedCachedDictionary(IEnumerable<KeyValuePair<TKey, TValue?>> collection, IEqualityComparer<TKey>? comparer)
        : this(collection.ToList(), comparer)
    {
    }

    public DelayedCachedDictionary(IEqualityComparer<TKey>? comparer)
        : this(Enumerable.Empty<KeyValuePair<TKey, TValue>>(), comparer)
    {
    }

    public DelayedCachedDictionary(int capacity)
        :this(capacity, DefaultKeyEqualityComparer)
    {
    }

    public DelayedCachedDictionary(int capacity, IEqualityComparer<TKey>? comparer)
        : this(new List<KeyValuePair<TKey, TValue>>(capacity), comparer)
    {
    }

    private DelayedCachedDictionary(List<KeyValuePair<TKey, TValue?>> pairs, IEqualityComparer<TKey>? comparer)
    {
        _dictionary = new(Environment.ProcessorCount, pairs.Capacity, comparer);
        _running = true;
        _disposed = false;
        _add_queue = new(kvp => _dictionary[kvp.Key] = kvp.Value, new(ref _running), 1, 500, 2);
        _remove_queue = new(kvp => _dictionary.TryRemove(kvp, out _), new(ref _running), 10, 1000, 2);
    }

    ~DelayedCachedDictionary() => Dispose(false);

    public void Add(TKey key, TValue? value) => Add(new(key, value));

    public void Add(object key, object? value) => Add((TKey)key, (TValue?)value);

    public void Add(KeyValuePair<TKey, TValue?> item) => _add_queue.Enqueue(item);

    public void Clear() => Keys.ToList().Do(Remove);

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public void CopyTo(Array array, int index)
    {
        List<KeyValuePair<TKey, TValue>> copy = _dictionary.ToList();
        IList array_list = array;

        Parallel.For(0, copy.Count, i => array_list[i + index] = copy[i]);
    }

    public bool Remove(TKey key)
    {
        bool result;

        if (result = ContainsKey(key))
            _remove_queue.Enqueue(key);

        return result;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue?>> GetEnumerator() => _dictionary.GetEnumerator();

    public bool Contains(object key) => ContainsKey((TKey)key);

    public bool Contains(KeyValuePair<TKey, TValue?> item) => ContainsKey(item.Key);

    public void CopyTo(KeyValuePair<TKey, TValue?>[] array, int arrayIndex) => CopyTo(array as Array, arrayIndex);

    public bool Remove(KeyValuePair<TKey, TValue?> item) => Remove(item.Key);

    public void Remove(object key) => Remove((TKey)key);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IDictionaryEnumerator IDictionary.GetEnumerator() => new DictionaryEnumerator<TKey, TValue?>(GetEnumerator());

    protected virtual void Dispose(bool managed)
    {
        if (!_disposed)
        {
            _running = false;

            if (managed)
            {
                _add_queue.Dispose();
                _remove_queue.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public class DictionaryEnumerator<TKey, TValue>
    : IDictionaryEnumerator
    where TKey : notnull
{
    public IEnumerator<KeyValuePair<TKey, TValue>> Enumerator { get; }

    public DictionaryEntry Entry => new(Enumerator.Current.Key, Enumerator.Current.Value);

    public object Key => Enumerator.Current.Key;

    public object Value => Enumerator.Current.Value;

    public object Current => Entry;


    public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator) => Enumerator = enumerator;

    public bool MoveNext() => Enumerator.MoveNext();

    public void Reset() => Enumerator.Reset();
}
