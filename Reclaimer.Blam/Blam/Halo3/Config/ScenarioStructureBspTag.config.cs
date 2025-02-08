using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo3
{
    [StructureDefinition<ScenarioStructureBspTag, DefinitionBuilder>]
    public partial class ScenarioStructureBspTag
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ScenarioStructureBspTag>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(null, CacheType.MccHalo3U4);
                builder.Property(x => x.XBounds).HasOffset(60);
                builder.Property(x => x.YBounds).HasOffset(68);
                builder.Property(x => x.ZBounds).HasOffset(76);
                builder.Property(x => x.Clusters).HasOffset(180);
                builder.Property(x => x.Shaders).HasOffset(192);
                builder.Property(x => x.GeometryInstances).HasOffset(432);
                builder.Property(x => x.ResourcePointer1).HasOffset(580);
                builder.Property(x => x.Sections).HasOffset(740);
                builder.Property(x => x.BoundingBoxes).HasOffset(752);
                builder.Property(x => x.ResourcePointer2).HasOffset(860);
                builder.Property(x => x.ResourcePointer3).HasOffset(892);

                builder = AddVersion(CacheType.MccHalo3U4, null);
                builder.Property(x => x.XBounds).HasOffset(64);
                builder.Property(x => x.YBounds).HasOffset(72);
                builder.Property(x => x.ZBounds).HasOffset(80);
                builder.Property(x => x.Clusters).HasOffset(196);
                builder.Property(x => x.Shaders).HasOffset(208);
                builder.Property(x => x.GeometryInstances).HasOffset(448);
                builder.Property(x => x.ResourcePointer1).HasOffset(596);
                builder.Property(x => x.Sections).HasOffset(756);
                builder.Property(x => x.BoundingBoxes).HasOffset(768);
                builder.Property(x => x.ResourcePointer2).HasOffset(876);
                builder.Property(x => x.ResourcePointer3).HasOffset(908);

                builder = AddVersion(CacheType.Halo3ODST, null);
                builder.Property(x => x.XBounds).HasOffset(64);
                builder.Property(x => x.YBounds).HasOffset(72);
                builder.Property(x => x.ZBounds).HasOffset(80);
                builder.Property(x => x.Clusters).HasOffset(184);
                builder.Property(x => x.Shaders).HasOffset(196);
                builder.Property(x => x.GeometryInstances).HasOffset(436);
                builder.Property(x => x.ResourcePointer1).HasOffset(584);
                builder.Property(x => x.Sections).HasOffset(744);
                builder.Property(x => x.BoundingBoxes).HasOffset(756);
                builder.Property(x => x.ResourcePointer2).HasOffset(864);
                builder.Property(x => x.ResourcePointer3).HasOffset(896);
            }
        }
    }

    [StructureDefinition<ClusterBlock, DefinitionBuilder>]
    public partial class ClusterBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ClusterBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(null, CacheType.Halo3Retail).HasFixedSize(236);
                builder.Property(x => x.XBounds).HasOffset(0);
                builder.Property(x => x.YBounds).HasOffset(8);
                builder.Property(x => x.ZBounds).HasOffset(16);
                builder.Property(x => x.SectionIndex).HasOffset(172);

                builder = AddVersion(CacheType.Halo3Retail, null).HasFixedSize(220);
                builder.Property(x => x.XBounds).HasOffset(0);
                builder.Property(x => x.YBounds).HasOffset(8);
                builder.Property(x => x.ZBounds).HasOffset(16);
                builder.Property(x => x.SectionIndex).HasOffset(156);

                builder = AddVersion(CacheType.MccHalo3, null).HasFixedSize(280);
                builder.Property(x => x.XBounds).HasOffset(0);
                builder.Property(x => x.YBounds).HasOffset(8);
                builder.Property(x => x.ZBounds).HasOffset(16);
                builder.Property(x => x.SectionIndex).HasOffset(216);

                builder = AddVersion(CacheType.Halo3ODST, null).HasFixedSize(220);
                builder.Property(x => x.XBounds).HasOffset(0);
                builder.Property(x => x.YBounds).HasOffset(8);
                builder.Property(x => x.ZBounds).HasOffset(16);
                builder.Property(x => x.SectionIndex).HasOffset(156);

                builder = AddVersion(CacheType.MccHalo3ODST, null).HasFixedSize(280);
                builder.Property(x => x.XBounds).HasOffset(0);
                builder.Property(x => x.YBounds).HasOffset(8);
                builder.Property(x => x.ZBounds).HasOffset(16);
                builder.Property(x => x.SectionIndex).HasOffset(216);
            }
        }
    }

    [StructureDefinition<BspGeometryInstanceBlock, DefinitionBuilder>]
    public partial class BspGeometryInstanceBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<BspGeometryInstanceBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(120);
                builder.Property(x => x.TransformScale).HasOffset(0);
                builder.Property(x => x.Transform).HasOffset(4);
                builder.Property(x => x.SectionIndex).HasOffset(52);
                builder.Property(x => x.Name).HasOffset(84);
            }
        }
    }

    [StructureDefinition<BspBoundingBoxBlock, DefinitionBuilder>]
    public partial class BspBoundingBoxBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<BspBoundingBoxBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(44);
                builder.Property(x => x.XBounds).HasOffset(4);
                builder.Property(x => x.YBounds).HasOffset(12);
                builder.Property(x => x.ZBounds).HasOffset(20);
                builder.Property(x => x.UBounds).HasOffset(28);
                builder.Property(x => x.VBounds).HasOffset(36);
            }
        }
    }
}
