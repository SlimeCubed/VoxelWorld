// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared/dxgi1_4.h in the Windows SDK for Windows 10.0.20348.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX
{
    [Guid("645967A4-1392-4310-A798-8053CE3E93FD")]
    [NativeTypeName("struct IDXGIAdapter3 : IDXGIAdapter2")]
    [NativeInheritance("IDXGIAdapter2")]
    public unsafe partial struct IDXGIAdapter3 : IDXGIAdapter3.Interface
    {
        public void** lpVtbl;

        [VtblIndex(0)]
        public HRESULT QueryInterface([NativeTypeName("const IID &")] Guid* riid, void** ppvObject)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, Guid*, void**, int>)(lpVtbl[0]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), riid, ppvObject);
        }

        [VtblIndex(1)]
        [return: NativeTypeName("ULONG")]
        public uint AddRef()
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, uint>)(lpVtbl[1]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this));
        }

        [VtblIndex(2)]
        [return: NativeTypeName("ULONG")]
        public uint Release()
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, uint>)(lpVtbl[2]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this));
        }

        [VtblIndex(3)]
        public HRESULT SetPrivateData([NativeTypeName("const GUID &")] Guid* Name, uint DataSize, [NativeTypeName("const void *")] void* pData)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, Guid*, uint, void*, int>)(lpVtbl[3]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), Name, DataSize, pData);
        }

        [VtblIndex(4)]
        public HRESULT SetPrivateDataInterface([NativeTypeName("const GUID &")] Guid* Name, [NativeTypeName("const IUnknown *")] IUnknown* pUnknown)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, Guid*, IUnknown*, int>)(lpVtbl[4]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), Name, pUnknown);
        }

        [VtblIndex(5)]
        public HRESULT GetPrivateData([NativeTypeName("const GUID &")] Guid* Name, uint* pDataSize, void* pData)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, Guid*, uint*, void*, int>)(lpVtbl[5]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), Name, pDataSize, pData);
        }

        [VtblIndex(6)]
        public HRESULT GetParent([NativeTypeName("const IID &")] Guid* riid, void** ppParent)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, Guid*, void**, int>)(lpVtbl[6]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), riid, ppParent);
        }

        [VtblIndex(8)]
        public HRESULT GetDesc(DXGI_ADAPTER_DESC* pDesc)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, DXGI_ADAPTER_DESC*, int>)(lpVtbl[8]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), pDesc);
        }
        
        [VtblIndex(14)]
        public HRESULT QueryVideoMemoryInfo(uint NodeIndex, DXGI_MEMORY_SEGMENT_GROUP MemorySegmentGroup, DXGI_QUERY_VIDEO_MEMORY_INFO* pVideoMemoryInfo)
        {
            return ((delegate* unmanaged[Stdcall]<IDXGIAdapter3*, uint, DXGI_MEMORY_SEGMENT_GROUP, DXGI_QUERY_VIDEO_MEMORY_INFO*, int>)(lpVtbl[14]))((IDXGIAdapter3*)Unsafe.AsPointer(ref this), NodeIndex, MemorySegmentGroup, pVideoMemoryInfo);
        }
        
        public interface Interface : IDXGIAdapter.Interface
        {
            [VtblIndex(14)]
            HRESULT QueryVideoMemoryInfo(uint NodeIndex, DXGI_MEMORY_SEGMENT_GROUP MemorySegmentGroup, DXGI_QUERY_VIDEO_MEMORY_INFO* pVideoMemoryInfo);
        }
    }
}
