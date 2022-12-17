using System.Runtime.InteropServices;

namespace Reclaimer.Blam.Utilities
{
    public static class XCompress
    {
        private static readonly bool Is64Bit = Environment.Is64BitProcess;

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

        public static long XMemCreateDecompressionContext(XMemCodecType codecType, int pCodecParams, int flags, ref long pContext)
        {
            if (Is64Bit)
                return XMemCreateDecompressionContext64(codecType, pCodecParams, flags, ref pContext);
            else
            {
                var context32 = (int)pContext;
                var result = XMemCreateDecompressionContext32(codecType, pCodecParams, flags, ref context32);
                pContext = context32;
                return result;
            }
        }

        public static void XMemDestroyDecompressionContext(long context)
        {
            if (Is64Bit)
                XMemDestroyDecompressionContext64(context);
            else
                XMemDestroyDecompressionContext32((int)context);
        }

        public static long XMemResetDecompressionContext(long context)
        {
            if (Is64Bit)
                return XMemResetDecompressionContext64(context);
            else
                return XMemResetDecompressionContext32((int)context);
        }

        public static long XMemDecompressStream(long context, byte[] pDestination, ref int pDestSize, byte[] pSource, ref int pSrcSize)
        {
            if (Is64Bit)
                return XMemDecompressStream64(context, pDestination, ref pDestSize, pSource, ref pSrcSize);
            else
                return XMemDecompressStream32((int)context, pDestination, ref pDestSize, pSource, ref pSrcSize);
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
        private static extern long XMemCreateDecompressionContext64(
            XMemCodecType codecType,
            int pCodecParams,
            int flags, ref long pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext64(long context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern long XMemResetDecompressionContext64(long context);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDecompressStream")]
        private static extern long XMemDecompressStream64(long context,
            byte[] pDestination, ref int pDestSize,
            byte[] pSource, ref int pSrcSize);
        #endregion
    }
}
