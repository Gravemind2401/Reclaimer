using System.Runtime.InteropServices;

namespace Reclaimer.Utilities
{
    internal static class Oodle
    {
        public const string OodleDllFileName = "oo2core_8_win64.dll";

        [DllImport(OodleDllFileName)]
        private static extern int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize,
            uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        public static bool DependencyExists()
        {
            try
            {
                Decompress(Array.Empty<byte>(), 0, 0);
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
        }

        public static byte[] Decompress(byte[] buffer, int size, int uncompressedSize)
        {
            var decompressedBuffer = new byte[uncompressedSize];
            var decompressedCount = OodleLZ_Decompress(buffer, size, decompressedBuffer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

            return decompressedCount <= uncompressedSize
                ? decompressedBuffer
                : throw new Exception("There was an error while decompressing");
        }
    }
}