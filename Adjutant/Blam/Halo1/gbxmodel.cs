using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adjutant.Geometry;
using System.IO;
using System.Numerics;

namespace Adjutant.Blam.Halo1
{
    public class gbxmodel : IRenderGeometry
    {
        private readonly CacheFile cache;

        public gbxmodel(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(0)]
        public ModelFlags Flags { get; set; }

        [Offset(48)]
        public float UScale { get; set; }

        [Offset(52)]
        public float VScale { get; set; }

        [Offset(172)]
        public BlockCollection<MarkerGroup> MarkerGroups { get; set; }

        [Offset(184)]
        public BlockCollection<Node> Nodes { get; set; }

        [Offset(196)]
        public BlockCollection<Region> Regions { get; set; }

        [Offset(208)]
        public BlockCollection<ModelSection> Sections { get; set; }

        [Offset(220)]
        public BlockCollection<Shader> Shaders { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => Regions.SelectMany(r => r.Permutations).Max(p => p.LodCount);

        public IGeometryModel ReadGeometry(int lod)
        {
            using (var reader = cache.CreateReader(cache.AddressTranslator))
            {
                var model = new GeometryModel { CoordinateSystem = CoordinateSystem.HaloCE };

                model.Nodes.AddRange(Nodes);
                model.MarkerGroups.AddRange(MarkerGroups);

                foreach (var region in Regions)
                {
                    var gRegion = new GeometryRegion { Name = region.Name };
                    gRegion.Permutations.AddRange(region.Permutations.Select(p =>
                        new GeometryPermutation
                        {
                            Name = p.Name,
                            NodeIndex = byte.MaxValue,
                            Transform = Matrix4x4.Identity,
                            TransformScale = 1,
                            BoundsIndex = -1,
                            MeshIndex = p.LodIndex(lod)
                        }));

                    model.Regions.Add(gRegion);
                }

                foreach (var section in Sections)
                {
                    var indices = new List<ushort>();
                    var vertices = new List<SkinnedVertex>();

                    var mesh = new GeometryMesh();

                    foreach (var submesh in section.Submeshes)
                    {
                        var gSubmesh = new GeometrySubmesh
                        {
                            MaterialIndex = submesh.ShaderIndex,
                            IndexStart = indices.Count,
                            IndexLength = submesh.IndexCount + 2,
                            VertexStart = vertices.Count,
                            VertexLength = submesh.VertexCount
                        };

                        var permutations = model.Regions
                            .SelectMany(r => r.Permutations)
                            .Where(p => p.MeshIndex == Sections.IndexOf(section));

                        foreach (var p in permutations)
                            ((List<IGeometrySubmesh>)p.Submeshes).Add(gSubmesh);

                        reader.Seek(cache.TagIndex.VertexDataOffset + cache.TagIndex.IndexDataOffset + submesh.IndexOffset, SeekOrigin.Begin);
                        indices.AddRange(reader.ReadEnumerable<ushort>(gSubmesh.IndexLength));

                        reader.Seek(cache.TagIndex.VertexDataOffset + submesh.VertexOffset, SeekOrigin.Begin);
                        //var vertsTemp = reader.ReadEnumerable<SkinnedVertex>(submesh.VertexCount).ToList();
                        var vertsTemp = new List<SkinnedVertex>();
                        for (int i = 0; i < submesh.VertexCount; i++)
                        {
                            var position = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            var normal = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            var binormal = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            var tangent = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            var texcoord = new RealVector2D(reader.ReadSingle() * UScale, reader.ReadSingle() * VScale);
                            var nodes = new RealVector2D(reader.ReadInt16(), reader.ReadInt16());
                            var weights = new RealVector2D(reader.ReadSingle(), reader.ReadSingle());

                            vertsTemp.Add(new SkinnedVertex
                            {
                                Position = position,
                                Normal = normal,
                                Binormal = binormal,
                                Tangent = tangent,
                                TexCoords = texcoord,
                                NodeIndex1 = (short)nodes.X,
                                NodeIndex2 = (short)nodes.Y,
                                NodeWeights = weights
                            });
                        }

                        vertices.AddRange(vertsTemp);
                    }

                    mesh.IndexFormat = IndexFormat.Stripped;
                    mesh.VertexWeights = VertexWeights.Multiple;
                    mesh.Indicies = indices.Select(i => (int)i).ToArray();
                    mesh.Vertices = vertices.ToArray();

                    model.Meshes.Add(mesh);
                }

                return model;
            }
        }

        #endregion
    }

    [Flags]
    public enum ModelFlags : short
    {
        UseLocalNodes = 1
    }

    [FixedSize(64)]
    public class MarkerGroup : IGeometryMarkerGroup
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(52)]
        public BlockCollection<Marker> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }

    [FixedSize(32)]
    public class Marker : IGeometryMarker
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(1)]
        public byte PermutationIndex { get; set; }

        [Offset(2)]
        public byte NodeIndex { get; set; }

        //something here

        [Offset(4)]
        public RealVector3D Position { get; set; }

        [Offset(16)]
        public RealVector4D Rotation { get; set; }

        #region IGeometryMarker

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(156)]
    public class Node : IGeometryNode
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public short NextSiblingIndex { get; set; }

        [Offset(34)]
        public short FirstChildIndex { get; set; }

        [Offset(36)]
        public short ParentIndex { get; set; }

        //int16 here

        [Offset(40)]
        public RealVector3D Position { get; set; }

        [Offset(56)]
        public RealVector4D Rotation { get; set; }

        [Offset(72)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        IRealVector3D IGeometryNode.Position => Position;

        IRealVector4D IGeometryNode.Rotation => Rotation;

        #endregion
    }

    [FixedSize(76)]
    public class Region
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(64)]
        public BlockCollection<Permutation> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(88)]
    public class Permutation
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }
        
        [Offset(64)]
        public short SuperLowSectionIndex { get; set; }

        [Offset(66)]
        public short LowSectionIndex { get; set; }

        [Offset(68)]
        public short MediumSectionIndex { get; set; }

        [Offset(70)]
        public short HighSectionIndex { get; set; }

        [Offset(72)]
        public short SuperHighSectionIndex { get; set; }

        internal short[] LodArray => new[] { SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex };

        internal int LodCount => LodArray.Count(i => i >= 0);

        internal short LodIndex(int lod)
        {
            if (lod < 0 || lod > 4)
                throw new IndexOutOfRangeException();

            return LodArray.Take(lod + 1)
                .Reverse()
                .FirstOrDefault(i => i >= 0);
        }

        public override string ToString() => Name;
    }

    [FixedSize(48)]
    public class ModelSection
    {
        [Offset(36)]
        public BlockCollection<Submesh> Submeshes { get; set; }
    }

    [FixedSize(132)]
    public class Submesh
    {
        [Offset(4)]
        public short ShaderIndex { get; set; }

        [Offset(72)]
        public int IndexCount { get; set; }

        [Offset(76)]
        public int IndexOffset { get; set; }

        [Offset(88)]
        public int VertexCount { get; set; }

        [Offset(100)]
        public int VertexOffset { get; set; }
    }

    [FixedSize(32)]
    public class Shader
    {
        [Offset(12)]
        public TagReference ShaderReference { get; set; }
    }
}
