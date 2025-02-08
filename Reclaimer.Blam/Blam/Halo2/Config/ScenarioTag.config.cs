using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo2
{
    [StructureDefinition<ScenarioTag, DefinitionBuilder>]
    public partial class ScenarioTag
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ScenarioTag>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.Halo2E3, CacheType.Halo2Xbox);
                builder.Property(x => x.StructureBsps).HasOffset(828);

                builder = AddVersion(CacheType.Halo2Xbox, null);
                builder.Property(x => x.Skies).HasOffset(8);
                builder.Property(x => x.ObjectNames).HasOffset(72);
                builder.Property(x => x.Scenery).HasOffset(80);
                builder.Property(x => x.SceneryPalette).HasOffset(88);
                builder.Property(x => x.Machines).HasOffset(168);
                builder.Property(x => x.MachinePalette).HasOffset(176);
                builder.Property(x => x.Controls).HasOffset(184);
                builder.Property(x => x.ControlPalette).HasOffset(192);
                builder.Property(x => x.StructureBsps).HasOffset(528);
                builder.Property(x => x.Crates).HasOffset(808);
                builder.Property(x => x.CratePalette).HasOffset(816);
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
                var builder = AddDefaultVersion().HasFixedSize(8);
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
            private static void Common(VersionBuilder builder)
            {
                builder.Property(x => x.PaletteIndex).HasOffset(0);
                builder.Property(x => x.NameIndex).HasOffset(2);
                builder.Property(x => x.Position).HasOffset(8);
                builder.Property(x => x.Rotation).HasOffset(20);
                builder.Property(x => x.Scale).HasOffset(32);
            }

            protected void Halo2Xbox(VersionBuilder builder)
            {
                Common(builder);
            }
        }
    }

    public partial class ObjectPlacementBlockBase
    {
        new protected abstract class DefinitionBuilderBase<TBlock> : PlacementBlockBase.DefinitionBuilderBase<TBlock>
            where TBlock : ObjectPlacementBlockBase
        {
            new protected void Halo2Xbox(VersionBuilder builder)
            {
                base.Halo2Xbox(builder);
                builder.Property(x => x.VariantName).HasOffset(52);
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
                var builder = AddDefaultVersion().HasFixedSize(40);
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
                var builder = AddDefaultVersion().HasFixedSize(92);
                Halo2Xbox(builder);
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
                var builder = AddDefaultVersion().HasFixedSize(72);
                Halo2Xbox(builder);
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
                var builder = AddDefaultVersion().HasFixedSize(68);
                Halo2Xbox(builder);
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
                var builder = AddVersion(CacheType.Halo2E3).HasFixedSize(80);
                builder.Property(x => x.MetadataAddress).HasOffset(0);
                builder.Property(x => x.Size).HasOffset(4);
                builder.Property(x => x.Magic).HasOffset(8);
                builder.Property(x => x.BspReference).HasOffset(16);

                builder = AddVersion(CacheType.Halo2Beta).HasFixedSize(84);
                builder.Property(x => x.MetadataAddress).HasOffset(0);
                builder.Property(x => x.Size).HasOffset(4);
                builder.Property(x => x.Magic).HasOffset(8);
                builder.Property(x => x.BspReference).HasOffset(16);

                builder = AddVersion(CacheType.Halo2Xbox, null).HasFixedSize(68);
                builder.Property(x => x.MetadataAddress).HasOffset(0);
                builder.Property(x => x.Size).HasOffset(4);
                builder.Property(x => x.Magic).HasOffset(8);
                builder.Property(x => x.BspReference).HasOffset(16);
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
                var builder = AddDefaultVersion().HasFixedSize(76);
                Halo2Xbox(builder);
            }
        }
    }
}
