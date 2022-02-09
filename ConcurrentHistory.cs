using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace Unknown6656.Generics;


/// <summary>
/// Represents a generic history datastructure. A history contains a modifyable list of chronological items, in which the user may navigate.
/// The navigation is performed one-dimensionally through the history's list backwards (i.e. towards older items) or forwards (towards newer items) in time.
/// <para/>
/// <para/>
/// This datastructure is concurrent.
/// </summary>
/// <typeparam name="T">The generic datatype <typeparamref name="T"/>.</typeparam>
public class ConcurrentHistory<T>
    : IEnumerable<T>
{
    private readonly List<T> _history;


    /// <summary>
    /// Returns the current history item.
    /// An <see cref="IndexOutOfRangeException"/> will be thrown if the history is either empty or has an invalid <see cref="HistoryIndex"/>.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"/>
    public T Current => this[HistoryIndex];

    /// <summary>
    /// Indicates whether a backward navigation is possible (i.e. the <see cref="HistoryIndex"/> is not pointing to the history's oldest element).
    /// </summary>
    public bool CanNavigateBack => HistoryIndex > 0;

    /// <summary>
    /// Indicates whether a forward navigation is possible (i.e. the <see cref="HistoryIndex"/> is not pointing to the history's newest element).
    /// </summary>
    public bool CanNavigateForward => HistoryIndex < HistorySize - 1;

    /// <summary>
    /// The history's current (zero-based) index.
    /// An Index of -1 indicates that the history is empty.
    /// </summary>
    public int HistoryIndex { get; private set; }

    /// <summary>
    /// The history's current size.
    /// </summary>
    public int HistorySize => _history.Count;

    /// <summary>
    /// The history's current capacity.
    /// This value is generally not equal to the histoy's current size.
    /// </summary>
    public int Capacity => _history.Capacity;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public T this[Index index] => GetItem(index, false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public ConcurrentHistory<T> this[Range range] => Slice(range, false);

    /// <summary>
    /// Returns the history slice which contains all past items (i.e. older than the current one).
    /// The returned slice excludes the current item.
    /// </summary>
    public ConcurrentHistory<T> PastItems => this[..HistoryIndex];

    /// <summary>
    /// Returns the history slice which contains all future items (i.e. newer than the current one).
    /// The returned slice excludes the current item.
    /// </summary>
    public ConcurrentHistory<T> FutureItems => this[(HistoryIndex + 1)..];


    /// <summary>
    /// Creates a new history of <typeparamref name="T"/>.
    /// </summary>
    public ConcurrentHistory() => _history = new();

    /// <summary>
    /// Creates a new history of T and adds the given items in chronological order (oldest to newest).
    /// </summary>
    /// <param name="items">Items to be added.</param>
    public ConcurrentHistory(params T[] items)
        : this(items as IEnumerable<T>)
    {
    }

    /// <summary>
    /// Creates a new history of T and adds the given items in chronological order (oldest to newest).
    /// </summary>
    /// <param name="items">Items to be added.</param>
    public ConcurrentHistory(IEnumerable<T> items)
        : this()
    {
        lock (this)
        {
            _history.AddRange(items);
            HistoryIndex = HistorySize - 1;
        }
    }

    /// <summary>
    /// Creates a new history of <typeparamref name="T"/> with the given initial capacity.
    /// </summary>
    /// <param name="initial_capacity">The history's initial capacity.</param>
    public ConcurrentHistory(int initial_capacity) => _history = new(initial_capacity);

    /// <summary>
    /// Clears the history.
    /// </summary>
    public void Clear()
    {
        lock (this)
        {
            _history.Clear();
            HistoryIndex = -1;
        }
    }

    /// <summary>
    /// Tries to navigate to the preivous (i.e. older) history item and returns whether the navigation process was successful.
    /// </summary>
    /// <returns>Indicates whether the backward navigation was successful.</returns>
    public bool NavigateBack() => NavigateBack(1) == 1;

    /// <summary>
    /// Tries to navigate to the next (i.e. newer) history item and returns whether the navigation process was successful.
    /// </summary>
    /// <returns>Indicates whether the forward navigation was successful.</returns>
    public bool NavigateForward() => NavigateForward(1) == 1;

    /// <summary>
    /// Tries to navigate to the previous (i.e. older) '<paramref name="count"/>'-th history item and returns how many navigation steps could be performed.
    /// </summary>
    /// <param name="count">Number of backward navigation steps to be taken.</param>
    /// <returns>The number of performed navigation steps.</returns>
    public int NavigateBack(int count)
    {
        if (count < 0)
            return NavigateForward(-count);
        else
            lock (this)
            {
                int actual = Math.Min(HistoryIndex, count);

                HistoryIndex -= actual;

                return actual;
            }
    }

    /// <summary>
    /// Tries to navigate to the next (i.e. newer) '<paramref name="count"/>'-th history item and returns how many navigation steps could be performed.
    /// </summary>
    /// <param name="count">Number of forward navigation steps to be taken.</param>
    /// <returns>The number of performed navigation steps.</returns>
    public int NavigateForward(int count)
    {
        if (count < 0)
            return NavigateBack(-count);
        else
            lock (this)
            {
                int actual = Math.Min(HistorySize - HistoryIndex - 1, count);

                HistoryIndex += actual;

                return actual;
            }
    }

    /// <summary>
    /// Tries to navigate to the history's newest item and returns whether the navigation process was successful.
    /// </summary>
    /// <returns>Indicates whether the forward navigation could be performed.</returns>
    public bool NavigateToNewest() => NavigateToIndex(Index.Start);

    /// <summary>
    /// Tries to navigate to the history's oldest item and returns whether the navigation process was successful.
    /// </summary>
    /// <returns>Indicates whether the backward navigation could be performed.</returns>
    public bool NavigateToOldest() => NavigateToIndex(Index.End);

    /// <summary>
    /// Adds the given item to the history at the current <see cref="HistoryIndex"/> and navigates directly to the newly added item.
    /// <para/>
    /// <b>NOTE:</b> This operation may ovewrite future history items when not performed at the history's end (i.e. the history's newest item).
    /// This may truncate the history.
    /// </summary>
    /// <param name="target">The item to be added.</param>
    public void Add(T target)
    {
        lock (this)
        {
            ++HistoryIndex;

            if (HistoryIndex == HistorySize)
                _history.Add(target);
            else
            {
                _history.Insert(HistoryIndex, target);
                _history.RemoveRange(HistoryIndex + 1, HistorySize - HistoryIndex);
            }
        }
    }

    /// <summary>
    /// Tries to navigate to the given target item (if existent) and returns whether the navigation was successful.
    /// </summary>
    /// <param name="target">The target element to be navigated to.</param>
    /// <returns>Indicates whether the navigation process was successful.</returns>
    public bool TryNavigateTo(T target) => TryNavigateTo(target, out _);

    /// <summary>
    /// Tries to navigate to the given target item (if existent) and returns the target index.
    /// </summary>
    /// <param name="target">The target element to be navigated to.</param>
    /// <param name="index">The target index after navigation (The index is undefined if the return value is <see langword="false"/>).</param>
    /// <returns>Indicates whether the navigation process was successful.</returns>
    public bool TryNavigateTo(T target, out Index index)
    {
        (bool result, index) = (false, default);

        lock (this)
            if (_history.LastIndexOf(target) is int i and >= 0)
                (result, index) = (true, HistoryIndex = i);

        return result;
    }

    /// <summary>
    /// Tries to navigate to last occurrence of the given item.
    /// If no occurrence has been found, the histoy will insert (or append) the given item after the current <see cref="HistoryIndex"/> and navigate to it.
    /// </summary>
    /// <param name="target">The item to added or navigated to.</param>
    public void NavigateOrAdd(T target)
    {
        if (TryNavigateTo(target, out Index i))
            NavigateToIndex(i);
        else
            Add(target);
    }

    /// <summary>
    /// Tries to navigate to the given index and returns whether the navigation process was successful.
    /// </summary>
    /// <param name="index">The target history index.</param>
    /// <param name="relative">Indicates whether the navigation should be performed relative to the current <see cref="HistoryIndex"/>.
    /// The behaviour of this method is equal to <see cref="NavigateToIndex(Index)"/> if the provided value is <see langword="false"/>.
    /// </param>
    /// <returns>Indication, whether the navigation could successfully be performed.</returns>
    public bool TryNavigateTo(Index index, bool relative)
    {
        bool m;

        lock (this)
        {
            int i = index.GetOffset(HistorySize);

            if (relative)
                i += HistoryIndex;

            if (m = i >= 0 && i < HistorySize)
                HistoryIndex = i;
        }

        return m;
    }

    /// <summary>
    /// Tries to navigate to the given absolute index and returns whether the navigation process was successful.
    /// </summary>
    /// <param name="index">The target history index.</param>
    /// <returns>Indication, whether the navigation could successfully be performed.</returns>
    public bool NavigateToIndex(Index index) => TryNavigateTo(index, false);

    /// <summary>
    /// Returns the item at the given index.
    /// </summary>
    /// <param name="index">Item index.</param>
    /// <param name="relative">Indicator, which indicates whether the item should be taken relative to the current <see cref="HistoryIndex"/>.</param>
    /// <returns>The obtained item.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public T GetItem(Index index, bool relative = false)
    {
        lock (this)
            return _history[index.GetOffset(HistorySize) + (relative ? 0 : HistoryIndex)];
    }

    /// <summary>
    /// Returns a slice of the current history.
    /// </summary>
    /// <param name="range">Slice range indices.</param>
    /// <param name="relative">Indicator, which indicates whether the range should be taken relative to the current <see cref="HistoryIndex"/>.</param>
    /// <returns>The history slice.</returns>
    public ConcurrentHistory<T> Slice(Range range, bool relative = false)
    {
        T[] slice;

        lock (this)
        {
            int start = range.Start.GetOffset(HistorySize);
            int end = range.Start.GetOffset(HistorySize);

            if (relative)
            {
                start += HistoryIndex;
                end += HistoryIndex;
            }

            start = start < 0 ? 0 : start >= HistorySize ? HistorySize - 1 : start;
            end = end < 0 ? 0 : end >= HistorySize ? HistorySize - 1 : end;
            slice = _history.Skip(start).Take(end - start).ToArray();
        }

        return new(slice);
    }

    /// <summary>
    /// Returns the enumerator which iterates through the current history from the oldest element up to the current element (inclusive).
    /// </summary>
    /// <returns>History enumerator.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        T[] slice;

        lock (this)
            slice = _history.Take(HistoryIndex + 1).ToArray();

        foreach (T item in slice)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
