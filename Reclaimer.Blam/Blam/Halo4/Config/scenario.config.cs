using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo4
{
    [StructureDefinition<scenario, DefinitionBuilder>]
    public partial class scenario
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario>
        {
            public DefinitionBuilder()
            {
                PreRelease();
                Halo4();
                Halo2X();
            }

            private void PreRelease()
            {
                var builder = AddVersion(CacheType.Halo4Beta);
                builder.Property(x => x.StructureBsps).HasOffset(152);
                builder.Property(x => x.Skies).HasOffset(208);
                builder.Property(x => x.ObjectNames).HasOffset(336);
                builder.Property(x => x.Scenery).HasOffset(348);
                builder.Property(x => x.SceneryPalette).HasOffset(360);
                builder.Property(x => x.Machines).HasOffset(480);
                builder.Property(x => x.MachinePalette).HasOffset(492);
                builder.Property(x => x.Controls).HasOffset(528);
                builder.Property(x => x.ControlPalette).HasOffset(540);
                builder.Property(x => x.Crates).HasOffset(1580);
                builder.Property(x => x.CratePalette).HasOffset(1592);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1884);
            }

            private void Halo4()
            {
                var builder = AddVersion(CacheType.Halo4Retail, null);
                builder.Property(x => x.StructureBsps).HasOffset(160);
                builder.Property(x => x.Skies).HasOffset(216);
                builder.Property(x => x.ObjectNames).HasOffset(344);
                builder.Property(x => x.Scenery).HasOffset(356);
                builder.Property(x => x.SceneryPalette).HasOffset(368);
                builder.Property(x => x.Machines).HasOffset(488);
                builder.Property(x => x.MachinePalette).HasOffset(500);
                builder.Property(x => x.Controls).HasOffset(536);
                builder.Property(x => x.ControlPalette).HasOffset(548);
                builder.Property(x => x.Crates).HasOffset(1616);
                builder.Property(x => x.CratePalette).HasOffset(1628);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1896);

                builder = AddVersion(CacheType.MccHalo4, null);
                builder.Property(x => x.StructureBsps).HasOffset(160);
                //builder.Property(x => x.Skies).HasOffset(???); //TODO
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Machines).HasOffset(???); //TODO
                //builder.Property(x => x.MachinePalette).HasOffset(???); //TODO
                //builder.Property(x => x.Controls).HasOffset(???); //TODO
                //builder.Property(x => x.ControlPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Crates).HasOffset(???); //TODO
                //builder.Property(x => x.CratePalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1908);

                builder = AddVersion(CacheType.MccHalo4U6, null);
                builder.Property(x => x.StructureBsps).HasOffset(164);
                builder.Property(x => x.Skies).HasOffset(220);
                builder.Property(x => x.ObjectNames).HasOffset(324);
                builder.Property(x => x.Scenery).HasOffset(336);
                builder.Property(x => x.SceneryPalette).HasOffset(348);
                builder.Property(x => x.Machines).HasOffset(468);
                builder.Property(x => x.MachinePalette).HasOffset(480);
                builder.Property(x => x.Controls).HasOffset(516);
                builder.Property(x => x.ControlPalette).HasOffset(528);
                builder.Property(x => x.Crates).HasOffset(1592);
                builder.Property(x => x.CratePalette).HasOffset(1604);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1868);
            }

            private void Halo2X()
            {
                var builder = AddVersion(CacheType.MccHalo2X, null);
                builder.Property(x => x.StructureBsps).HasOffset(160);
                //builder.Property(x => x.Skies).HasOffset(???); //TODO
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Machines).HasOffset(???); //TODO
                //builder.Property(x => x.MachinePalette).HasOffset(???); //TODO
                //builder.Property(x => x.Controls).HasOffset(???); //TODO
                //builder.Property(x => x.ControlPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Crates).HasOffset(???); //TODO
                //builder.Property(x => x.CratePalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1908);

                builder = AddVersion(CacheType.MccHalo2XU10, null);
                builder.Property(x => x.StructureBsps).HasOffset(164);
                builder.Property(x => x.Skies).HasOffset(220);
                builder.Property(x => x.ObjectNames).HasOffset(324);
                builder.Property(x => x.Scenery).HasOffset(336);
                builder.Property(x => x.SceneryPalette).HasOffset(348);
                builder.Property(x => x.Machines).HasOffset(468);
                builder.Property(x => x.MachinePalette).HasOffset(480);
                builder.Property(x => x.Controls).HasOffset(516);
                builder.Property(x => x.ControlPalette).HasOffset(528);
                builder.Property(x => x.Crates).HasOffset(1592);
                builder.Property(x => x.CratePalette).HasOffset(1604);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1868);
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
                var builder = AddVersion(null, CacheType.Halo4Retail).HasFixedSize(296);
                builder.Property(x => x.BspReference).HasOffset(0);

                builder = AddVersion(CacheType.Halo4Retail, null).HasFixedSize(336);
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
                var builder = AddDefaultVersion().HasFixedSize(52);
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

    public partial class ObjectPlacementBlockBase
    {
        new protected abstract class DefinitionBuilderBase<TBlock> : PlacementBlockBase.DefinitionBuilderBase<TBlock>
            where TBlock : ObjectPlacementBlockBase
        {
            new protected static void Common(VersionBuilder builder)
            {
                PlacementBlockBase.DefinitionBuilderBase<TBlock>.Common(builder);
                builder.Property(x => x.VariantName).HasOffset(156);
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
                var builder = AddDefaultVersion().HasFixedSize(16);
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
                var builder = AddVersion(CacheType.Halo4Beta, null).HasFixedSize(292);
                Common(builder);

                builder = AddVersion(CacheType.Halo4Retail, null).HasFixedSize(380);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo4, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo4U6, null).HasFixedSize(380);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo2X, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo2XU10, null).HasFixedSize(384);
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
                var builder = AddVersion(CacheType.Halo4Beta, null).HasFixedSize(300);
                Common(builder);

                builder = AddVersion(CacheType.Halo4Retail, null).HasFixedSize(388);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo4, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo4U6, null).HasFixedSize(388);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo2X, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo2XU10, null).HasFixedSize(392);
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
                var builder = AddVersion(CacheType.Halo4Beta, null).HasFixedSize(292);
                Common(builder);

                builder = AddVersion(CacheType.Halo4Retail, null).HasFixedSize(380);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo4, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo4U6, null).HasFixedSize(380);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo2X, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo2XU10, null).HasFixedSize(384);
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
                var builder = AddVersion(CacheType.Halo4Beta, null).HasFixedSize(288);
                Common(builder);

                builder = AddVersion(CacheType.Halo4Retail, null).HasFixedSize(376);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo4, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo4U6, null).HasFixedSize(376);
                Common(builder);

                //builder = AddVersion(CacheType.MccHalo2X, null).HasFixedSize(???); //TODO
                //Common(builder);

                builder = AddVersion(CacheType.MccHalo2XU10, null).HasFixedSize(380);
                Common(builder);
            }
        }
    }
}
