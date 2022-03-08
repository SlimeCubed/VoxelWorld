using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX;

[Guid("C0BFA96C-E089-44FB-8EAF-26F8796190DA")]
[NativeTypeName("struct ID3D11DeviceContext : ID3D11DeviceChild")]
[NativeInheritance("ID3D11DeviceChild")]
public unsafe partial struct ID3D11DeviceContext : ID3D11DeviceContext.Interface
{
    public void** lpVtbl;

    [VtblIndex(0)]
    public HRESULT QueryInterface([NativeTypeName("const IID &")] Guid* riid, void** ppvObject)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, Guid*, void**, int>)(lpVtbl[0]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this), riid, ppvObject);
    }

    [VtblIndex(1)]
    [return: NativeTypeName("ULONG")]
    public uint AddRef()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, uint>)(lpVtbl[1]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this));
    }

    [VtblIndex(2)]
    [return: NativeTypeName("ULONG")]
    public uint Release()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, uint>)(lpVtbl[2]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this));
    }

    [VtblIndex(3)]
    public void GetDevice(ID3D11Device** ppDevice)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11Device**, void>)(lpVtbl[3]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), ppDevice);
    }

    [VtblIndex(4)]
    public HRESULT GetPrivateData([NativeTypeName("const GUID &")] Guid* guid, uint* pDataSize, void* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, Guid*, uint*, void*, int>)(lpVtbl[4]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), guid, pDataSize, pData);
    }

    [VtblIndex(5)]
    public HRESULT SetPrivateData([NativeTypeName("const GUID &")] Guid* guid, uint DataSize, [NativeTypeName("const void *")] void* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, Guid*, uint, void*, int>)(lpVtbl[5]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), guid, DataSize, pData);
    }

    [VtblIndex(6)]
    public HRESULT SetPrivateDataInterface([NativeTypeName("const GUID &")] Guid* guid, [NativeTypeName("const IUnknown *")] IUnknown* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, Guid*, IUnknown*, int>)(lpVtbl[6]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), guid, pData);
    }

    
    [VtblIndex(14)]
    public HRESULT Map(ID3D11Resource* pResource, uint Subresource, D3D11_MAP MapType, uint MapFlags,
        D3D11_MAPPED_SUBRESOURCE* pMappedResource)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11Resource*, uint, D3D11_MAP, uint,
            D3D11_MAPPED_SUBRESOURCE*, int>)(lpVtbl[14]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), pResource,
            Subresource, MapType, MapFlags, pMappedResource);
    }

    [VtblIndex(15)]
    public void Unmap(ID3D11Resource* pResource, uint Subresource)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11Resource*, uint, void>)(lpVtbl[15]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this), pResource, Subresource);
    }

    [VtblIndex(46)]
    public void CopySubresourceRegion(ID3D11Resource* pDstResource, uint DstSubresource, uint DstX, uint DstY,
        uint DstZ, ID3D11Resource* pSrcResource, uint SrcSubresource,
        [NativeTypeName("const D3D11_BOX *")] D3D11_BOX* pSrcBox)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11Resource*, uint, uint, uint, uint, ID3D11Resource*, uint,
            D3D11_BOX*, void>)(lpVtbl[46]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), pDstResource,
            DstSubresource, DstX, DstY, DstZ, pSrcResource, SrcSubresource, pSrcBox);
    }

    [VtblIndex(47)]
    public void CopyResource(ID3D11Resource* pDstResource, ID3D11Resource* pSrcResource)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11Resource*, ID3D11Resource*, void>)(lpVtbl[47]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this), pDstResource, pSrcResource);
    }

    [VtblIndex(48)]
    public void UpdateSubresource(ID3D11Resource* pDstResource, uint DstSubresource,
        [NativeTypeName("const D3D11_BOX *")] D3D11_BOX* pDstBox, [NativeTypeName("const void *")] void* pSrcData,
        uint SrcRowPitch, uint SrcDepthPitch)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11Resource*, uint, D3D11_BOX*, void*, uint, uint, void>)
            (lpVtbl[48]))((ID3D11DeviceContext*)Unsafe.AsPointer(ref this), pDstResource, DstSubresource, pDstBox,
            pSrcData, SrcRowPitch, SrcDepthPitch);
    }

    [VtblIndex(58)]
    public void ExecuteCommandList(ID3D11CommandList* pCommandList, BOOL RestoreContextState)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, ID3D11CommandList*, BOOL, void>)(lpVtbl[58]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this), pCommandList, RestoreContextState);
    }

    [VtblIndex(114)]
    public HRESULT FinishCommandList(BOOL RestoreDeferredContextState, ID3D11CommandList** ppCommandList)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceContext*, BOOL, ID3D11CommandList**, int>)(lpVtbl[114]))(
            (ID3D11DeviceContext*)Unsafe.AsPointer(ref this), RestoreDeferredContextState, ppCommandList);
    }

    public interface Interface : ID3D11DeviceChild.Interface
    {
        HRESULT Map(ID3D11Resource* pResource, uint Subresource, D3D11_MAP MapType, uint MapFlags, D3D11_MAPPED_SUBRESOURCE* pMappedResource);

        void Unmap(ID3D11Resource* pResource, uint Subresource);

        void CopySubresourceRegion(ID3D11Resource* pDstResource, uint DstSubresource, uint DstX, uint DstY, uint DstZ, ID3D11Resource* pSrcResource, uint SrcSubresource, [NativeTypeName("const D3D11_BOX *")] D3D11_BOX* pSrcBox);

        void CopyResource(ID3D11Resource* pDstResource, ID3D11Resource* pSrcResource);

        void UpdateSubresource(ID3D11Resource* pDstResource, uint DstSubresource, [NativeTypeName("const D3D11_BOX *")] D3D11_BOX* pDstBox, [NativeTypeName("const void *")] void* pSrcData, uint SrcRowPitch, uint SrcDepthPitch);

    }
}