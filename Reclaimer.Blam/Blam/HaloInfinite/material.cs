using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class material
    {
        [Offset(64)]
        public BlockCollection<PostprocessDefinitionBlock> PostprocessDefinitions { get; set; }
    }

    [FixedSize(160)]
    public class PostprocessDefinitionBlock
    {
        [Offset(0)]
        public BlockCollection<TexturesBlock> Textures { get; set; }
    }

    [FixedSize(56)]
    [DebuggerDisplay($"{{{nameof(BitmapReference)},nq}}")]
    public class TexturesBlock
    {
        [Offset(0)]
        public TagReference BitmapReference { get; set; }
    }
}