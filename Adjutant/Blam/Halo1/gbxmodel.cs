using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class gbxmodel
    {
        [Offset(0)]
        public short Flags { get; set; }

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
    }

    [FixedSize(64)]
    public class MarkerGroup
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(52)]
        public BlockCollection<Marker> Markers { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(32)]
    public class Marker
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
    }

    [FixedSize(156)]
    public class Node
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

        //offset 64:
        //int16: super low LOD
        //int16: low LOD
        //int16: medium LOD
        //int16: high LOD

        [Offset(72)]
        public short PieceIndex { get; set; } //super high LOD

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
        public int TagId { get; set; }
    }
}
