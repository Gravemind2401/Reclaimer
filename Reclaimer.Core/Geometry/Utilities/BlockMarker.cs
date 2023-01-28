using System.IO;

namespace Reclaimer.Geometry.Utilities
{
    internal sealed class BlockMarker : IDisposable
    {
        private readonly BinaryWriter writer;
        private readonly int pointerAddress;

        private bool isDisposed;

        public BlockMarker(BinaryWriter writer, int identifier)
        {
            this.writer = writer;

            writer.Write(identifier);
            pointerAddress = (int)writer.BaseStream.Position;
            writer.Write(0);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            var endOf = (int)writer.BaseStream.Position;
            writer.Seek(pointerAddress, SeekOrigin.Begin);
            writer.Write(endOf);
            writer.Seek(endOf, SeekOrigin.Begin);

            isDisposed = true;
        }
    }
}
