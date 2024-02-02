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
                //builder.Property(x => x.Skies).HasOffset(???); //TODO
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1828);

                builder = AddVersion(CacheType.HaloReachRetail);
                builder.Property(x => x.StructureBsps).HasOffset(76);
                //builder.Property(x => x.Skies).HasOffset(???); //TODO
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1844);

                builder = AddVersion(CacheType.MccHaloReach, null);
                builder.Property(x => x.StructureBsps).HasOffset(76);
                //builder.Property(x => x.Skies).HasOffset(???); //TODO
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
                builder.Property(x => x.ScenarioLightmapReference).HasOffset(1856);

                builder = AddVersion(CacheType.MccHaloReachU13, null);
                builder.Property(x => x.StructureBsps).HasOffset(80);
                //builder.Property(x => x.Skies).HasOffset(???); //TODO
                //builder.Property(x => x.ObjectNames).HasOffset(???); //TODO
                //builder.Property(x => x.Scenery).HasOffset(???); //TODO
                //builder.Property(x => x.SceneryPalette).HasOffset(???); //TODO
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
}
