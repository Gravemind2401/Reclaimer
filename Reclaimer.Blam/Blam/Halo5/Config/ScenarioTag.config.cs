using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo5
{
    [StructureDefinition<ScenarioTag, DefinitionBuilder>]
    public partial class ScenarioTag
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ScenarioTag>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion();
                builder.Property(x => x.StructureBsps).HasOffset(380);
                builder.Property(x => x.Skies).HasOffset(500);
                builder.Property(x => x.ObjectNames).HasOffset(780);
                builder.Property(x => x.Scenery).HasOffset(808);
                builder.Property(x => x.SceneryPalette).HasOffset(836);
                builder.Property(x => x.Machines).HasOffset(1116);
                builder.Property(x => x.MachinePalette).HasOffset(1144);
                builder.Property(x => x.Controls).HasOffset(1228);
                builder.Property(x => x.ControlPalette).HasOffset(1256);
                builder.Property(x => x.Crates).HasOffset(3416);
                builder.Property(x => x.CratePalette).HasOffset(3444);
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

    [StructureDefinition<ObjectNameBlock, DefinitionBuilder>]
    public partial class ObjectNameBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ObjectNameBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(8);
                builder.Property(x => x.Name).HasOffset(0);
                builder.Property(x => x.ObjectType).HasOffset(4);
                builder.Property(x => x.PlacementIndex).HasOffset(6);
            }
        }
    }

    public partial class PlacementBlockBase
    {
        protected abstract class DefinitionBuilderBase<TBlock> : DefinitionBuilder<TBlock>
            where TBlock : PlacementBlockBase
        {
            protected static void Common(VersionBuilder builder)
            {
                builder.Property(x => x.PaletteIndex).HasOffset(0);
                builder.Property(x => x.NameIndex).HasOffset(2);
                builder.Property(x => x.Position).HasOffset(12);
                builder.Property(x => x.Rotation).HasOffset(24);
                builder.Property(x => x.Scale).HasOffset(36);
            }
        }
    }

    public partial class ObjectPlacementBlockBase
    {
        new protected abstract class DefinitionBuilderBase<TBlock> : PlacementBlockBase.DefinitionBuilderBase<TBlock>
            where TBlock : ObjectPlacementBlockBase
        {
            new protected static void Common(VersionBuilder builder)
            {
                PlacementBlockBase.DefinitionBuilderBase<TBlock>.Common(builder);
                builder.Property(x => x.VariantName).HasOffset(300);
            }
        }
    }

    [StructureDefinition<ObjectPaletteBlock, DefinitionBuilder>]
    public partial class ObjectPaletteBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ObjectPaletteBlock>
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
        private sealed class DefinitionBuilder : DefinitionBuilderBase<SceneryPlacementBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(720);
                Common(builder);
            }
        }
    }

    [StructureDefinition<MachinePlacementBlock, DefinitionBuilder>]
    public partial class MachinePlacementBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilderBase<MachinePlacementBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(732);
                Common(builder);
            }
        }
    }

    [StructureDefinition<ControlPlacementBlock, DefinitionBuilder>]
    public partial class ControlPlacementBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilderBase<ControlPlacementBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(736);
                Common(builder);
            }
        }
    }

    [StructureDefinition<CratePlacementBlock, DefinitionBuilder>]
    public partial class CratePlacementBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilderBase<CratePlacementBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(716);
                Common(builder);
            }
        }
    }
}
