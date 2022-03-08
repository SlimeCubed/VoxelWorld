// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared/dxgi.h in the Windows SDK for Windows 10.0.20348.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX
{
    [Guid("54EC77FA-1377-44E6-8C32-88FD5F44C84C")]
    [NativeTypeName("struct IDXGIDevice : IDXGIObject")]
    [NativeInheritance("IDXGIObject")]
    public unsafe partial struct IDXGIDevice : IDXGIDevice.Interface
    {
        public void** lpVtbl;

        [VtblIndex(0)]
        public HRESULT QueryInterface([NativeTypeName("const IID &")] Guid* riid, void** ppvObject)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, Guid*, void**, int>)(lpVtbl[0]))((IDXGIDevice*)Unsafe.AsPointer(ref this), riid, ppvObject);
        }

        [VtblIndex(1)]
        [return: NativeTypeName("ULONG")]
        public uint AddRef()
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, uint>)(lpVtbl[1]))((IDXGIDevice*)Unsafe.AsPointer(ref this));
        }

        [VtblIndex(2)]
        [return: NativeTypeName("ULONG")]
        public uint Release()
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, uint>)(lpVtbl[2]))((IDXGIDevice*)Unsafe.AsPointer(ref this));
        }

        [VtblIndex(3)]
        public HRESULT SetPrivateData([NativeTypeName("const GUID &")] Guid* Name, uint DataSize, [NativeTypeName("const void *")] void* pData)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, Guid*, uint, void*, int>)(lpVtbl[3]))((IDXGIDevice*)Unsafe.AsPointer(ref this), Name, DataSize, pData);
        }

        [VtblIndex(4)]
        public HRESULT SetPrivateDataInterface([NativeTypeName("const GUID &")] Guid* Name, [NativeTypeName("const IUnknown *")] IUnknown* pUnknown)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, Guid*, IUnknown*, int>)(lpVtbl[4]))((IDXGIDevice*)Unsafe.AsPointer(ref this), Name, pUnknown);
        }

        [VtblIndex(5)]
        public HRESULT GetPrivateData([NativeTypeName("const GUID &")] Guid* Name, uint* pDataSize, void* pData)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, Guid*, uint*, void*, int>)(lpVtbl[5]))((IDXGIDevice*)Unsafe.AsPointer(ref this), Name, pDataSize, pData);
        }

        [VtblIndex(6)]
        public HRESULT GetParent([NativeTypeName("const IID &")] Guid* riid, void** ppParent)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, Guid*, void**, int>)(lpVtbl[6]))((IDXGIDevice*)Unsafe.AsPointer(ref this), riid, ppParent);
        }

        [VtblIndex(7)]
        public HRESULT GetAdapter(IDXGIAdapter** pAdapter)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIDevice*, IDXGIAdapter**, int>)(lpVtbl[7]))((IDXGIDevice*)Unsafe.AsPointer(ref this), pAdapter);
        }

        public interface Interface : IDXGIObject.Interface
        {
            [VtblIndex(7)]
            HRESULT GetAdapter(IDXGIAdapter** pAdapter);
        }
    }
}
