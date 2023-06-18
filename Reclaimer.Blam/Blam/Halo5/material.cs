using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public class material
    {
        [Offset(76)]
        public BlockCollection<PostprocessDefinitionBlock> PostprocessDefinitions { get; set; }
    }

    [FixedSize(198)]
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
