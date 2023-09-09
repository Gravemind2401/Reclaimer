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
                //gets loaded when the map gets opened. For h3a it will just have null property values
                //since none of the properties have offsets defined.
                var builder = AddVersion(CacheType.Halo3Alpha);

                builder = AddVersion(CacheType.Halo3Delta);
                builder.Property(x => x.StructureBsps).HasOffset(12);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1736);
                
                builder = AddVersion(CacheType.Halo3Beta);
                builder.Property(x => x.StructureBsps).HasOffset(12);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1720);
            }

            private void Halo3()
            {
                var builder = AddVersion(CacheType.Halo3Retail, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1776);

                builder = AddVersion(CacheType.MccHalo3F6, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1764);

                builder = AddVersion(CacheType.MccHalo3U12, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1700);

                builder = AddVersion(CacheType.MccHalo3U13, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1708);
            }

            private void Odst()
            {
                var builder = AddVersion(CacheType.Halo3ODST, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1852);

                builder = AddVersion(CacheType.MccHalo3ODSTF3, null);
                builder.Property(x => x.StructureBsps).HasOffset(20);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1840);

                builder = AddVersion(CacheType.MccHalo3ODSTU7, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1776);

                builder = AddVersion(CacheType.MccHalo3ODSTU8, null);
                builder.Property(x => x.StructureBsps).HasOffset(24);
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
}
