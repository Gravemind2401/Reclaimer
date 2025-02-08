using Reclaimer.Blam.Common;
using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo2
{
    [StructureDefinition<ScenarioStructureBspTag, DefinitionBuilder>]
    public partial class ScenarioStructureBspTag
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<ScenarioStructureBspTag>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.Halo2Beta);
                builder.Property(x => x.XBounds).HasOffset(72);
                builder.Property(x => x.YBounds).HasOffset(80);
                builder.Property(x => x.ZBounds).HasOffset(88);
                builder.Property(x => x.Clusters).HasOffset(212);
                builder.Property(x => x.Shaders).HasOffset(224);
                builder.Property(x => x.Sections).HasOffset(452);
                builder.Property(x => x.GeometryInstances).HasOffset(464);

                builder = AddVersion(CacheType.Halo2Xbox, null);
                builder.Property(x => x.XBounds).HasOffset(52);
                builder.Property(x => x.YBounds).HasOffset(60);
                builder.Property(x => x.ZBounds).HasOffset(68);
                builder.Property(x => x.Clusters).HasOffset(156);
                builder.Property(x => x.Shaders).HasOffset(164);
                builder.Property(x => x.Sections).HasOffset(312);
                builder.Property(x => x.GeometryInstances).HasOffset(320);
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
                var builder = AddVersion(CacheType.Halo2Beta).HasFixedSize(196);
                builder.Property(x => x.VertexCount).HasOffset(0);
                builder.Property(x => x.FaceCount).HasOffset(2);
                builder.Property(x => x.BoundingBoxes).HasOffset(24);
                builder.Property(x => x.DataPointer).HasOffset(44);
                builder.Property(x => x.DataSize).HasOffset(48);
                builder.Property(x => x.HeaderSize).HasOffset(52);
                builder.Property(x => x.Resources).HasOffset(60);

                builder = AddVersion(CacheType.Halo2Xbox, null).HasFixedSize(176);
                builder.Property(x => x.VertexCount).HasOffset(0);
                builder.Property(x => x.FaceCount).HasOffset(2);
                builder.Property(x => x.BoundingBoxes).HasOffset(24);
                builder.Property(x => x.DataPointer).HasOffset(40);
                builder.Property(x => x.DataSize).HasOffset(44);
                builder.Property(x => x.HeaderSize).HasOffset(48);
                builder.Property(x => x.Resources).HasOffset(56);
            }
        }
    }

    [StructureDefinition<BspSectionBlock, DefinitionBuilder>]
    public partial class BspSectionBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<BspSectionBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddVersion(CacheType.Halo2Beta).HasFixedSize(260);
                builder.Property(x => x.VertexCount).HasOffset(0);
                builder.Property(x => x.FaceCount).HasOffset(2);
                builder.Property(x => x.BoundingBoxes).HasOffset(24);
                builder.Property(x => x.DataPointer).HasOffset(44);
                builder.Property(x => x.DataSize).HasOffset(48);
                builder.Property(x => x.HeaderSize).HasOffset(52);
                builder.Property(x => x.Resources).HasOffset(60);

                builder = AddVersion(CacheType.Halo2Xbox, null).HasFixedSize(200);
                builder.Property(x => x.VertexCount).HasOffset(0);
                builder.Property(x => x.FaceCount).HasOffset(2);
                builder.Property(x => x.BoundingBoxes).HasOffset(24);
                builder.Property(x => x.DataPointer).HasOffset(40);
                builder.Property(x => x.DataSize).HasOffset(44);
                builder.Property(x => x.HeaderSize).HasOffset(48);
                builder.Property(x => x.Resources).HasOffset(56);
            }
        }
    }

    [StructureDefinition<GeometryInstanceBlock, DefinitionBuilder>]
    public partial class GeometryInstanceBlock
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<GeometryInstanceBlock>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion().HasFixedSize(88);
                builder.Property(x => x.TransformScale).HasOffset(0);
                builder.Property(x => x.Transform).HasOffset(4);
                builder.Property(x => x.SectionIndex).HasOffset(52);
                builder.Property(x => x.Name).HasOffset(80);
            }
        }
    }
}
