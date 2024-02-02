using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.HaloReach
{
    [StructureDefinition<scenario_structure_bsp, DefinitionBuilder>]
    public partial class scenario_structure_bsp
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario_structure_bsp>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.HaloReachBeta);
                builder.Property(x => x.XBounds).HasOffset(236);
                builder.Property(x => x.YBounds).HasOffset(244);
                builder.Property(x => x.ZBounds).HasOffset(252);
                builder.Property(x => x.Clusters).HasOffset(308);
                builder.Property(x => x.Shaders).HasOffset(320);
                builder.Property(x => x.GeometryInstances).HasOffset(620);
                builder.Property(x => x.Sections).HasOffset(1112);
                builder.Property(x => x.BoundingBoxes).HasOffset(1124);
                //builder.Property(x => x.InstancesResourcePointer).HasOffset(-); //not applicable to beta

                builder = AddVersion(CacheType.HaloReachRetail);
                builder.Property(x => x.XBounds).HasOffset(236);
                builder.Property(x => x.YBounds).HasOffset(244);
                builder.Property(x => x.ZBounds).HasOffset(252);
                builder.Property(x => x.Clusters).HasOffset(308);
                builder.Property(x => x.Shaders).HasOffset(320);
                builder.Property(x => x.GeometryInstances).HasOffset(608);
                builder.Property(x => x.Sections).HasOffset(1100);
                builder.Property(x => x.BoundingBoxes).HasOffset(1112);
                builder.Property(x => x.InstancesResourcePointer).HasOffset(1296);

                builder = AddVersion(CacheType.MccHaloReach, null);
                builder.Property(x => x.XBounds).HasOffset(240);
                builder.Property(x => x.YBounds).HasOffset(248);
                builder.Property(x => x.ZBounds).HasOffset(256);
                builder.Property(x => x.Clusters).HasOffset(312);
                builder.Property(x => x.Shaders).HasOffset(324);
                builder.Property(x => x.GeometryInstances).HasOffset(612);
                builder.Property(x => x.Sections).HasOffset(1128);
                builder.Property(x => x.BoundingBoxes).HasOffset(1140);
                builder.Property(x => x.InstancesResourcePointer).HasOffset(1336);

                builder = AddVersion(CacheType.MccHaloReachU13, null);
                builder.Property(x => x.XBounds).HasOffset(240);
                builder.Property(x => x.YBounds).HasOffset(248);
                builder.Property(x => x.ZBounds).HasOffset(256);
                builder.Property(x => x.Clusters).HasOffset(312);
                builder.Property(x => x.Shaders).HasOffset(324);
                builder.Property(x => x.GeometryInstances).HasOffset(600);
                builder.Property(x => x.Sections).HasOffset(1104);
                builder.Property(x => x.BoundingBoxes).HasOffset(1116);
                builder.Property(x => x.InstancesResourcePointer).HasOffset(1312);
            }
        }
    }
}
