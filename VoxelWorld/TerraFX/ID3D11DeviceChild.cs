using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX;

public unsafe partial struct ID3D11DeviceChild : ID3D11DeviceChild.Interface
{
    public void** lpVtbl;

    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, Guid*, void**, int>)(lpVtbl[0]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this), riid, ppvObject);
    }

    public uint AddRef()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, uint>)(lpVtbl[1]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this));
    }

    public uint Release()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, uint>)(lpVtbl[2]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this));
    }

    public void GetDevice(ID3D11Device** ppDevice)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, ID3D11Device**, void>)(lpVtbl[3]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this), ppDevice);
    }

    public HRESULT GetPrivateData(Guid* guid, uint* pDataSize, void* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, Guid*, uint*, void*, int>)(lpVtbl[4]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this), guid, pDataSize, pData);
    }

    public HRESULT SetPrivateData(Guid* guid, uint DataSize,
        void* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, Guid*, uint, void*, int>)(lpVtbl[5]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this), guid, DataSize, pData);
    }

    public HRESULT SetPrivateDataInterface(Guid* guid,
        IUnknown* pData)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11DeviceChild*, Guid*, IUnknown*, int>)(lpVtbl[6]))(
            (ID3D11DeviceChild*)Unsafe.AsPointer(ref this), guid, pData);
    }

    public interface Interface : IUnknown.Interface
    {
        void GetDevice(ID3D11Device** ppDevice);

        HRESULT GetPrivateData(Guid* guid, uint* pDataSize, void* pData);

        HRESULT SetPrivateData(Guid* guid, uint DataSize,
            void* pData);

        HRESULT SetPrivateDataInterface(Guid* guid,
            IUnknown* pData);
    }
}