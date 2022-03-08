using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX;

public unsafe partial struct ID3D11Resource : ID3D11Resource.Interface
{
    public void** lpVtbl;

    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, Guid*, void**, int>)(lpVtbl[0]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this), riid, ppvObject);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public uint AddRef()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, uint>)(lpVtbl[1]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public uint Release()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, uint>)(lpVtbl[2]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this));
    }

    public void GetDevice(ID3D11Device** ppDevice)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11Resource*, ID3D11Device**, void>)(lpVtbl[3]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this), ppDevice);
    }

    public HRESULT GetPrivateData(Guid* guid, uint* pDataSize, void* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, Guid*, uint*, void*, int>)(lpVtbl[4]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this), guid, pDataSize, pData);
    }

    public HRESULT SetPrivateData(Guid* guid, uint DataSize,
        void* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, Guid*, uint, void*, int>)(lpVtbl[5]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this), guid, DataSize, pData);
    }

    public HRESULT SetPrivateDataInterface(Guid* guid,
        IUnknown* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, Guid*, IUnknown*, int>)(lpVtbl[6]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this), guid, pData);
    }

    //public void GetType(D3D11_RESOURCE_DIMENSION* pResourceDimension)
    //{
    //    ((delegate* unmanaged[Stdcall]<ID3D11Resource*, D3D11_RESOURCE_DIMENSION*, void>)(lpVtbl[7]))(
    //        (ID3D11Resource*)Unsafe.AsPointer(ref this), pResourceDimension);
    //}

    public void SetEvictionPriority(uint EvictionPriority)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11Resource*, uint, void>)(lpVtbl[8]))((ID3D11Resource*)Unsafe.AsPointer(ref this),
            EvictionPriority);
    }

    public uint GetEvictionPriority()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Resource*, uint>)(lpVtbl[9]))(
            (ID3D11Resource*)Unsafe.AsPointer(ref this));
    }

    public interface Interface : ID3D11DeviceChild.Interface
    {
        //void GetType(D3D11_RESOURCE_DIMENSION* pResourceDimension);

        void SetEvictionPriority(uint EvictionPriority);

        uint GetEvictionPriority();
    }
}