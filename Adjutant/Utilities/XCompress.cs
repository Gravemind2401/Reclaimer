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
        private static bool Is64Bit => Environment.Is64BitProcess;

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

        public static int XMemCreateDecompressionContext(XMemCodecType codecType, int pCodecParams, int flags, ref int pContext)
        {
            if (Is64Bit)
                return XMemCreateDecompressionContext64(codecType, pCodecParams, flags, ref pContext);
            else
                return XMemCreateDecompressionContext32(codecType, pCodecParams, flags, ref pContext);
        }

        public static void XMemDestroyDecompressionContext(int context)
        {
            if (Is64Bit)
                XMemDestroyDecompressionContext64(context);
            else
                XMemDestroyDecompressionContext32(context);
        }

        public static int XMemResetDecompressionContext(int context)
        {
            if (Is64Bit)
                return XMemResetDecompressionContext64(context);
            else
                return XMemResetDecompressionContext32(context);
        }

        public static int XMemDecompressStream(int context, byte[] pDestination, ref int pDestSize, byte[] pSource, ref int pSrcSize)
        {
            if (Is64Bit)
                return XMemDecompressStream64(context, pDestination, ref pDestSize, pSource, ref pSrcSize);
            else
                return XMemDecompressStream32(context, pDestination, ref pDestSize, pSource, ref pSrcSize);
        }

        #region x86
        [DllImport("xcompress32.dll", EntryPoint = "XMemCreateDecompressionContext")]
        private static extern int XMemCreateDecompressionContext32(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern int XMemResetDecompressionContext32(int context);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDecompressStream")]
        private static extern int XMemDecompressStream32(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);
        #endregion

        #region x64
        [DllImport("xcompress64.dll", EntryPoint = "XMemCreateDecompressionContext")]
        private static extern int XMemCreateDecompressionContext64(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref int pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern int XMemResetDecompressionContext64(int context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDecompressStream")]
        private static extern int XMemDecompressStream64(int context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);
        #endregion
    }
}
