using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using System.Buffers;
using System.Runtime.InteropServices;

//https://github.com/Connor-jt/H5_bitmap_exporter/blob/bd49da34bf9de7ede90912a895d975a8c2de0606/detiling/recovered_structs.h
//https://github.com/Connor-jt/H5_bitmap_exporter/blob/bd49da34bf9de7ede90912a895d975a8c2de0606/detiling/DirectXTexXboxDetile.cpp

//The structs here are just a direct C# port from the links above

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Reclaimer.Utilities
{
    public static class XG
    {
        #region Native Structures

        private enum XG_TILE_MODE : uint
        {
            XG_TILE_MODE_INVALID = 0,
            XG_TILE_MODE_LINEAR = 1,
        }

        private enum XG_PLANE_USAGE : uint
        {
            XG_PLANE_USAGE_UNUSED = 0,
            XG_PLANE_USAGE_DEFAULT = 1,
            XG_PLANE_USAGE_COLOR_MASK = 2,
            XG_PLANE_USAGE_FRAGMENT_MASK = 3,
            XG_PLANE_USAGE_HTILE = 4,
            XG_PLANE_USAGE_LUMA = 5,
            XG_PLANE_USAGE_CHROMA = 6,
            XG_PLANE_USAGE_DEPTH = 7,
            XG_PLANE_USAGE_STENCIL = 8,
            XG_PLANE_USAGE_DELTA_COLOR_COMPRESSION = 9,
        }

        private enum XG_RESOURCE_DIMENSION : uint
        {   // "Subset here matches D3D10_RESOURCE_DIMENSION and D3D11_RESOURCE_DIMENSION"
            XG_RESOURCE_DIMENSION_TEXTURE1D = 2,
            XG_RESOURCE_DIMENSION_TEXTURE2D = 3,
            XG_RESOURCE_DIMENSION_TEXTURE3D = 4,
        }

        private struct XG_Mipmap
        { // sizeof = 0x60
            public ulong SizeBytes; // 0x8
            public ulong OffsetBytes; // 0x10
            public ulong Slice2DSizeBytes; // 0x18
            public uint PitchPixels; // 0x1C
            public uint PitchBytes; // 0x20
            public uint AlignmentBytes; // 0x24
            public uint PaddedWidthElements; // 0x28
            public uint PaddedHeightElements; // 0x2C
            public uint PaddedDepthOrArraySize; // 0x30
            public uint WidthElements; // 0x34
            public uint HeightElements; // 0x38
            public uint DepthOrArraySize; // 0x3C
            public uint SampleCount; // 0x40
            public XG_TILE_MODE TileMode; // 0x44
            public int Padding1; // 0x48
            public ulong BankRotationAddressBitMask; // 0x50
            public ulong BankRotationBytesPerSlice; // 0x58
            public uint SliceDepthElements; // 0x5C
            public int Padding2; // 0x60
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XG_PLANE_LAYOUT
        { // sizeof = 5C8
            public XG_PLANE_USAGE Usage; // 0x4
            public int Padding1; // 0x8
            public ulong SizeBytes; // 0x10
            public ulong BaseOffsetBytes; // 0x18
            public ulong BaseAlignmentBytes; // 0x20
            public uint BytesPerElement; // 0x24
            public int Padding2; // 0x28

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
            public XG_Mipmap[] MipLayout; // sizeof = 5A0

            public XG_PLANE_LAYOUT()
            {
                MipLayout = new XG_Mipmap[15];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XG_RESOURCE_LAYOUT
        {
            public ulong SizeBytes; // 0x8
            public ulong BaseAlignmentBytes; // 0x10
            public uint MipLevels; // 0x14
            public uint Planes; // 0x18

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public XG_PLANE_LAYOUT[] Plane; // 0x1738 // sizeof = 0x1720

            public XG_RESOURCE_DIMENSION Dimension; // 0x173C

            public int Padding;
        }

        private enum XG_USAGE
        {
            XG_USAGE_DEFAULT = 0,
            XG_USAGE_IMMUTABLE = 1,
            XG_USAGE_DYNAMIC = 2,
            XG_USAGE_STAGING = 3
        }

        private enum XG_CPU_ACCESS_FLAG : uint
        {
            XG_CPU_ACCESS_WRITE = 0x10000u,
            XG_CPU_ACCESS_READ = 0x20000u
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XG_SAMPLE_DESC
        {
            public uint Count;
            public uint Quality;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XG_TEXTURE1D_DESC
        {
            public uint Width;
            public uint MipLevels;
            public uint ArraySize;
            public uint Format; // XG_FORMAT (maybe same as DXGI_FORMAT?)
            public XG_USAGE Usage;
            public uint BindFlags; // XG_BIND_FLAG
            public uint MiscFlags; // XG_RESOURCE_MISC_FLAG
            public uint ESRAMOffsetBytes;
            public uint ESRAMUsageBytes;
            public XG_TILE_MODE TileMode;
            public uint Pitch;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XG_TEXTURE2D_DESC
        {
            public uint Width;
            public uint Height;
            public uint MipLevels;
            public uint ArraySize;
            public uint Format; // XG_FORMAT (maybe same as DXGI_FORMAT?)
            public XG_SAMPLE_DESC SampleDesc;
            public XG_USAGE Usage;
            public uint BindFlags; // XG_BIND_FLAG
            public XG_CPU_ACCESS_FLAG CPUAccessFlags;
            public uint MiscFlags; // XG_RESOURCE_MISC_FLAG
            public uint ESRAMOffsetBytes; // im pretty sure these are only for the ESRAM example
            public uint ESRAMUsageBytes;
            public XG_TILE_MODE TileMode;
            public uint Pitch;
        }

        #endregion

        [DllImport("xg.dll", EntryPoint = "XGCreateTexture1DComputer")]
        private static extern int XGCreateTexture1DComputer(ref XG_TEXTURE1D_DESC texdata, ref IntPtr computer);

        [DllImport("xg.dll", EntryPoint = "XGCreateTexture2DComputer")]
        private static extern int XGCreateTexture2DComputer(ref XG_TEXTURE2D_DESC texdata, ref IntPtr computer);

        public static void Detile2D(BitmapProperties props, byte[] data)
        {
            const uint XG_BIND_SHADER_RESOURCE = 8u;
            const uint XG_RESOURCE_MISC_TEXTURECUBE = 4u;

            if (!Environment.Is64BitProcess)
                return;

            var descriptor = props.CreateFormatDescriptor();

            var desc = new XG_TEXTURE2D_DESC
            {
                Width = (uint)descriptor.Width,
                Height = (uint)descriptor.Height,
                MipLevels = (uint)descriptor.MipmapCount + 1,
                ArraySize = (uint)descriptor.FrameCount,
                Format = (uint)descriptor.DxgiFormat,
                SampleDesc = new XG_SAMPLE_DESC { Count = 1 },
                Usage = XG_USAGE.XG_USAGE_DEFAULT,
                BindFlags = XG_BIND_SHADER_RESOURCE,
                MiscFlags = props.BitmapType?.ToString().ToLower() == "cubemap" ? XG_RESOURCE_MISC_TEXTURECUBE : 0,
                TileMode = (XG_TILE_MODE)props.X1TileMode
            };

            IntPtr computerPtr = default;

            var hresult = XGCreateTexture2DComputer(ref desc, ref computerPtr);
            if (hresult != 0)
                throw new InvalidOperationException($"Unexpected HRESULT from {nameof(XGCreateTexture2DComputer)}");

            var computer = new XGTextureAddressComputer(computerPtr);

            try
            {
                var layout = new XG_RESOURCE_LAYOUT();
                hresult = computer.GetResourceLayout(ref layout);

                var success = Detile2D(descriptor.DxgiFormat, 0, computer, layout, data);
            }
            finally
            {
                computer.Release();
            }
        }

        //https://github.com/Connor-jt/H5_bitmap_exporter/blob/bd49da34bf9de7ede90912a895d975a8c2de0606/detiling/DirectXTexXboxDetile.cpp#L316
        private static bool Detile2D(DxgiFormat format, int mipLevel, XGTextureAddressComputer computer, XG_RESOURCE_LAYOUT layout, byte[] data)
        {
            //the code referenced in the link above has different logic depending on the format, but everything seems to be working so far with the same method?

            //var formatString = format.ToString();
            //var isBlockFormat = formatString.StartsWith("BC");
            //var isPacked = false; //the definition for this was external
            //var isTypless = formatString.EndsWith("Typeless");

            var mip = layout.Plane[0].MipLayout[mipLevel];

            //if (isBlockFormat)
            {
                var xBlocks = (int)mip.WidthElements;
                var yBlocks = (int)mip.HeightElements;

                var blockSizeBytes = (int)(mip.PitchBytes / mip.PaddedWidthElements);
                return DetileByElement2D(mipLevel, computer, data, blockSizeBytes, xBlocks, yBlocks, false);
            }
            //else if (isPacked)
            //{

            //}
            //else if (isTypless)
            //{

            //}
            //else
            //{

            //}

            //return false;
        }

        private static bool DetileByElement2D(int mipLevel, XGTextureAddressComputer computer, byte[] data, int texelPitch, int xBlocks, int yBlocks, bool packed)
        {
            using var output = MemoryPool<byte>.Shared.Rent(data.Length);

            var outputSpan = output.Memory.Span;
            var arrayIndex = 0u; //TODO: doesnt appear to be needed since reclaimer clips the data to the specific array index before detiling
            var xStride = packed ? 2 : 1;

            for (var y = 0; y < yBlocks; y++)
            {
                for (var x = 0; x < xBlocks; x += xStride)
                {
                    var sourceOffset = (int)computer.GetTexelElementOffsetBytes(0, (uint)mipLevel, (ulong)x, (uint)y, arrayIndex, 0);
                    if (sourceOffset < 0)
                        return false;

                    var destIndex = y * xBlocks * texelPitch + x * texelPitch;
                    data.AsSpan(sourceOffset, texelPitch).CopyTo(outputSpan.Slice(destIndex, texelPitch));
                }
            }

            output.Memory[..data.Length].CopyTo(data);

            return true;
        }

        private class XGTextureAddressComputer
        {
            //This class mimics the native XGTextureAddressComputer class.
            //It expects the XG DLL's vtable layout to be in the same order as the methods below and with the exact same signatures.
            //Therefore it may only work with certain versions of the DLL.

            private delegate uint AddRefDelegate(IntPtr instancePtr);
            private delegate uint ReleaseDelegate(IntPtr instancePtr);
            private delegate int GetResourceLayoutDelegate(IntPtr instancePtr, ref XG_RESOURCE_LAYOUT layout);
            private delegate ulong GetResourceSizeBytesDelegate(IntPtr instancePtr);
            private delegate ulong GetResourceBaseAlignmentBytesDelegate(IntPtr instancePtr);
            private delegate ulong GetMipLevelOffsetBytesDelegate(IntPtr instancePtr, uint param1, uint param2);
            private delegate ulong GetTexelElementOffsetBytesDelegate(IntPtr instancePtr, uint plane, uint mipLevel, ulong x, uint y, uint zOrSlice, uint sample);
            private delegate int GetTexelCoordinateDelegate(IntPtr instancePtr, ulong param1, ref uint out_texture_index, ref uint param3, ref ulong param4, ref uint param5, ref uint param6, ref uint param7);
            //private delegate int CopyIntoSubresourceDelegate(IntPtr instancePtr, ref void param1, uint param2, uint param3, ref void param4, uint param5, uint param6);
            //private delegate int CopyFromSubresourceDelegate(IntPtr instancePtr, ref void param1, uint param2, uint param3, ref void param4, uint param5, uint param6);
            //private delegate int GetResourceTilingDelegate(IntPtr instancePtr, ref uint param1, ref XG_PACKED_MIP_DESC param2, ref XG_TILE_SHAPE param3, ref uint param4, uint param5, ref XG_SUBRESOURCE_TILING param6);
            //private delegate int GetTextureViewDescriptorDelegate(IntPtr instancePtr, uint planeIndex, ref XG_DESCRIPTOR_TEXTURE_VIEW texView);

            private readonly IntPtr computerPtr;

            private readonly AddRefDelegate addRef;
            private readonly ReleaseDelegate release;
            private readonly GetResourceLayoutDelegate getResourceLayout;
            private readonly GetResourceSizeBytesDelegate getResourceSizeBytes;
            private readonly GetResourceBaseAlignmentBytesDelegate getResourceBaseAlignmentBytes;
            private readonly GetMipLevelOffsetBytesDelegate getMipLevelOffsetBytes;
            private readonly GetTexelElementOffsetBytesDelegate getTexelElementOffsetBytes;
            private readonly GetTexelCoordinateDelegate getTexelCoordinate;
            //private readonly CopyIntoSubresourceDelegate copyIntoSubresource;
            //private readonly CopyFromSubresourceDelegate copyFromSubresource;
            //private readonly GetResourceTilingDelegate getResourceTiling;
            //private readonly GetTextureViewDescriptorDelegate getTextureViewDescriptor;

            public XGTextureAddressComputer(IntPtr computerPtr)
            {
                var vtablePtr = Marshal.ReadIntPtr(computerPtr);

                addRef = GetVTableMethod<AddRefDelegate>(0);
                release = GetVTableMethod<ReleaseDelegate>(1);
                getResourceLayout = GetVTableMethod<GetResourceLayoutDelegate>(2);
                getResourceSizeBytes = GetVTableMethod<GetResourceSizeBytesDelegate>(3);
                getResourceBaseAlignmentBytes = GetVTableMethod<GetResourceBaseAlignmentBytesDelegate>(4);
                getMipLevelOffsetBytes = GetVTableMethod<GetMipLevelOffsetBytesDelegate>(5);
                getTexelElementOffsetBytes = GetVTableMethod<GetTexelElementOffsetBytesDelegate>(6);
                getTexelCoordinate = GetVTableMethod<GetTexelCoordinateDelegate>(7);
                //copyIntoSubresource = GetVTableMethod<CopyIntoSubresourceDelegate>(8);
                //copyFromSubresource = GetVTableMethod<CopyFromSubresourceDelegate>(9);
                //getResourceTiling = GetVTableMethod<GetResourceTilingDelegate>(10);
                //getTextureViewDescriptor = GetVTableMethod<GetTextureViewDescriptorDelegate>(11);


                TDelegate GetVTableMethod<TDelegate>(int methodIndex)
                    where TDelegate : Delegate
                {
                    var methodPtr = Marshal.ReadIntPtr(vtablePtr, methodIndex * IntPtr.Size);
                    return (TDelegate)Marshal.GetDelegateForFunctionPointer(methodPtr, typeof(TDelegate));
                }

                this.computerPtr = computerPtr;
            }

            public uint AddRef()
                => addRef(computerPtr);

            public uint Release()
                => release(computerPtr);

            public int GetResourceLayout(ref XG_RESOURCE_LAYOUT layout)
                => getResourceLayout(computerPtr, ref layout);

            public ulong GetResourceSizeBytes()
                => getResourceSizeBytes(computerPtr);

            public ulong GetResourceBaseAlignmentBytes()
                => getResourceBaseAlignmentBytes(computerPtr);

            public ulong GetMipLevelOffsetBytes(uint param1, uint param2)
                => getMipLevelOffsetBytes(computerPtr, param1, param2);

            public ulong GetTexelElementOffsetBytes(uint plane, uint mipLevel, ulong x, uint y, uint zOrSlice, uint sample)
                => getTexelElementOffsetBytes(computerPtr, plane, mipLevel, x, y, zOrSlice, sample);

            public int GetTexelCoordinate(ulong param1, ref uint out_texture_index, ref uint param3, ref ulong param4, ref uint param5, ref uint param6, ref uint param7)
                => getTexelCoordinate(computerPtr, param1, ref out_texture_index, ref param3, ref param4, ref param5, ref param6, ref param7);

            //public int CopyIntoSubresource(ref void param1, uint param2, uint param3, ref void param4, uint param5, uint param6)
            //    => copyIntoSubresource(computerPtr, ref param1, param2, param3, ref param4, param5, param6);

            //public int CopyFromSubresource(ref void param1, uint param2, uint param3, ref void param4, uint param5, uint param6)
            //    => copyFromSubresource(computerPtr, ref param1, param2, param3, ref param4, param5, param6);

            //public int GetResourceTiling(ref uint param1, ref XG_PACKED_MIP_DESC param2, ref XG_TILE_SHAPE param3, ref uint param4, uint param5, ref XG_SUBRESOURCE_TILING param6)
            //    => getResourceTiling(computerPtr, ref param1, ref param2, ref param3, ref param4, param5, ref param6);

            //public int GetTextureViewDescriptor(uint planeIndex, ref XG_DESCRIPTOR_TEXTURE_VIEW texView)
            //    => getTextureViewDescriptor(computerPtr, planeIndex, ref TexView);
        }
    }
}
