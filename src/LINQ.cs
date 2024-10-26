using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using System.Reflection;

namespace Unknown6656.Generics;

using static Math;


/// <summary>
/// A delegate which accepts a pointer of <typeparamref name="T"/> as single parameter and which does not return any value.
/// </summary>
/// <typeparam name="T">The generic pointer base type.</typeparam>
/// <param name="pointer">A pointer of the type <typeparamref name="T"/>.</param>
public unsafe delegate void PointerCallback<T>(T* pointer) where T : unmanaged;

/// <summary>
/// A delegate which accepts a pointer of <typeparamref name="T"/> as single parameter and which returns a value of the type <typeparamref name="U"/>.
/// </summary>
/// <typeparam name="T">The generic pointer base type.</typeparam>
/// <typeparam name="U">The generic return type.</typeparam>
/// <param name="pointer">A pointer of the type <typeparamref name="T"/>.</param>
public unsafe delegate U PointerCallback<T, U>(T* pointer) where T : unmanaged;

/// <summary>
/// A static class containing a set of LINQ (language-integrated queries) methods and extensions.
/// The <see cref="LINQ"/>-class further contains generic extension methods suitable for every-day use.
/// </summary>
public static partial class LINQ
{
    /// <summary>
    /// The empty function NO-OP which consists of the following code:
    /// <para/>
    /// <para/>
    /// <code><see cref="Action"/> <see cref="NoOp"/> = <see langword="new"/>(<see langword="delegate"/> { });</code>
    /// </summary>
    public static Action NoOp { get; } = new(delegate { });

    /// <summary>
    /// Returns the internal array from the given list (by reference, not by value).
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="list">List</param>
    /// <returns>Internal array.</returns>
    public static T[] GetInternalArray<T>(this List<T> list) =>
        (list.GetType()?.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(list) as T[]) ?? throw new InvalidProgramException($"This function depends on the type {typeof(List<>)} having a field '_items'.");

    /// <summary>
    /// Pins/fixes the variable <paramref name="array"/> for the duration of this method and passes the pinned pointer to <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="array">The array to be pinned.</param>
    /// <param name="callback">The callback to be called.</param>
    public unsafe static void Fixed<T>(this T[] array, PointerCallback<T> callback) where T : unmanaged
    {
        fixed (T* ptr = array)
            callback(ptr);
    }

    /// <summary>
    /// Pins/fixes the variable <paramref name="array"/> for the duration of this method and passes the pinned pointer to <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <typeparam name="U">The return type of the callback.</typeparam>
    /// <param name="array">The array to be pinned.</param>
    /// <param name="callback">The callback to be called.</param>
    public unsafe static U Fixed<T, U>(this T[] array, PointerCallback<T, U> callback) where T : unmanaged
    {
        fixed (T* ptr = array)
            return callback(ptr);
    }

    /// <summary>
    /// Pins/fixes the variable <paramref name="list"/> for the duration of this method and passes the pinned pointer to <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="T">The list element type.</typeparam>
    /// <param name="list">The list to be pinned.</param>
    /// <param name="callback">The callback to be called.</param>
    public static void Fixed<T>(this List<T> list, PointerCallback<T> callback) where T : unmanaged => Fixed(list.GetInternalArray(), callback);

