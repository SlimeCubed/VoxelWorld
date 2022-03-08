using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.DirectX;

[Guid("DB6F6DDB-AC77-4E88-8253-819DF9BBF140")]
public unsafe partial struct ID3D11Device : ID3D11Device.Interface
{
    public void** lpVtbl;
    
    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, Guid*, void**, int>)(lpVtbl[0]))(
            (ID3D11Device*)Unsafe.AsPointer(ref this), riid, ppvObject);
    }

    public uint AddRef()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, uint>)(lpVtbl[1]))((ID3D11Device*)Unsafe.AsPointer(ref this));
    }

    public uint Release()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, uint>)(lpVtbl[2]))((ID3D11Device*)Unsafe.AsPointer(ref this));
    }
    
    [VtblIndex(6)]
    public HRESULT CreateTexture3D([NativeTypeName("const D3D11_TEXTURE3D_DESC *")] D3D11_TEXTURE3D_DESC* pDesc, [NativeTypeName("const D3D11_SUBRESOURCE_DATA *")] D3D11_SUBRESOURCE_DATA* pInitialData, ID3D11Texture3D** ppTexture3D)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, D3D11_TEXTURE3D_DESC*, D3D11_SUBRESOURCE_DATA*, ID3D11Texture3D**, int>)(lpVtbl[6]))((ID3D11Device*)Unsafe.AsPointer(ref this), pDesc, pInitialData, ppTexture3D);
    }

    public HRESULT CreateDeferredContext(uint ContextFlags, ID3D11DeviceContext** ppDeferredContext)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, uint, ID3D11DeviceContext**, int>)(lpVtbl[27]))((ID3D11Device*)Unsafe.AsPointer(ref this), ContextFlags, ppDeferredContext);
    }
    
    public void GetImmediateContext(ID3D11DeviceContext** ppImmediateContext)
    {
        ((delegate* unmanaged[Stdcall]<ID3D11Device*, ID3D11DeviceContext**, void>)(lpVtbl[40]))((ID3D11Device*)Unsafe.AsPointer(ref this), ppImmediateContext);
    }
    
    
    [VtblIndex(33)]
    public HRESULT CheckFeatureSupport(D3D11_FEATURE Feature, void* pFeatureSupportData, uint FeatureSupportDataSize)
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, D3D11_FEATURE, void*, uint, int>)(lpVtbl[33]))((ID3D11Device*)Unsafe.AsPointer(ref this), Feature, pFeatureSupportData, FeatureSupportDataSize);
    }


    [VtblIndex(37)]
    public D3D_FEATURE_LEVEL GetFeatureLevel()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, D3D_FEATURE_LEVEL>)(lpVtbl[37]))((ID3D11Device*)Unsafe.AsPointer(ref this));
    }
    
    [VtblIndex(38)]
    public uint GetCreationFlags()
    {
        return ((delegate* unmanaged[Stdcall]<ID3D11Device*, uint>)(lpVtbl[38]))((ID3D11Device*)Unsafe.AsPointer(ref this));
    }
    
    public interface Interface : IUnknown.Interface
    {
        [VtblIndex(6)]
        HRESULT CreateTexture3D([NativeTypeName("const D3D11_TEXTURE3D_DESC *")] D3D11_TEXTURE3D_DESC* pDesc, [NativeTypeName("const D3D11_SUBRESOURCE_DATA *")] D3D11_SUBRESOURCE_DATA* pInitialData, ID3D11Texture3D** ppTexture3D);

        HRESULT CreateDeferredContext(uint ContextFlags, ID3D11DeviceContext** ppDeferredContext);

        void GetImmediateContext(ID3D11DeviceContext** ppImmediateContext);
        
        [VtblIndex(33)]
        HRESULT CheckFeatureSupport(D3D11_FEATURE Feature, void* pFeatureSupportData, uint FeatureSupportDataSize);

        [VtblIndex(37)]
        D3D_FEATURE_LEVEL GetFeatureLevel();
        
        
        [VtblIndex(38)]
        uint GetCreationFlags();


    }
}