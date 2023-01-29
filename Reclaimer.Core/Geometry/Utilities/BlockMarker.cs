using System.IO;

namespace Reclaimer.Geometry.Utilities
{
    internal sealed class BlockMarker : IDisposable
    {
        private readonly BinaryWriter writer;
        private readonly int pointerAddress;

        private bool isDisposed;

        public BlockMarker(BinaryWriter writer, BlockCode code)
        {
            this.writer = writer;

            writer.Write(code.Value);
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

    internal sealed class ListBlockMarker : IDisposable
    {
        private readonly BinaryWriter writer;
        private readonly int pointerAddress;

        private bool isDisposed;

        public ListBlockMarker(BinaryWriter writer, BlockCode code, int count)
        {
            this.writer = writer;

            writer.Write(SceneCodes.List.Value);
            writer.Write(code.Value);
            pointerAddress = (int)writer.BaseStream.Position;
            writer.Write(0);
            writer.Write(count);
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
