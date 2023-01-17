using System.Diagnostics;
using System.Numerics;

namespace Reclaimer.Geometry
{
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Model
    {
        public string Name { get; set; }
        public List<ModelRegion> Regions { get; } = new();
        public List<Marker> Markers { get; } = new();
        public List<Bone> Bones { get; } = new();
        public List<Mesh> Meshes { get; } = new();
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ModelRegion
    {
        public string Name { get; set; }
        public List<ModelPermutation> Permutations { get; } = new();
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ModelPermutation
    {
        public string Name { get; set; }
        public bool IsInstanced { get; set; }
        public (int Index, int Count) MeshRange { get; set; }
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        public IEnumerable<int> MeshIndices => MeshRange.Index >= 0 ? Enumerable.Range(MeshRange.Index, MeshRange.Count) : Enumerable.Empty<int>();
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Marker
    {
        public string Name { get; set; }
        public List<MarkerInstance> Instances { get; } = new();
    }

    public class MarkerInstance
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        [Obsolete("legacy")]
        public int RegionIndex { get; set; }

        [Obsolete("legacy")]
        public int PermutationIndex { get; set; }

        [Obsolete("legacy")]
        public int BoneIndex { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Bone
    {
        public string Name { get; set; }
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        [Obsolete("legacy")]
        public int ParentIndex { get; set; }

        [Obsolete("legacy")]
        public int FirstChildIndex { get; set; }

        [Obsolete("legacy")]
        public int NextSiblingIndex { get; set; }

        [Obsolete("legacy")]
        public Quaternion Rotation { get; set; }

        [Obsolete("legacy")]
        public Vector3 Position { get; set; }
    }

    public class Mesh : IMeshCompat
    {
        public VertexBuffer VertexBuffer { get; set; }
        public IIndexBuffer IndexBuffer { get; set; }
        public List<MeshSegment> Segments { get; } = new();

        [Obsolete("legacy")]
        public byte? BoneIndex { get; set; }

        [Obsolete("legacy")]
        public int VertexWeights => (VertexBuffer.HasBlendIndices || BoneIndex.HasValue) ? VertexBuffer.HasBlendWeights ? 1 : 2 : 0; //skinned : rigid : none

        public RealBounds3D PositionBounds { get; set; }
        public RealBounds2D TextureBounds { get; set; }

        public bool IsCompressed => !PositionBounds.IsEmpty || !TextureBounds.IsEmpty;
    }

    public class MeshSegment : ISubmeshCompat
    {
        public int IndexStart { get; set; }
        public int IndexLength { get; set; }
        public Material Material { get; set; }
    }
}
