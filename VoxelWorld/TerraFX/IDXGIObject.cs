// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared/dxgi.h in the Windows SDK for Windows 10.0.20348.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX
{
    [Guid("AEC22FB8-76F3-4639-9BE0-28EB43A67A2E")]
    [NativeTypeName("struct IDXGIObject : IUnknown")]
    [NativeInheritance("IUnknown")]
    public unsafe partial struct IDXGIObject : IDXGIObject.Interface
    {
        public void** lpVtbl;

        [VtblIndex(0)]
        public HRESULT QueryInterface([NativeTypeName("const IID &")] Guid* riid, void** ppvObject)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIObject*, Guid*, void**, int>)(lpVtbl[0]))((IDXGIObject*)Unsafe.AsPointer(ref this), riid, ppvObject);
        }

        [VtblIndex(1)]
        [return: NativeTypeName("ULONG")]
        public uint AddRef()
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIObject*, uint>)(lpVtbl[1]))((IDXGIObject*)Unsafe.AsPointer(ref this));
        }

        [VtblIndex(2)]
        [return: NativeTypeName("ULONG")]
        public uint Release()
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIObject*, uint>)(lpVtbl[2]))((IDXGIObject*)Unsafe.AsPointer(ref this));
        }

        public interface Interface : IUnknown.Interface
        {

        }
    }
}