    /// <summary>
    /// Pins/fixes the variable <paramref name="list"/> for the duration of this method and passes the pinned pointer to <paramref name="callback"/>.
    /// </summary>
    /// <typeparam name="T">The list element type.</typeparam>
    /// <typeparam name="U">The return type of the callback.</typeparam>
    /// <param name="list">The list to be pinned.</param>
    /// <param name="callback">The callback to be called.</param>
    public static U Fixed<T, U>(this List<T> list, PointerCallback<T, U> callback) where T : unmanaged => Fixed(list.GetInternalArray(), callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Resize<T>(this T[] array, int new_size)
    {
        Array.Resize(ref array, new_size);

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T With<T>(this T value, Action<T> func)
        where T : class
    {
        func(value);

        return value;
    }

    /// <summary>
    /// Executes the given function or lambda expression if it is not <see langword="null"/>.
    /// This method may be useful in <see langword="switch"/>-expressions or function wrapping.
    /// </summary>
    /// <param name="func">The function or lambda expression to be executed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Do(this Action? func) => func?.Invoke();

    /// <summary>
    /// Executes the given function or lambda expression and returns its result.
    /// This method may be useful in <see langword="switch"/>-expressions or function wrapping.
    /// </summary>
    /// <typeparam name="T">The generic type parameter which matches the result type.</typeparam>
    /// <param name="func">The function or lambda expression to be executed.</param>
    /// <returns>The result of the given function or lambda expression.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Do<T>(this Func<T> func) => func.Invoke();

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static void Do<T>(this T value, Action<T>? func) => func?.Invoke(value);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static unsafe void Do<T>(this T value, delegate*<T, void> func) => func(value);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static U Do<T, U>(this T value, Func<T, U> func) => func(value);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static unsafe U Do<T, U>(this T value, delegate*<T, U> func) => func(value);

    /// <summary>
    /// Executes the given function pointer if it is not a <see langword="null"/>-pointer.
    /// This method may be useful in <see langword="switch"/>-expressions or function wrapping.
    /// </summary>
    /// <param name="func">The function pointer to be executed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Do(delegate*<void> func)
    {
        if (func != null)
            func();
    }

    /// <summary>
    /// Executes the given function pointer and returns its result.
    /// This method may be useful in <see langword="switch"/>-expressions or function wrapping.
    /// </summary>
    /// <typeparam name="T">The generic type parameter which matches the result type.</typeparam>
    /// <param name="func">The function pointer to be executed.</param>
    /// <returns>The result of the function call.</returns>
    /// <exception cref="ArgumentNullException">Thrown, if <paramref name="func"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T Do<T>(delegate*<T> func) => func == null ? throw new ArgumentNullException(nameof(func)) : func();

    /// <summary>
    /// Tries to silently execute the given function and catches all occuring exceptions.
    /// </summary>
    /// <param name="func">The function to be executed.</param>
    public static void TryDo(this Action? func)
    {
        try
        {
            if (func is { } f)
                f();
        }
        catch
        {
        }
    }

    /// <summary>
    /// Tries to silently execute the given function and return its result.
    /// If the given function throws an exception or is <see langword="null"/>, the <paramref name="default_value"/> will be returned.
    /// </summary>
    /// <typeparam name="T">The generic type parameter which matches the result type.</typeparam>
    /// <param name="func">The function pointer to be executed.</param>
    /// <param name="default_value">The default return value.</param>
    /// <returns>The return value obtained by either the function or the default value.</returns>
    public static T TryDo<T>(this Func<T>? func, T default_value)
    {
        try
        {
            if (func is { } f)
                return f();
        }
        catch
        {
        }

        return default_value;
    }

    /// <summary>
    /// Tries to silently execute the given function pointer and catches all occuring exceptions.
    /// </summary>
    /// <param name="func">The function pointer to be executed.</param>
    public static unsafe void TryDo(delegate*<void> func)
    {
        try
        {
#pragma warning disable IDE1005
            if (func != null)
                func();
#pragma warning restore
        }
        catch
        {
        }
    }

    /// <summary>
    /// Tries to silently execute the given function pointer and return its result.
    /// If the given function pointer throws an exception or is <see langword="null"/>, the <paramref name="default_value"/> will be returned.
    /// </summary>
    /// <typeparam name="T">The generic type parameter which matches the result type.</typeparam>
    /// <param name="func">The function pointer pointer to be executed.</param>
    /// <param name="default_value">The default return value.</param>
    /// <returns>The return value obtained by either the function or the default value.</returns>
    public static unsafe T TryDo<T>(delegate*<T> func, T default_value)
    {
        try
        {
            if (func != null)
                return func();
        }
        catch
        {
        }

        return default_value;
    }

    /// <summary>
    /// Tries to execute the first working function and retuns its result.
    /// If the first of the given functions throws an exception, the following function will be executed.
    /// An <see cref="AggregateException"/> will be thrown if no function successfully returns a value.
    /// An <see cref="ArgumentException"/> will be thrown if the collection "<paramref name="functions"/>" does not contain any function.
    /// </summary>
    /// <typeparam name="T">Generic return type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="functions">The collection of functions to be executed</param>
    /// <returns>The first successful return value.</returns>
    /// <exception cref="AggregateException">Thrown if all functions fail.</exception>
    /// <exception cref="ArgumentException">Thrown if no function has been provided.</exception>
    public static T TryAll<T>(params Func<T>[] functions)
    {
        List<Exception> exceptions = new();

        foreach (Func<T> f in functions)
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

        throw exceptions.Count == 0 ? new ArgumentException("No functions have been provided.", nameof(functions)) : new AggregateException(exceptions.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T DelegateFromPointer<T>(void* pointer) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>((nint)pointer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* DelegateToPointer<T>(this T @delegate) where T : Delegate => (void*)Marshal.GetFunctionPointerForDelegate(@delegate);

#pragma warning disable IDE1006 // naming style
    /// <summary>
    /// The identity function. This function returns the parameter <paramref name="x"/> unmodified.
    /// </summary>
    /// <typeparam name="T">Generic type parameter.</typeparam>
    /// <param name="x">Input value.</param>
    /// <returns>The unmodified input value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("x")]
    public static T id<T>(T x) => x;

    /// <summary>
    /// The first element tuple selector function. This function returns the unmodified first element of the given tuple <paramref name="x"/>.
    /// </summary>
    /// <typeparam name="T">Generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">Generic type parameter <typeparamref name="U"/>.</typeparam>
    /// <param name="x">Input tuple.</param>
    /// <returns>First element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("x.Item1")]
    public static T fst<T, U>((T t, U) x) => x.t;

    /// <summary>
    /// The first element tuple selector function. This function returns the unmodified first element of the given tuple <paramref name="x"/>.
    /// </summary>
    /// <typeparam name="T">Generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">Generic type parameter <typeparamref name="U"/>.</typeparam>
    /// <typeparam name="V">Generic type parameter <typeparamref name="V"/>.</typeparam>
    /// <param name="x">Input tuple.</param>
    /// <returns>First element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("x.Item1")]
    public static T fst<T, U, V>((T t, U, V) x) => x.t;

    /// <summary>
    /// The second element tuple selector function. This function returns the unmodified second element of the given tuple <paramref name="x"/>.
    /// </summary>
    /// <typeparam name="T">Generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">Generic type parameter <typeparamref name="U"/>.</typeparam>
    /// <param name="x">Input tuple.</param>
    /// <returns>Second element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("x.Item2")]
    public static U snd<T, U>((T, U u) x) => x.u;

    /// <summary>
    /// The second element tuple selector function. This function returns the unmodified second element of the given tuple <paramref name="x"/>.
    /// </summary>
    /// <typeparam name="T">Generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">Generic type parameter <typeparamref name="U"/>.</typeparam>
    /// <typeparam name="V">Generic type parameter <typeparamref name="V"/>.</typeparam>
    /// <param name="x">Input tuple.</param>
    /// <returns>Second element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("x.Item2")]
    public static U snd<T, U, V>((T, U u, V) x) => x.u;

    /// <summary>
    /// The third element tuple selector function. This function returns the unmodified third element of the given tuple <paramref name="x"/>.
    /// </summary>
    /// <typeparam name="T">Generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">Generic type parameter <typeparamref name="U"/>.</typeparam>
    /// <typeparam name="V">Generic type parameter <typeparamref name="V"/>.</typeparam>
    /// <param name="x">Input tuple.</param>
    /// <returns>Third element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull("x.Item3")]
    public static V trd<T, U, V>((T, U, V v) x) => x.v;
#pragma warning restore IDE1006

    public static IEnumerable<T[]> SplitBy<T>(this IEnumerable<T> collection, T separator, bool return_empty_collections = true) =>
        SplitBy(collection, separator, EqualityComparer<T>.Default, return_empty_collections);

    public static IEnumerable<T[]> SplitBy<T>(this IEnumerable<T> collection, T separator, IEqualityComparer<T> compararer, bool return_empty_collections = true) =>
        SplitBy(collection, t => compararer.Equals(t, separator), return_empty_collections);

    public static IEnumerable<T[]> SplitBy<T>(this IEnumerable<T> collection, IEnumerable<T> separators, bool return_empty_collections = true) =>
        SplitBy(collection, separators, null, return_empty_collections);

    public static IEnumerable<T[]> SplitBy<T>(this IEnumerable<T> collection, IEnumerable<T> separators, IEqualityComparer<T>? compararer, bool return_empty_collections = true) =>
        SplitBy(collection, t => separators.Contains(t, compararer), return_empty_collections);

    public static IEnumerable<T[]> SplitBy<T>(this IEnumerable<T> collection, Func<T, bool> predicate, bool return_empty_collections = true)
    {
        List<T> current = [];

        foreach (T item in collection)
            if (predicate(item))
            {
                if (current.Count > 0 || return_empty_collections)
                    yield return current.ToArray();

                current.Clear();
            }
            else
                current.Add(item);

        if (current.Count > 0 || return_empty_collections)
            yield return current.ToArray();
    }


    /// <inheritdoc cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
    public static int Count(this IEnumerable? collection)
    {
        int count = 0;

        if (collection is { })
            foreach (object? _ in collection)
                ++count;

        return count;
    }

    public static int MaxIndex<T>(this IEnumerable<T> collection) where T : IComparable<T> => MaxIndex(collection, id);

    public static int MaxIndex<T, U>(this IEnumerable<T> collection, Func<T, U> selector)
        where U : IComparable<U>
    {
        U[] array = collection.ToArray(selector);
        U max = array.Max();

        return array.IndexOf(max);
    }

    public static int MinIndex<T>(this IEnumerable<T> collection) where T : IComparable<T> => MinIndex(collection, id);

    public static int MinIndex<T, U>(this IEnumerable<T> collection, Func<T, U> selector)
        where U : IComparable<U>
    {
        U[] array = collection.ToArray(selector);
        U min = array.Min();

        return array.IndexOf(min);
    }

    public static int IndexOf<T>(this IEnumerable<T> collection, T item) => IndexWhere(collection, t => Equals(t, item));

    public static int IndexWhere<T>(this IEnumerable<T> collection, Predicate<T> predicate)
    {
        T[] array = collection as T[] ?? collection.ToArray();

        if (array.Length < 512)
        {
            for (int i = 0; i < array.Length; ++i)
                if (predicate(array[i]))
                    return i;
        }
        else
        {
            ConcurrentQueue<int> index = new();
            CancellationTokenSource source = new();
            ParallelOptions options = new()
            {
                CancellationToken = source.Token,
            };

            try
            {
                Parallel.For(0, array.Length, options, i =>
                {
                    if (!source.IsCancellationRequested && predicate(array[i]))
                    {
                        index.Enqueue(i);
                        source.Cancel();
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }

            if (index.TryDequeue(out int i))
                return i;
        }

        return -1;
    }

    public static int LastIndexOf<T>(this IEnumerable<T> collection, T item) => LastIndexWhere(collection, t => Equals(t, item));

    public static int LastIndexWhere<T>(this IEnumerable<T> collection, Predicate<T> predicate) => IndexWhere(collection.Reverse(), predicate);

    /// <summary>
    /// Swaps the elements of the given tuple.
    /// </summary>
    /// <typeparam name="T">The generic tuple element type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">The generic tuple element type parameter <typeparamref name="U"/>.</typeparam>
    /// <param name="tuple">Input tuple.</param>
    /// <returns>Output tuple with swapped elements.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (U, T) Swap<T, U>(this (T t, U u) tuple) => (tuple.u, tuple.t);

    /// <summary>
    /// Swaps all tuples element-wise in the given collection.
    /// </summary>
    /// <typeparam name="T">The generic tuple element type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="U">The generic tuple element type parameter <typeparamref name="U"/>.</typeparam>
    /// <param name="collection">The input collection.</param>
    /// <returns>The output collection, in which all tuples have their elements swapped.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(U, T)> Swap<T, U>(this IEnumerable<(T, U)> collection) => collection.Select(Swap);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IEnumerable<T>> PowerSet<T>(this T[] collection)
    {
        T[][] set = new T[1 << collection.Length][];
        T[] src, dst;
        T elem;

        set[0] = [];

        for (int i = 0, j, count; i < collection.Length; i++)
        {
            elem = collection[i];

            for (j = 0, count = 1 << i; j < count; j++)
            {
                src = set[j];
                dst = set[count + j] = new T[src.Length + 1];

                for (int q = 0; q < src.Length; q++)
                    dst[q] = src[q];

                dst[src.Length] = elem;
            }
        }

        return set;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringJoinLines<T>(this IEnumerable<T> collection, string line_break = "\n") => collection.StringJoin(line_break);

    /// <inheritdoc cref="string.Join{T}(string?, IEnumerable{T})"/>
    /// <param name="collection">The collection to be joined.</param>
    /// <param name="separator">The separator string.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringJoin<T>(this IEnumerable<T> collection, string separator) => string.Join(separator, collection);

    /// <inheritdoc cref="string.Concat{T}(IEnumerable{T})"/>
    /// <param name="collection">The collection to be joined.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string StringConcat<T>(this IEnumerable<T> collection) => string.Concat(collection);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IEnumerable<T>> PartitionBySetCount<T>(this IEnumerable<T> collection, int number_of_sets) => from t in collection.WithIndex()
                                                                                                                            group t.Item by t.Index % number_of_sets into g
                                                                                                                            select g as IEnumerable<T>;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[][] PartitionByArraySize<T>(this IEnumerable<T> collection, int max_items_per_array)
    {
        T[] items = collection as T[] ?? collection.ToArray();
        int arr_count = items.Length / max_items_per_array;

        while (arr_count * max_items_per_array < items.Length)
            ++arr_count;

        T[][] arrays = new T[arr_count][];

        for (int i = 0; i < arr_count; ++i)
        {
            int end = Min((i + 1) * max_items_per_array, items.Length);

            arrays[i] = items[(i * max_items_per_array)..end];
        }

        return arrays;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[][] PartitionByArrayCount<T>(this IEnumerable<T> collection, int number_of_sets)
    {
        T[] items = collection as T[] ?? collection.ToArray();
        T[][] sets = new T[number_of_sets][];
        int items_per_set = items.Length / number_of_sets;

        for (int i = 0; i < number_of_sets; ++i)
        {
            int end = Min((i + 1) * number_of_sets, items.Length);

            sets[i] = items[(i * number_of_sets)..end];
        }

        return sets;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] HeapSort<T>(this IEnumerable<T> coll) where T : IComparable<T> => BinaryHeap<T>.HeapSort(coll);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ZipArray<T>(this IEnumerable<T> coll1, IEnumerable<T> coll2, Func<T, T, T> func) => ZipArray<T, T, T>(coll1, coll2, func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static V[] ZipArray<T, U, V>(this IEnumerable<T> coll1, IEnumerable<U> coll2, Func<T, U, V> func)
    {
        T[] arr1 = coll1 as T[] ?? coll1.ToArray();
        U[] arr2 = coll2 as U[] ?? coll2.ToArray();
        V[] res = new V[Min(arr1.Length, arr2.Length)];

        for (int i = 0, l = res.Length; i < l; ++i)
            res[i] = func(arr1[i], arr2[i]);

        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<V> ZipOuter<T, U, V>(this IEnumerable<T> coll1, IEnumerable<U> coll2, Func<T, U, V> func)
    {
        T[] a = coll1 as T[] ?? coll1.ToArray();
        U[] b = coll2 as U[] ?? coll2.ToArray();
        int cmp = a.Length.CompareTo(b.Length);

        if (cmp < 0)
            Array.Resize(ref a, b.Length);
        else if (cmp > 0)
            Array.Resize(ref b, a.Length);

        return a.Zip(b, func);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(this IEnumerable<T> coll) => !coll.Any();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool None<T>(this IEnumerable<T> coll, Func<T, bool> func) => !coll.Any(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> coll, int start, int count) => coll.Skip(start).Take(count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> coll, Index start, Index end) => coll.Slice(start..end);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Slice<T>(this IEnumerable<T> collection, Range range) => (collection as T[] ?? collection.ToArray())[range];

    public static (List<T> @false, List<T> @true) Partition<T>(this IEnumerable<T> coll, Predicate<T> pred)
    {
        List<T> tl = new();
        List<T> fl = new();

        foreach (T t in coll)
            (pred(t) ? tl : fl).Add(t);

        return (fl, tl);
    }

    public static IEnumerable<(T t, U u)> Product<T, U>(this IEnumerable<T> coll1, IEnumerable<U> coll2)
    {
        foreach (T t in coll1)
            foreach (U u in coll2)
                yield return (t, u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableDictionary<T, U> ToObservableDictionary<T, U>(this IDictionary<T, U> dictionary) where T : notnull => new(dictionary);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableDictionary<T, U> ToObservableDictionary<T, U>(this IEnumerable<KeyValuePair<T, U>> dictionary) where T : notnull =>
        dictionary.ToDictionary<T, U>().ToObservableDictionary();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableDictionary<T, U> ToObservableDictionary<T, U>(this IEnumerable<Tuple<T, U>> dictionary) where T : notnull =>
        dictionary.ToDictionary().ToObservableDictionary();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableDictionary<T, U> ToObservableDictionary<T, U>(this IEnumerable<(T, U)> dictionary) where T : notnull =>
        dictionary.ToDictionary().ToObservableDictionary();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<U> ToList<T, U>(this IEnumerable<T> coll, Func<T, U> func) => coll.Select(func).ToList();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<U> ToList<T, U>(this IEnumerable<T> coll, Func<T, int, U> func) => coll.Select(func).ToList();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToListWhere<T>(this IEnumerable<T> coll, Func<T, bool> func) => coll.Where(func).ToList();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToListWhere<T>(this IEnumerable<T> coll, Func<T, int, bool> func) => coll.Where(func).ToList();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<U> ToListWhere<T, U>(this IEnumerable<T> coll, Func<T, bool> func, Func<T, U> map) => coll.Where(func).ToList(map);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<U> ToListWhere<T, U>(this IEnumerable<T> coll, Func<T, int, bool> func, Func<T, int, U> map) => coll.Where(func).ToList(map);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U[] ToArray<T, U>(this IEnumerable<T> coll, Func<T, U> func) => coll.Select(func).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U[] ToArray<T, U>(this IEnumerable<T> coll, Func<T, int, U> func) => coll.Select(func).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArrayWhere<T>(this IEnumerable<T> coll, Func<T, bool> func) => coll.Where(func).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArrayWhere<T>(this IEnumerable<T> coll, Func<T, int, bool> func) => coll.Where(func).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U[] ToArrayWhere<T, U>(this IEnumerable<T> coll, Func<T, bool> func, Func<T, U> map) => coll.Where(func).ToArray(map);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U[] ToArrayWhere<T, U>(this IEnumerable<T> coll, Func<T, int, bool> func, Func<T, int, U> map) => coll.Where(func).ToArray(map);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<U> SelectWhere<T, U>(this IEnumerable<T> coll, Func<T, bool> pred, Func<T, U> func) => coll.Where(pred).Select(func);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> coll) => coll.Select(Join);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> AppendToList<T>(this IEnumerable<T> coll, IList<T> list)
    {
        foreach (T t in coll)
            list.Add(t);

        return coll;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> PrependToList<T>(this IEnumerable<T> coll, IList<T> list)
    {
        foreach (T t in coll.Reverse())
            list.Insert(0, t);

        return coll;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<T, U> ToDictionary<T, U>(this IEnumerable<Tuple<T, U>> dictionary) where T : notnull => dictionary.ToDictionary(t => t.Item1, t => t.Item2);

#if !NET8_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<T, U> ToDictionary<T, U>(this IEnumerable<KeyValuePair<T, U>> dictionary) where T : notnull => dictionary.ToDictionary(t => t.Key, t => t.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Dictionary<T, U> ToDictionary<T, U>(this IEnumerable<(T key, U value)> pairs) where T : notnull => pairs.ToDictionary(fst, snd);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T key, U value)[] FromDictionary<T, U>(this IDictionary<T, U> dictionary) => dictionary.ToArray(kvp => (kvp.Key, kvp.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T key, U value)[] FromDictionary<T, U>(this IReadOnlyDictionary<T, U> dictionary) => dictionary.ToArray(kvp => (kvp.Key, kvp.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDictionary<T, U> Merge<T, U>(this IDictionary<T, U> dictionary, params IDictionary<T, U>[] others)
        where T : notnull => dictionary.Append(others).Merge();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDictionary<T, U> Merge<T, U>(this IEnumerable<IDictionary<T, U>?> dictionaries)
        where T : notnull
    {
        Dictionary<T, U> result = new();

        foreach (IDictionary<T, U>? dic in dictionaries)
            if (dic is { })
                foreach (T key in dic.Keys)
                    result[key] = dic[key];

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDictionary<T, IDictionary<U, V>> Merge<T, U, V>(this IDictionary<T, IDictionary<U, V>> dictionary, params IDictionary<T, IDictionary<U, V>>[] others)
        where T : notnull where U : notnull => dictionary.Append(others).Merge();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDictionary<T, IDictionary<U, V>> Merge<T, U, V>(this IEnumerable<IDictionary<T, IDictionary<U, V>>?> dictionaries)
        where T : notnull
        where U : notnull
    {
        Dictionary<T, IDictionary<U, V>> result = new();

        foreach (IDictionary<T, IDictionary<U, V>>? dic in dictionaries)
            if (dic is { })
                foreach (T key in dic.Keys)
                    result[key] = result.TryGetValue(key, out IDictionary<U, V>? existing) ? existing.Merge(dic[key]) : dic[key];

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U[] ToArray<T, U>(this IEnumerable<IEnumerable<T>> coll, Func<IEnumerable<T>, IEnumerable<U>> func) => coll.SelectMany(func).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U[] ToArray<T, U>(this IEnumerable<IEnumerable<T>> coll, Func<IEnumerable<T>, int, IEnumerable<U>> func) => coll.SelectMany(func).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SetEquals<T>(this IEnumerable<T>? set1, IEnumerable<T>? set2)
    {
        List<T> a1 = set1?.ToList() ?? new List<T>();
        List<T> a2 = set2?.ToList() ?? new List<T>();

        while (a1.Count == a2.Count)
        {
            if (a1.Count == 0)
                return true;

            T item = a1[0];
            int[] idx = a2.ToArrayWhere((t, _) => Equals(item, t), (_, i) => i);

            a1.RemoveAt(0);

            if (idx.Length > 0)
                a2.RemoveAt(idx[0]);
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IEnumerable<T>> SequentialDistinct<T>(this IEnumerable<IEnumerable<T>> coll) => coll.Distinct(new SequentialDistinctEqualityCompararer<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> coll)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = [[]];
        IEnumerable<IEnumerable<T>> result = emptyProduct;

        foreach (IEnumerable<T> sequence in coll)
            result = from accseq in result
                     from item in sequence
                     select accseq.Append(item);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T[]> Transpose<T>(this IEnumerable<IEnumerable<T>> coll)
    {
        IEnumerator<T>[] enums = coll.Select(e => e.GetEnumerator()).ToArray();

        try
        {
            while (enums.All(e => e.MoveNext()))
                yield return enums.Select(e => e.Current).ToArray();
        }
        finally
        {
            Array.ForEach(enums, e => e.Dispose());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Append<T>(this T item, IEnumerable<T> collection) => collection.Prepend(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ApplyRecursively<T>(this T element, Func<T, T> func, int count)
    {
        while (count > 0)
        {
            element = func(element);

            --count;
        }

        return element;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ApplyRecursively<T>(this T element, Func<T, T> func, int max_count, Predicate<T> @while)
    {
        while (max_count > 0 && @while(element))
        {
            element = func(element);

            --max_count;
        }

        return element;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ApplyRecursively<T>(this T element, Func<T, T> func, Predicate<T> @while)
    {
        while (@while(element))
            element = func(element);

        return element;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe U BinaryCast<T, U>(this T value) where T : unmanaged where U : unmanaged => *(U*)&value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe U[] CopyTo<T, U>(this T[] source) where T : unmanaged where U : unmanaged => source.CopyTo<T, U>(sizeof(T) * source.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe U[] CopyTo<T, U>(this T[] source, int byte_count)
        where T : unmanaged
        where U : unmanaged
    {
        U[] dest = new U[(int)Ceiling(byte_count / (float)sizeof(U))];

        dest.Fixed(ptrd => source.CopyTo(ptrd, byte_count));

        return dest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CopyTo<T, U>(this T[] source, U* target) where T : unmanaged where U : unmanaged => source.CopyTo(target, sizeof(T) * source.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void CopyTo<T, U>(this T[] source, U* target, int byte_count)
        where T : unmanaged
        where U : unmanaged => source.Fixed(ptrs =>
        {
            byte* bptrd = (byte*)target;
            byte* bptrs = (byte*)ptrs;

            for (int i = 0; i < byte_count; ++i)
                bptrd[i] = bptrs[i];
        });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Rotate<T>(this IEnumerable<T> collection, int count)
    {
        T[] array = collection as T[] ?? collection.ToArray();
        count = ((count % array.Length) + array.Length) % array.Length;

        return count is 0 ? array : array[count..].Concat(array[..count]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U AggregateCast<T, U>(this IEnumerable<T> coll, Func<U, U, U> accumulator)
        where T : U => coll.Cast<U>().Aggregate<U>(accumulator);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AggregateNonEmpty<T>(this IEnumerable<T> coll, Func<T, T, T> accumulator/*, U init = default*/)
    {
        List<T>? list = coll?.ToList();

        if (list is null)
            throw new ArgumentNullException(nameof(coll));
        else if (list.Count < 2)
            throw new ArgumentException("The given collection must have more than one element.", nameof(coll));

        T result = accumulator(list[0], list[1]);

        foreach (T t in list.Skip(2))
            result = accumulator(result, t);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AggregateNonEmpty<T>(this IEnumerable<T> coll, Func<T, T, T> accumulator, T init) => coll?.ToList() switch
    {
        null => throw new ArgumentNullException(nameof(coll)),
        { Count: 0 } => init,
        List<T> list => Do(() =>
        {
            list.Insert(0, init);

            return AggregateNonEmpty(list, accumulator);
        })
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Propagate<T>(this T start, Func<T, (T next, bool @continue)> next)
    {
        T current = start;

        yield return current;

        while (next(current) is (T n, true))
            yield return current = n;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Propagate<T>(this T start, Func<T, T> next, Predicate<T> @while)
    {
        T current = start;

        yield return current;

        while (@while(current))
            yield return current = next(current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Propagate<T>(this T start, Func<T, T> next, int max_count)
    {
        T current = start;

        yield return current;

        while (max_count --> 0)
            yield return current = next(current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Propagate<T>(this T start, Func<T, (T next, bool @continue)> next, int max_count) => start.Propagate(t =>
    {
        (T res, bool cont) = next(t);

        return (res, cont && max_count-- > 0);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Propagate<T>(this T start, Func<T, T> next, Predicate<T> @while, int max_count) => start.Propagate(next, e => @while(e) && max_count --> 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Are<T>(this IEnumerable<T> xs, IEnumerable<T> ys, IEqualityComparer<T> comparer) => xs.Are(ys, comparer.Equals);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Are<T>(this IEnumerable<T> xs, IEnumerable<T> ys, Func<T, T, bool> comparer)
    {
        if (xs == ys || (xs?.Equals(ys) ?? ys is null))
            return true;

        using IEnumerator<T> ex = xs.GetEnumerator();
        using IEnumerator<T> ey = ys.GetEnumerator();
        bool cm;

        do
        {
            cm = ex.MoveNext();

            if (cm != ey.MoveNext() || (cm && !comparer(ex.Current, ey.Current)))
                return false;
        }
        while (cm);

        return true;
    }

    /// <summary>
    /// Tries to execute the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TryDo<T, _>(this IEnumerable<T> collection, Func<T, _> action)
    {
        foreach (T item in collection)
            TryDo(() => action(item));
    }

    /// <summary>
    /// Tries to execute the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void TryDo<T, _>(this IEnumerable<T> collection, delegate*<T, _> action)
    {
        foreach (T item in collection)
            TryDo(() => action(item));
    }

    /// <summary>
    /// Executes the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="_"><i>(Ignored type parameter)</i></typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Do<T, _>(this IEnumerable<T> collection, Func<T, _> action)
    {
        foreach (T item in collection)
            action(item);
    }

    /// <summary>
    /// Executes the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="_"><i>(Ignored type parameter)</i></typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Do<T, _>(this IEnumerable<T> collection, delegate*<T, _> action)
    {
        foreach (T item in collection)
            action(item);
    }

    /// <summary>
    /// Executes the given <paramref name="action"/> in parallel for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="_"><i>(Ignored type parameter)</i></typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoInParallel<T, _>(this IEnumerable<T> collection, Func<T, _> action) => Parallel.ForEach(collection, x => action(x));

    /// <summary>
    /// Executes the given <paramref name="action"/> in parallel for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <typeparam name="_"><i>(Ignored type parameter)</i></typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoInParallel<T, _>(this IEnumerable<T> collection, Func<T, _> action, ParallelOptions options) => Parallel.ForEach(collection, options, x => action(x));

    /// <summary>
    /// Tries to execute the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TryDo<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (T item in collection)
            TryDo(() => action(item));
    }

    /// <summary>
    /// Tries to execute the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void TryDo<T>(this IEnumerable<T> collection, delegate*<T, void> action)
    {
        foreach (T item in collection)
            TryDo(() => action(item));
    }

    /// <summary>
    /// Executes the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Do<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (T item in collection)
            action(item);
    }

    /// <summary>
    /// Executes the given <paramref name="action"/> for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Do<T>(this IEnumerable<T> collection, delegate*<T, void> action)
    {
        foreach (T item in collection)
            action(item);
    }

    /// <summary>
    /// Executes the given <paramref name="action"/> in parallel for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoInParallel<T>(this IEnumerable<T> collection, Action<T> action) => Parallel.ForEach(collection, action);

    /// <summary>
    /// Executes the given <paramref name="action"/> in parallel for each element in the given <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The collections of items.</param>
    /// <param name="action">The function to be executed on each item in the given collection.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoInParallel<T>(this IEnumerable<T> collection, Action<T> action, ParallelOptions options) => Parallel.ForEach(collection, options, action);

    /// <summary>
    /// Deterministically computes the hash code of the given collection.
    /// The hashcode is dependent on each item in the collection, as well as their order.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The input collection.</param>
    /// <returns>The computed hash code.</returns>
    public static int GetHashCode<T>(IEnumerable<T> collection) => GetHashCode(collection as T[] ?? collection.ToArray());

    /// <summary>
    /// Deterministically computes the hash code of the given collection.
    /// The hashcode is dependent on each item in the collection, as well as their order.
    /// </summary>
    /// <typeparam name="T">The generic type parameter <typeparamref name="T"/>.</typeparam>
    /// <param name="collection">The input collection.</param>
    /// <returns>The computed hash code.</returns>
    public static int GetHashCode<T>(params T[] collection)
    {
        int code = collection.Length;

        foreach (T elem in collection)
            code = HashCode.Combine(code, elem);

        return code;
    }
}
