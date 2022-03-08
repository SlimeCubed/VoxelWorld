using System;
using System.Diagnostics;

namespace TerraFX.Interop;

/// <summary>Defines the vtbl index of a method as it was in the native signature.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
[Conditional("DEBUG")]
internal sealed partial class VtblIndexAttribute : Attribute
{
    private readonly uint _index;

    /// <summary>Initializes a new instance of the <see cref="VtblIndexAttribute" /> class.</summary>
    /// <param name="index">The vtbl index of a method as it was in the native signature.</param>
    public VtblIndexAttribute(uint index)
    {
        _index = index;
    }

    /// <summary>Gets the vtbl index of a method as it was in the native signature.</summary>
    public uint Index => _index;
}

/// <summary>Defines the type of a member as it was used in the native signature.</summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
[Conditional("DEBUG")]
internal sealed partial class NativeTypeNameAttribute : Attribute
{
    private readonly string _name;

    /// <summary>Initializes a new instance of the <see cref="NativeTypeNameAttribute" /> class.</summary>
    /// <param name="name">The name of the type that was used in the native signature.</param>
    public NativeTypeNameAttribute(string name)
    {
        _name = name;
    }

    /// <summary>Gets the name of the type that was used in the native signature.</summary>
    public string Name => _name;
}

/// <summary>Defines the base type of a struct as it was in the native signature.</summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
[Conditional("DEBUG")]
internal sealed partial class NativeInheritanceAttribute : Attribute
{
    private readonly string _name;

    /// <summary>Initializes a new instance of the <see cref="NativeInheritanceAttribute" /> class.</summary>
    /// <param name="name">The name of the base type that was inherited from in the native signature.</param>
    public NativeInheritanceAttribute(string name)
    {
        _name = name;
    }

    /// <summary>Gets the name of the base type that was inherited from in the native signature.</summary>
    public string Name => _name;
}
