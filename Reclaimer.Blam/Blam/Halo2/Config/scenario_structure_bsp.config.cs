using Reclaimer.IO;
using Reclaimer.IO.Dynamic;

namespace Reclaimer.Blam.Halo2
{
    [StructureDefinition<scenario_structure_bsp, DefinitionBuilder>]
    public partial class scenario_structure_bsp
    {
        private sealed class DefinitionBuilder : DefinitionBuilder<scenario_structure_bsp>
        {
            public DefinitionBuilder()
            {
                var builder = AddDefaultVersion();
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
                var builder = AddDefaultVersion().HasFixedSize(176);
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
                var builder = AddDefaultVersion().HasFixedSize(200);
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
