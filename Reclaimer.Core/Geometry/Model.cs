using Reclaimer.Geometry.Compatibility;

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


        public Matrix4x4 GetBoneWorldTransform(int boneIndex)
        {
            var bone = Bones[boneIndex];

            if (bone.WorldTransform != default && !bone.WorldTransform.IsIdentity)
                return bone.WorldTransform;

            var parentTransform = bone.ParentIndex < 0
                ? Matrix4x4.Identity
                : GetBoneWorldTransform(bone.ParentIndex);

            return bone.LocalTransform * parentTransform;
        }

        /// <summary>
        /// Enumerates all distinct materials across all meshes in the model.
        /// </summary>
        public IEnumerable<Material> EnumerateMaterials() => Meshes.SelectMany(EnumerateMaterials);

        public IEnumerable<Material> EnumerateExportedMaterials()
        {
            return Regions.Where(r => r.Export)
                .SelectMany(r => r.Permutations)
                .Where(p => p.Export)
                .SelectMany(p => p.MeshIndices)
                .Select(i => Meshes.ElementAtOrDefault(i))
                .SelectMany(EnumerateMaterials);
        }

        private static IEnumerable<Material> EnumerateMaterials(Mesh mesh)
        {
            return mesh?.Segments
                .Select(s => s.Material)
                .Where(m => m != null)
                .DistinctBy(m => m.Id) ?? Enumerable.Empty<Material>();
        }
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class ModelRegion
    {
        public string Name { get; set; }
        public bool Export { get; set; } = true;
        public List<ModelPermutation> Permutations { get; } = new();
        public CustomProperties CustomProperties { get; } = new();
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
        public CustomProperties CustomProperties { get; } = new();

        public float UniformScale
        {
            get => Scale.Y == Scale.X && Scale.Z == Scale.X ? Scale.X : float.NaN;
            set => Scale = new Vector3(value);
        }

        public IEnumerable<int> MeshIndices => MeshRange.Index >= 0 && MeshRange.Count > 0 ? Enumerable.Range(MeshRange.Index, MeshRange.Count) : Enumerable.Empty<int>();

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
        public CustomProperties CustomProperties { get; } = new();
    }

    public class MarkerInstance
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        //TODO: use reference instead of index
        public int RegionIndex { get; set; }

        //TODO: use reference instead of index
        public int PermutationIndex { get; set; }

        //TODO: use reference instead of index
        public int BoneIndex { get; set; }

        public CustomProperties CustomProperties { get; } = new();
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class Bone
    {
        public string Name { get; set; }
        public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 WorldTransform { get; set; } = Matrix4x4.Identity;

        //TODO: use reference instead of index
        public int ParentIndex { get; set; }

        [Obsolete("Backwards compatibility for AMF")]
        public int FirstChildIndex { get; set; }

        [Obsolete("Backwards compatibility for AMF")]
        public int NextSiblingIndex { get; set; }

        [Obsolete("Backwards compatibility for AMF")]
        public Quaternion Rotation
        {
            get
            {
                Matrix4x4.Decompose(LocalTransform, out _, out var rotation, out _);
                return rotation;
            }
        }

        [Obsolete("Backwards compatibility for AMF")]
        public Vector3 Position
        {
            get
            {
                Matrix4x4.Decompose(LocalTransform, out _, out _, out var translation);
                return translation;
            }
        }

        public CustomProperties CustomProperties { get; } = new();
    }

    public class Mesh
    {
        public List<MeshSegment> Segments { get; } = new();

        public VertexBuffer VertexBuffer { get; set; }
        public IIndexBuffer IndexBuffer { get; set; }

        //TODO: use reference instead of index
        public byte? BoneIndex { get; set; }

        [Obsolete("Backwards compatibility for AMF")]
        public VertexWeightsCompat VertexWeights => (VertexBuffer.HasBlendIndices || BoneIndex.HasValue) ? VertexBuffer.HasBlendWeights ? VertexWeightsCompat.Skinned : VertexWeightsCompat.Rigid : VertexWeightsCompat.None;

        public RealBounds3D PositionBounds { get; set; }
        public RealBounds2D TextureBounds { get; set; }

        public CustomProperties CustomProperties { get; } = new();

        public int VertexCount => VertexBuffer?.Count ?? 0;
        public int IndexCount => IndexBuffer?.Count ?? 0;

        public bool IsCompressed => !PositionBounds.IsEmpty || !TextureBounds.IsEmpty;
    }

    public class MeshSegment
    {
        public int IndexStart { get; set; }
        public int IndexLength { get; set; }
        public Material Material { get; set; }

        public CustomProperties CustomProperties { get; } = new();
    }
}
