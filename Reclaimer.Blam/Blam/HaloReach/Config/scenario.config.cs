using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.HaloReach
{
    [StructureDefinition<scenario, DefinitionBuilder>]
    public partial class scenario
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.HaloReachBeta);
                builder.Property(x => x.StructureBsps).HasOffset(68);
                builder.Property(x => x.Skies).HasOffset(124);
                builder.Property(x => x.ObjectNames).HasOffset(252);
                builder.Property(x => x.Scenery).HasOffset(264);
                builder.Property(x => x.SceneryPalette).HasOffset(276);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1828);

                builder = AddVersion(CacheType.HaloReachRetail);
                builder.Property(x => x.StructureBsps).HasOffset(76);
                builder.Property(x => x.Skies).HasOffset(132);
                builder.Property(x => x.ObjectNames).HasOffset(260);
                builder.Property(x => x.Scenery).HasOffset(272);
                builder.Property(x => x.SceneryPalette).HasOffset(284);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1844);

                builder = AddVersion(CacheType.MccHaloReach, null);
                builder.Property(x => x.StructureBsps).HasOffset(76);
                builder.Property(x => x.Skies).HasOffset(132);
                builder.Property(x => x.ObjectNames).HasOffset(260);
                builder.Property(x => x.Scenery).HasOffset(272);
                builder.Property(x => x.SceneryPalette).HasOffset(284);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1856);

                builder = AddVersion(CacheType.MccHaloReachU13, null);
                builder.Property(x => x.StructureBsps).HasOffset(80);
                builder.Property(x => x.Skies).HasOffset(136);
                builder.Property(x => x.ObjectNames).HasOffset(240);
                builder.Property(x => x.Scenery).HasOffset(252);
                builder.Property(x => x.SceneryPalette).HasOffset(264);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1800);
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
                var builder = AddDefaultVersion().HasFixedSize(172);
                builder.Property(x => x.BspReference).HasOffset(0);
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
                var builder = AddDefaultVersion().HasFixedSize(48);
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
                builder.Property(x => x.Position).HasOffset(8);
                builder.Property(x => x.Rotation).HasOffset(20);
                builder.Property(x => x.Scale).HasOffset(32);
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
                var builder = AddDefaultVersion().HasFixedSize(220);
                Common(builder);
                builder.Property(x => x.VariantName).HasOffset(88);
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
                var builder = AddDefaultVersion().HasFixedSize(16);
                builder.Property(x => x.TagReference).HasOffset(0);
            }
        }
    }
}
