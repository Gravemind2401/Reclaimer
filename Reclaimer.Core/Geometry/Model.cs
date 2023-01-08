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
        public (int Index, int Count) MeshRange { get; set; }
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
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
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Bone
    {
        public string Name { get; set; }
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
    }

    public class Mesh : IMeshCompat
    {
        public VertexBuffer VertexBuffer { get; set; }
        public IIndexBuffer IndexBuffer { get; set; }
        public List<MeshSegment> Segments { get; } = new();

        public Vector3 MinBounds { get; set; }
        public Vector3 MaxBounds { get; set; }
        public Vector2 MinTexBounds { get; set; }
        public Vector2 MaxTexBounds { get; set; }

        public bool IsCompressed => MinBounds.LengthSquared() != 0 && MaxBounds.LengthSquared() != 0;
    }

    public class MeshSegment : ISubmeshCompat
    {
        public int IndexStart { get; set; }
        public int IndexLength { get; set; }
        public Material Material { get; set; }
    }
}
