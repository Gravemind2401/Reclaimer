using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo5
{
    [StructureDefinition<scenario, DefinitionBuilder>]
    public partial class scenario
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion();
                builder.Property(x => x.StructureBsps).HasOffset(380);
                builder.Property(x => x.Skies).HasOffset(500);
                builder.Property(x => x.Scenery).HasOffset(808);
                builder.Property(x => x.SceneryPalette).HasOffset(836);
            }
        }
    }

    [StructureDefinition<StructureBspBlock, DefinitionBuilder>]
    public partial class StructureBspBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<StructureBspBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(196);
                builder.Property(x => x.BspReference).HasOffset(0);
                builder.Property(x => x.LightingVariants).HasOffset(140);
            }
        }
    }

    [StructureDefinition<StructureLightingBlock, DefinitionBuilder>]
    public partial class StructureLightingBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<StructureLightingBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(140);
                builder.Property(x => x.StructureLightmapReference).HasOffset(36);
            }
        }
    }

    [StructureDefinition<SkyReferenceBlock, DefinitionBuilder>]
    public partial class SkyReferenceBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<SkyReferenceBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(44);
                builder.Property(x => x.SkyReference).HasOffset(0);
            }
        }
    }

    [StructureDefinition<SceneryPaletteBlock, DefinitionBuilder>]
    public partial class SceneryPaletteBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<SceneryPaletteBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(32);
                builder.Property(x => x.TagReference).HasOffset(0);
            }
        }
    }

    [StructureDefinition<SceneryPlacementBlock, DefinitionBuilder>]
    public partial class SceneryPlacementBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<SceneryPlacementBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(720);
                builder.Property(x => x.PaletteIndex).HasOffset(0);
                builder.Property(x => x.NameIndex).HasOffset(2);
                builder.Property(x => x.Position).HasOffset(12);
                builder.Property(x => x.Rotation).HasOffset(24);
                builder.Property(x => x.Scale).HasOffset(36);
            }
        }
    }
}
