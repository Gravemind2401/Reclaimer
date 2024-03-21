using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo1
{
    [StructureDefinition<scenario, DefinitionBuilder>]
    public partial class scenario
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion();
                builder.Property(x => x.Skies).HasOffset(48);
                builder.Property(x => x.ObjectNames).HasOffset(516);
                builder.Property(x => x.Scenery).HasOffset(528);
                builder.Property(x => x.SceneryPalette).HasOffset(540);
                builder.Property(x => x.Machines).HasOffset(660);
                builder.Property(x => x.MachinePalette).HasOffset(672);
                builder.Property(x => x.Controls).HasOffset(684);
                builder.Property(x => x.ControlPalette).HasOffset(696);
                builder.Property(x => x.StructureBsps).HasOffset(1444);
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
                var builder = AddDefaultVersion().HasFixedSize(16);
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
                var builder = AddDefaultVersion().HasFixedSize(36);
                builder.Property(x => x.Name).HasOffset(0).IsNullTerminated(32);
                builder.Property(x => x.ObjectType).HasOffset(32);
                builder.Property(x => x.PlacementIndex).HasOffset(34);
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
                builder.Property(x => x.Position).HasOffset(8);
                builder.Property(x => x.Rotation).HasOffset(20);
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
                var builder = AddDefaultVersion().HasFixedSize(48);
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
                var builder = AddDefaultVersion().HasFixedSize(72);
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
                var builder = AddDefaultVersion().HasFixedSize(64);
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
                var builder = AddDefaultVersion().HasFixedSize(64);
                Common(builder);
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
                var builder = AddDefaultVersion().HasFixedSize(32);
                builder.Property(x => x.MetadataAddress).HasOffset(0);
                builder.Property(x => x.Size).HasOffset(4);
                builder.Property(x => x.Magic).HasOffset(8);
                builder.Property(x => x.BspReference).HasOffset(16);
            }
        }
    }
}
