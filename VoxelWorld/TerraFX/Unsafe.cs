namespace System.Runtime.CompilerServices;

public static unsafe class Unsafe
{
    public static IntPtr AsPointer<T>(ref T t) where T : unmanaged
    {
        fixed (T* ptr = &t)
        {
            return (IntPtr)ptr;
        }
    }
}