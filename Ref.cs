using System.Runtime.CompilerServices;


namespace Unknown6656.Generics;


public readonly unsafe struct Ref<T>
    where T : unmanaged
{
    public static Ref<T> Null { get; } = new((T*)null);

    public readonly T* Pointer;

    public readonly bool IsNull => Pointer is null;

    public readonly T Value => *Pointer;

    public readonly ref T Reference => ref Unsafe.AsRef<T>(Pointer);


    public Ref(Ref<T> @ref)
        : this(@ref.Pointer)
    {
    }

    public Ref(T* pointer) => Pointer = pointer;

    public Ref(T** pointer)
        : this(*pointer)
    {
    }

    public Ref(ref T variable)
        : this((T*)Unsafe.AsPointer(ref variable))
    {
    }

    public readonly Ref<U> To<U>() where U : unmanaged => new((U*)Pointer);

    public static implicit operator T(Ref<T> @ref) => @ref.Value;
}
