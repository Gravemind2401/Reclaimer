using System.IO;

namespace Reclaimer.Geometry.Utilities
{
    /// <summary>
    /// A disposable object used for writing block headers.
    /// </summary>
    /// <remarks>
    /// The block header will be written immediately when the <see cref="BlockMarker"/> is initialized.
    /// <br/> When disposed, the 'end-of-block' pointer will be filled in automatically with the current stream position.
    /// </remarks>
    internal class BlockMarker : IDisposable
    {
        private readonly BinaryWriter writer;
        private readonly int pointerAddress;

        private bool isDisposed;

        public BlockMarker(BinaryWriter writer, BlockCode code)
        {
            this.writer = writer;

            writer.Write(code.Value);
            pointerAddress = (int)writer.BaseStream.Position;
            writer.Write(0); //dummy pointer that will get filled in when the object is disposed
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

    /// <remarks>
    /// The block header and element count will be written immediately when the <see cref="ListBlockMarker"/> is initialized.
    /// <br/> When disposed, the 'end-of-block' pointer will be filled in automatically with the current stream position.
    /// </remarks>
    /// <inheritdoc cref="BlockMarker"/>
    internal sealed class ListBlockMarker : BlockMarker
    {
        public ListBlockMarker(BinaryWriter writer, BlockCode code, int count)
            : base(writer, SceneCodes.List)
        {
            writer.Write(code.Value);
            writer.Write(count);
        }
    }
}
