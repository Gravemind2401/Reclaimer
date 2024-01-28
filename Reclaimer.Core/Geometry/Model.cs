using System.Diagnostics;
using System.Numerics;

namespace Reclaimer.Geometry
{
    public class Model : SceneObject
    {
        public List<ModelRegion> Regions { get; } = new();
        public List<Marker> Markers { get; } = new();
        public List<Bone> Bones { get; } = new();
        public List<Mesh> Meshes { get; } = new();

        /// <summary>
        /// Sets the <see cref="Mesh.PositionBounds"/> and <see cref="Mesh.TextureBounds"/>
        /// of all non-null meshes currently in the mesh list.
        /// <br/>Does not automatically update new meshes added later on.
        /// </summary>
        /// <param name="positionBounds">The compression boundaries for vertex positions.</param>
        /// <param name="textureBounds">The compression boundaries for texture coordinates.</param>
        public void SetCompressionBounds(RealBounds3D positionBounds, RealBounds2D textureBounds)
        {
            foreach (var mesh in Meshes.Where(m => m != null))
                (mesh.PositionBounds, mesh.TextureBounds) = (positionBounds, textureBounds);
        }

        /// <summary>
        /// Enumerates all distinct materials across all meshes in the model.
        /// </summary>
        public IEnumerable<Material> EnumerateMaterials()
        {
            return Meshes.Where(m => m != null)
                .SelectMany(m => m.Segments)
                .Select(s => s.Material)
                .Where(m => m != null)
                .DistinctBy(m => m.Id);
        }
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ModelRegion
    {
        public string Name { get; set; }
        public bool Export { get; set; } = true;
        public List<ModelPermutation> Permutations { get; } = new();
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ModelPermutation
    {
        public string Name { get; set; }
        public bool Export { get; set; } = true;
        public bool IsInstanced { get; set; }
        public (int Index, int Count) MeshRange { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        public float UniformScale
        {
            get => Scale.Y == Scale.X && Scale.Z == Scale.X ? Scale.X : float.NaN;
            set => Scale = new Vector3(value);
        }

        public IEnumerable<int> MeshIndices => MeshRange.Index >= 0 ? Enumerable.Range(MeshRange.Index, MeshRange.Count) : Enumerable.Empty<int>();

        public Matrix4x4 GetFinalTransform()
        {
            Matrix4x4.Decompose(Transform, out var scale, out var rotation, out var translation);
            return Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);
        }
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
