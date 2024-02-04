using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo3
{
    [StructureDefinition<scenario, DefinitionBuilder>]
    public partial class scenario
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario>
        {
            public DefinitionBuilder()
            {
                PreRelease();
                Halo3();
                Odst();
            }

            private void PreRelease()
            {
                //Halo3Alpha not actually supported, but needs to be defined because the scenario tag
                //gets loaded when the map gets opened. For h3 alpha it will just have null property values
                //since none of the properties have offsets defined.
                var builder = AddVersion(CacheType.Halo3Alpha);

                builder = AddVersion(CacheType.Halo3Delta);
                builder.Property(x => x.StructureBsps).HasOffset(12);
                builder.Property(x => x.Skies).HasOffset(40);
                builder.Property(x => x.ObjectNames).HasOffset(156);
                builder.Property(x => x.Scenery).HasOffset(168);
                builder.Property(x => x.SceneryPalette).HasOffset(180);
                builder.Property(x => x.Machines).HasOffset(300);
                builder.Property(x => x.MachinePalette).HasOffset(312);
                builder.Property(x => x.Controls).HasOffset(348);
                builder.Property(x => x.ControlPalette).HasOffset(360);
                builder.Property(x => x.Crates).HasOffset(1460);
                builder.Property(x => x.CratePalette).HasOffset(1472);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1736);
                
                builder = AddVersion(CacheType.Halo3Beta);
                builder.Property(x => x.StructureBsps).HasOffset(12);
                builder.Property(x => x.Skies).HasOffset(40);
                builder.Property(x => x.ObjectNames).HasOffset(156);
                builder.Property(x => x.Scenery).HasOffset(168);
                builder.Property(x => x.SceneryPalette).HasOffset(180);
                builder.Property(x => x.Machines).HasOffset(300);
                builder.Property(x => x.MachinePalette).HasOffset(312);
                builder.Property(x => x.Controls).HasOffset(348);
                builder.Property(x => x.ControlPalette).HasOffset(360);
                builder.Property(x => x.Crates).HasOffset(1444);
                builder.Property(x => x.CratePalette).HasOffset(1456);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1720);
            }

            private void Halo3()
            {
                var builder = AddVersion(CacheType.Halo3Retail, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.Skies).HasOffset(48);
                builder.Property(x => x.ObjectNames).HasOffset(176);
                builder.Property(x => x.Scenery).HasOffset(188);
                builder.Property(x => x.SceneryPalette).HasOffset(200);
                builder.Property(x => x.Machines).HasOffset(320);
                builder.Property(x => x.MachinePalette).HasOffset(332);
                builder.Property(x => x.Controls).HasOffset(368);
                builder.Property(x => x.ControlPalette).HasOffset(380);
                builder.Property(x => x.Crates).HasOffset(1512);
                builder.Property(x => x.CratePalette).HasOffset(1524);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1776);

                builder = AddVersion(CacheType.MccHalo3F6, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.Skies).HasOffset(48);
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Machines).HasOffset(???); //TODO
                //builder.Property(x => x.MachinePalette).HasOffset(???); //TODO
                //builder.Property(x => x.Controls).HasOffset(???); //TODO
                //builder.Property(x => x.ControlPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Crates).HasOffset(???); //TODO
                //builder.Property(x => x.CratePalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1764);

                builder = AddVersion(CacheType.MccHalo3U12, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.Skies).HasOffset(52);
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Machines).HasOffset(???); //TODO
                //builder.Property(x => x.MachinePalette).HasOffset(???); //TODO
                //builder.Property(x => x.Controls).HasOffset(???); //TODO
                //builder.Property(x => x.ControlPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Crates).HasOffset(???); //TODO
                //builder.Property(x => x.CratePalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1700);

                builder = AddVersion(CacheType.MccHalo3U13, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.Skies).HasOffset(52);
                builder.Property(x => x.ObjectNames).HasOffset(168);
                builder.Property(x => x.Scenery).HasOffset(180);
                builder.Property(x => x.SceneryPalette).HasOffset(192);
                builder.Property(x => x.Machines).HasOffset(312);
                builder.Property(x => x.MachinePalette).HasOffset(324);
                builder.Property(x => x.Controls).HasOffset(360);
                builder.Property(x => x.ControlPalette).HasOffset(372);
                builder.Property(x => x.Crates).HasOffset(1468);
                builder.Property(x => x.CratePalette).HasOffset(1480);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1708);
            }

            private void Odst()
            {
                var builder = AddVersion(CacheType.Halo3ODST, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.Skies).HasOffset(76);
                builder.Property(x => x.ObjectNames).HasOffset(216);
                builder.Property(x => x.Scenery).HasOffset(228);
                builder.Property(x => x.SceneryPalette).HasOffset(240);
                builder.Property(x => x.Machines).HasOffset(360);
                builder.Property(x => x.MachinePalette).HasOffset(372);
                builder.Property(x => x.Controls).HasOffset(432);
                builder.Property(x => x.ControlPalette).HasOffset(444);
                builder.Property(x => x.Crates).HasOffset(1588);
                builder.Property(x => x.CratePalette).HasOffset(1600);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1852);

                builder = AddVersion(CacheType.MccHalo3ODSTF3, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.Skies).HasOffset(76);
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Machines).HasOffset(???); //TODO
                //builder.Property(x => x.MachinePalette).HasOffset(???); //TODO
                //builder.Property(x => x.Controls).HasOffset(???); //TODO
                //builder.Property(x => x.ControlPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Crates).HasOffset(???); //TODO
                //builder.Property(x => x.CratePalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1840);

                builder = AddVersion(CacheType.MccHalo3ODSTU7, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.Skies).HasOffset(80);
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Machines).HasOffset(???); //TODO
                //builder.Property(x => x.MachinePalette).HasOffset(???); //TODO
                //builder.Property(x => x.Controls).HasOffset(???); //TODO
                //builder.Property(x => x.ControlPalette).HasOffset(???); //TODO
                //builder.Property(x => x.Crates).HasOffset(???); //TODO
                //builder.Property(x => x.CratePalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1776);

                builder = AddVersion(CacheType.MccHalo3ODSTU8, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.Skies).HasOffset(80);
                builder.Property(x => x.ObjectNames).HasOffset(196);
                builder.Property(x => x.Scenery).HasOffset(208);
                builder.Property(x => x.SceneryPalette).HasOffset(220);
                builder.Property(x => x.Machines).HasOffset(340);
                builder.Property(x => x.MachinePalette).HasOffset(352);
                builder.Property(x => x.Controls).HasOffset(412);
                builder.Property(x => x.ControlPalette).HasOffset(424);
                builder.Property(x => x.Crates).HasOffset(1532);
                builder.Property(x => x.CratePalette).HasOffset(1544);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1772);
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
                var builder = AddVersion(null, CacheType.Halo3Retail).HasFixedSize(104);
                builder.Property(x => x.BspReference).HasOffset(0);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(108);
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
                var builder = AddDefaultVersion().HasFixedSize(20);
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

            protected void Halo3Beta(VersionBuilder builder)
            {
                Common(builder);
            }

            protected void Halo3Retail(VersionBuilder builder)
            {
                Common(builder);
            }

            protected void Halo3Odst(VersionBuilder builder)
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
            new protected void Halo3Beta(VersionBuilder builder)
            {
                base.Halo3Beta(builder);
                builder.Property(x => x.VariantName).HasOffset(76);
            }

            new protected void Halo3Retail(VersionBuilder builder)
            {
                base.Halo3Retail(builder);
                builder.Property(x => x.VariantName).HasOffset(84);
            }

            new protected void Halo3Odst(VersionBuilder builder)
            {
                base.Halo3Odst(builder);
                builder.Property(x => x.VariantName).HasOffset(84);
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
                var builder = AddVersion(CacheType.Halo3Delta, null).HasFixedSize(48);
                builder.Property(x => x.TagReference).HasOffset(0);

                //TODO: confirm which version this changed in
                builder = AddVersion(CacheType.MccHalo3U13, null).HasFixedSize(16);
                builder.Property(x => x.TagReference).HasOffset(0);

                builder = AddVersion(CacheType.Halo3ODST, null).HasFixedSize(48);
                builder.Property(x => x.TagReference).HasOffset(0);

                //TODO: confirm which version this changed in
                builder = AddVersion(CacheType.MccHalo3ODSTU8, null).HasFixedSize(16);
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
                var builder = AddVersion(CacheType.Halo3Delta, null).HasFixedSize(160);
                Halo3Beta(builder);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(180);
                Halo3Retail(builder);
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
                var builder = AddVersion(CacheType.Halo3Delta, null).HasFixedSize(100);
                Halo3Beta(builder);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(112);
                Halo3Retail(builder);

                builder = AddVersion(CacheType.MccHalo3U13, null).HasFixedSize(136);
                Halo3Retail(builder);

                builder = AddVersion(CacheType.Halo3ODST, null).HasFixedSize(136);
                Halo3Odst(builder);
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
                var builder = AddVersion(CacheType.Halo3Delta, null).HasFixedSize(92);
                Halo3Beta(builder);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(100);
                Halo3Retail(builder);

                builder = AddVersion(CacheType.MccHalo3U13, null).HasFixedSize(120);
                Halo3Retail(builder);

                builder = AddVersion(CacheType.Halo3ODST, null).HasFixedSize(124);
                Halo3Odst(builder);

                builder = AddVersion(CacheType.MccHalo3ODSTU8, null).HasFixedSize(120);
                Halo3Odst(builder);
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
                var builder = AddVersion(CacheType.Halo3Delta, null).HasFixedSize(156);
                Halo3Beta(builder);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(176);
                Halo3Retail(builder);
            }
        }
    }
}
