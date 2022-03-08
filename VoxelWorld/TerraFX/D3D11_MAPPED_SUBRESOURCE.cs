namespace TerraFX.Interop.DirectX;

public unsafe partial struct D3D11_MAPPED_SUBRESOURCE
{
    public void* pData;

    public uint RowPitch;

    public uint DepthPitch;
}