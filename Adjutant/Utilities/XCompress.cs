using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public static class XCompress
    {
        public enum XMemCodecType
        {
            Default = 0,
            LZX = 1
        }

        public struct XMemCodecParametersLZX
        {
            public int Flags;
            public int WindowSize;
            public int CompressionPartitionSize;
        }

        [DllImport("xcompress.dll", EntryPoint = "XMemCreateDecompressionContext")]
        public static extern int XMemCreateDecompressionContext(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        public static extern void XMemDestroyDecompressionContext(int context);

        [DllImport("xcompress.dll", EntryPoint = "XMemResetDecompressionContext")]
        public static extern int XMemResetDecompressionContext(int context);

        [DllImport("xcompress.dll", EntryPoint = "XMemDecompressStream")]
        public static extern int XMemDecompressStream(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);
    }
}
